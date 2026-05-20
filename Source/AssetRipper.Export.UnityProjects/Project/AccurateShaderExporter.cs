using AsmResolver;
using AssetRipper.AccurateShaders;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Extensions;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files.BundleFiles;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;
using AssetsTools.NET;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using EasyCompressor;
using LibCpp2IL;
using RudeShaderMiddleman.Common.BlobTable;
using RudeShaderMiddleman.Common.Metadata;
using RudeShaderMiddleman.Common.ShaderTable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ICSharpCode.Decompiler.SingleFileBundle;

namespace AssetRipper.Export.UnityProjects.Project
{
	#region Extension methods for binary file generation
	static class BinaryReaderExtensions
	{
		public static string ReadCountString(this BinaryReader reader)
		{
			int len = reader.ReadInt32();
			return Encoding.UTF8.GetString(reader.ReadBytes(len));
		}

		public static void Align(this BinaryReader reader)
		{
			long pad = 4 - (reader.BaseStream.Position % 4);
			if (pad != 4) reader.BaseStream.Position += pad;
		}
	}
	#endregion

	internal class AccurateShaderExporter : IPostExporter
	{
		public static bool alreadyExportedBinaries = false;
		public readonly AccurateShaderDefinition definition;

		public AccurateShaderExporter(AccurateShaderDefinition definition)
		{
			this.definition = definition;
		}

		#region Structures for binary file generation
		struct TableEntry
		{
			public int Offset;
			public int Length;
			public int Segment;
			public bool IsParameterEntry;

			public TableEntry(int offset, int length, int segment)
			{
				Offset = offset;
				Length = length;
				Segment = segment;
			}
		}

		enum ShaderPlatformProgramType
		{
			Unknown = -1,
			Dx11_Vertex,
			Dx11_Pixel,
			Glcore,
			Vulkan,
		}
		#endregion

		public void DoPostExport(GameData gameData, LibraryConfiguration settings)
		{
			Logger.Info($"Installing accurate shaders for Unity Editor {definition.Editor}");

			if (!alreadyExportedBinaries)
			{
				MakeBinaries(gameData, settings);
				alreadyExportedBinaries = true;
			}
		}

		public void MakeBinaries(GameData gameData, LibraryConfiguration settings)
		{
			Logger.Info($"Generating shader binary data");

			UnityVersion UNITY_VERSION = UnityVersion.Parse(definition.Editor);
			const int MB = 1 * 1024 * 1024;
			
			// Shader table data
			Dictionary<string, ShaderEntry> entries = new();
			Dictionary<(string, int, ShaderPlatformProgramType), List<VariantEntry>> variantMap = new();

			// Blob binary data
			using MemoryStream currentSegment = new(MB);
			using MemoryStream compressedSegment = new(MB);
			string blobsPath = Path.Combine(settings.ProjectRootPath, "blobs.bin");
			if (File.Exists(blobsPath))
				File.Delete(blobsPath);
			using FileStream blobFile = File.Open(blobsPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			int currentSegmentIndex = 0;
			int currentSegmentPosition = 4 + 8 * (1024 * 1024 / 2);
			blobFile.Seek(0, SeekOrigin.Begin);
			blobFile.Write(BitConverter.GetBytes(1024 * 1024 / 2));
			blobFile.Seek(4 + 8 * (1024 * 1024 / 2), SeekOrigin.Begin);

			byte[] decompressedParameterBlob = new byte[MB];
			byte[] decompressedBlob = new byte[MB];

			void AddSegment()
			{
				LZMACompressor compressor = new LZMACompressor(LZMACompressionLevel.Fast);
				currentSegment.Seek(0, SeekOrigin.Begin);
				compressedSegment.Seek(0, SeekOrigin.Begin);
				compressedSegment.SetLength(0);
				compressor.Compress(currentSegment, compressedSegment);

				// Write segment position and size
				blobFile.Seek(4 + 8 * currentSegmentIndex, SeekOrigin.Begin);
				blobFile.Write(BitConverter.GetBytes(currentSegmentPosition));
				blobFile.Write(BitConverter.GetBytes(compressedSegment.Length));

				// Write the binary data
				blobFile.Seek(currentSegmentPosition, SeekOrigin.Begin);
				compressedSegment.Seek(0, SeekOrigin.Begin);
				compressedSegment.CopyTo(blobFile);

				// Update the variables
				currentSegment.Seek(0, SeekOrigin.Begin);
				currentSegment.SetLength(0);
				currentSegmentIndex += 1;
				currentSegmentPosition += (int)compressedSegment.Length;
			}

			//foreach (var asset in gameData.GameBundle.Bundles[0].Collections[0].Assets.Values)
			foreach (var asset in gameData.GameBundle.Bundles.SelectMany(b => b.Collections.SelectMany(c => c.Assets.Values)))
			{
				if (asset is not IShader shader)
					continue;

				var parsed = shader.ParsedForm;
				if (parsed == null)
					continue;

				if (parsed.SubShaders.Count != 1)
				{
					Logger.Warning($"Skipping {shader.Name} (multiple sub shaders)");
					continue;
				}

				using var compStream = new MemoryStream(shader.CompressedBlob);
				(int, int) decompressedParameterBlobCache = (-1, -1);
				(int, int) decompressedBlobCache = (-1, -1);

				byte[] ReadBlob(int i, int j, ref byte[] buff, ref (int, int) cache)
				{
					if (cache.Item1 == i && cache.Item2 == j)
						return buff;

					cache.Item1 = i;
					cache.Item2 = j;

					int decompressedLength = (int)shader.DecompressedLengths_AssetList_AssetList_UInt32[i][j];
					int newLength = buff.Length;
					while (newLength < decompressedLength)
						newLength *= 2;

					if (newLength != buff.Length)
						buff = new byte[newLength];

					using var segStream = new SegmentStream(compStream, shader.Offsets_AssetList_AssetList_UInt32[i][j], shader.CompressedLengths_AssetList_AssetList_UInt32[i][j]);
					using Lz4DecoderStream decoder = new Lz4DecoderStream(segStream);
					decoder.Read(buff, 0, decompressedLength);
					return buff;
				}

				// Create segment table

				List<TableEntry>[] blobIndex = new List<TableEntry>[shader.CompressedLengths_AssetList_AssetList_UInt32.Count];

				for (int i = 0; i < blobIndex.Length; i++)
				{
					ReadBlob(i, 0, ref decompressedBlob, ref decompressedBlobCache);
					BinaryReader reader = new BinaryReader(new MemoryStream(decompressedBlob, false));

					int entryCount = reader.ReadInt32();
					blobIndex[i] = new List<TableEntry>(entryCount);

					for (int j = 0; j < entryCount; j++)
					{
						int Offset = reader.ReadInt32();
						int Length = reader.ReadInt32();
						int segment = reader.ReadInt32();

						blobIndex[i].Add(new TableEntry(Offset, Length, segment));
					}
				}

				// Read and process all passes

				ShaderEntry entry = new ShaderEntry();
				if (!GameData.OriginalGuids.TryGetValue(shader, out string guid))
				{
					Logger.Warning($"Skipping {shader.Name}. No GUID.");
					continue;
				}

				if (shader.OriginalName == "ULTRAKILL-Standard")
				{
					Logger.Info($"Processing ULTRAKILL-Standard (this will take some time...)");
				}
				else
				{
					Logger.Info($"Processing shader {shader.OriginalName}");
				}

				entry.shaderKeywords = parsed.KeywordNames.Select(k => k.String).ToList();
				entry.shaderPasses = new List<ShaderPass>();

				for (int passNum = 0; passNum < parsed.SubShaders[0].Passes.Count; passNum++)
				{
					var pass = parsed.SubShaders[0].Passes[passNum];
					var state = pass.State;

					#region ASSUMPTIONS
					if (!string.IsNullOrEmpty(pass.UseName.String) || !string.IsNullOrEmpty(pass.TextureName.String))
					{
						Logger.Info($"Skipping {shader.Name} (using named pass)");
						continue;
					}

					if (pass.ProgVertex.PlayerSubPrograms.Count != 4)
					{
						Logger.Info($"Skipping {shader.Name} pass {passNum} (incorrect vertex program count)");
						continue;
					}

					if (pass.ProgFragment.PlayerSubPrograms.Count != 4)
					{
						Logger.Info($"Skipping {shader.Name} pass {passNum} (incorrect fragment program count)");
						continue;
					}

					if (pass.ProgVertex.PlayerSubPrograms[0].Count != 0)
					{
						Logger.Info($"Skipping {shader.Name} pass {passNum} (incorrect vertex program structure)");
						continue;
					}

					if (pass.ProgVertex.PlayerSubPrograms[1].Count != 0)
					{
						Logger.Info($"Skipping {shader.Name} pass {passNum} (incorrect vertex program structure)");
						continue;
					}

					if (pass.ProgVertex.PlayerSubPrograms[2].Count != 0)
					{
						Logger.Info($"Skipping {shader.Name} pass {passNum} (incorrect vertex program structure)");
						continue;
					}

					if (pass.ProgFragment.PlayerSubPrograms[0].Count != 0)
					{
						Logger.Info($"Skipping {shader.Name} pass {passNum} (incorrect fragment program structure)");
						continue;
					}

					if (pass.ProgFragment.PlayerSubPrograms[1].Count != 0)
					{
						Logger.Info($"Skipping {shader.Name} pass {passNum} (incorrect fragment program structure)");
						continue;
					}

					if (pass.ProgFragment.PlayerSubPrograms[2].Count != 0)
					{
						Logger.Info($"Skipping {shader.Name} pass {passNum} (incorrect fragment program structure)");
						continue;
					}
					#endregion

					variantMap[(guid, passNum, ShaderPlatformProgramType.Dx11_Vertex)] = new();
					variantMap[(guid, passNum, ShaderPlatformProgramType.Dx11_Pixel)] = new();
					variantMap[(guid, passNum, ShaderPlatformProgramType.Glcore)] = new();

					ShaderPass passEntry = new ShaderPass();
					passEntry.passNum = passNum;

					var vertexShader = pass.ProgVertex;
					var vertexPrograms = vertexShader.PlayerSubPrograms[3];
					var vertexCommonParams = vertexShader.CommonParameters;
					var vertexParams = vertexShader.ParameterBlobIndices[3];

					var fragmentShader = pass.ProgFragment;
					var fragmentPrograms = fragmentShader.PlayerSubPrograms[3];
					var fragmentCommonParams = fragmentShader.CommonParameters;
					var fragmentParams = fragmentShader.ParameterBlobIndices[3];

					if (!string.IsNullOrEmpty(pass.Name.String))
						;

					passEntry.vertexCommonParameters = RudeShaderMiddlemanExtensions.ToShaderParameters(vertexCommonParams, pass);
					passEntry.fragmentCommonParameters = RudeShaderMiddlemanExtensions.ToShaderParameters(fragmentCommonParams, pass);

					// Shader entry

					passEntry.vertexKeywordMask = 0;
					passEntry.fragmentKeywordMask = 0;

					foreach (var vertKeyword in vertexShader.SerializedKeywordStateMask)
						passEntry.vertexKeywordMask |= (long)1 << vertKeyword;

					foreach (var fragKeyword in fragmentShader.SerializedKeywordStateMask)
						passEntry.fragmentKeywordMask |= (long)1 << fragKeyword;

					static bool IsValidGpuType(ShaderGpuProgramType p)
					{
						return ((int)p >= 15 && (int)p <= 18) // DX11
							|| ((int)p == 6)                  // GLCore32
							|| ((int)p == 7)                  // GLCore41
							|| ((int)p == 8)                  // GLCore43
							|| ((int)p == 25);                // Vulkan;
					}

					static bool IsVulkan(ShaderGpuProgramType p) => (int)p == 25;

					static bool IsGlCore(ShaderGpuProgramType p) => (int)p >= 6 && (int)p <= 8;

					static ShaderPlatformProgramType ProgramPlatformIndex(ShaderGpuProgramType p)
					{
						switch (p)
						{
							case ShaderGpuProgramType.DX11VertexSM40:
							case ShaderGpuProgramType.DX11VertexSM50:
								return ShaderPlatformProgramType.Dx11_Vertex;

							case ShaderGpuProgramType.DX11PixelSM40:
							case ShaderGpuProgramType.DX11PixelSM50:
								return ShaderPlatformProgramType.Dx11_Pixel;

							case ShaderGpuProgramType.GLCore32:
							case ShaderGpuProgramType.GLCore41:
							case ShaderGpuProgramType.GLCore43:
								return ShaderPlatformProgramType.Glcore;

							case ShaderGpuProgramType.SPIRV:
								return ShaderPlatformProgramType.Vulkan;

							default:
								return ShaderPlatformProgramType.Unknown;
						}
					}

					int gcCounter = 0;

					// Process vertex shaders

					foreach ((var vertexProg, var progParamBlobIndex) in Enumerable.Zip(vertexPrograms, vertexParams)/*.OrderBy(pair => ProgramPlatformIndex((ShaderGpuProgramType)pair.First.GpuProgramType))*/)
					{
						if (!IsValidGpuType((ShaderGpuProgramType)vertexProg.GpuProgramType))
							throw new Exception("Unsupported program type");

						if (IsVulkan((ShaderGpuProgramType)vertexProg.GpuProgramType))
							continue;

						// Find correct blob
						var platrofms = pass.Platforms;
						int platrofmIdx = (int)((ShaderGpuProgramType)vertexProg.GpuProgramType).ToGPUPlatform(IO.Files.BuildTarget.StandaloneWin64Player);
						int vertexBlob = platrofms.IndexOf((byte)platrofmIdx);

						TableEntry blobEntry = blobIndex[vertexBlob][(int)vertexProg.BlobIndex];

						TableEntry parameterEntry = blobIndex[vertexBlob][(int)progParamBlobIndex];
						ShaderParameters parameters = RudeShaderMiddlemanExtensions.ToShaderParameters(new AssetsFileReader(new MemoryStream(ReadBlob(vertexBlob, parameterEntry.Segment, ref decompressedParameterBlob, ref decompressedParameterBlobCache), parameterEntry.Offset, parameterEntry.Length, false)), UNITY_VERSION, true);

						int statsALU;
						int statsTex;
						int statsFlow;
						int statsTempRegister;

						int segmentIndex;
						int segmentOffset;
						int segmentLength;

						int sourceMap;
						List<BindChannel> bindings = new List<BindChannel>();

						byte[] shaderSegment = ReadBlob(vertexBlob, blobEntry.Segment, ref decompressedBlob, ref decompressedBlobCache);
						using (BinaryReader shaderReader = new BinaryReader(new MemoryStream(shaderSegment, blobEntry.Offset, blobEntry.Length, false)))
						{
							int blobVersion = shaderReader.ReadInt32();
							int shaderVersion = shaderReader.ReadInt32();

							statsALU = shaderReader.ReadInt32();
							statsTex = shaderReader.ReadInt32();
							statsFlow = shaderReader.ReadInt32();
							statsTempRegister = shaderReader.ReadInt32();

							int globalKeywordCount = shaderReader.ReadInt32();
							for (int i = 0; i < globalKeywordCount; i++)
							{
								string keyword = shaderReader.ReadCountString();
								shaderReader.Align();
							}

							int programDataSize = shaderReader.ReadInt32();
							byte[] programData = shaderReader.ReadBytes(programDataSize);
							shaderReader.Align();

							if (programData.Length + currentSegment.Length >= MB)
								AddSegment();

							segmentIndex = currentSegmentIndex;
							segmentOffset = (int)currentSegment.Length;
							segmentLength = programData.Length;
							currentSegment.Write(programData);

							sourceMap = shaderReader.ReadInt32();
							int bindingCount = shaderReader.ReadInt32();
							for (int i = 0; i < bindingCount; i++)
							{
								int source = shaderReader.ReadInt32();
								int target = shaderReader.ReadInt32();
								bindings.Add(new BindChannel(source, target));
							}

							if (shaderReader.BaseStream.Position != shaderReader.BaseStream.Length)
								throw new Exception("Invalid");
						}

						long keywordMask = 0;
						foreach (var keywordIdx in vertexProg.KeywordIndices)
						{
							keywordMask |= (long)1 << keywordIdx;
						}

						VariantEntry varEntry = new VariantEntry();
						varEntry.type = vertexProg.GpuProgramType;
						varEntry.statsAlu = statsALU;
						varEntry.statsTex = statsTex;
						varEntry.statsFlow = statsFlow;
						varEntry.statsTempRegister = statsTempRegister;
						varEntry.segment = segmentIndex;
						varEntry.offset = segmentOffset;
						varEntry.length = segmentLength;
						varEntry.sourceMap = sourceMap;
						varEntry.inputBindings = bindings;
						varEntry.parameters = parameters;
						varEntry.keywords = keywordMask;

						variantMap[(guid, passNum, ProgramPlatformIndex((ShaderGpuProgramType)vertexProg.GpuProgramType))].Add(varEntry);

						if (++gcCounter >= 100)
						{
							GC.Collect();
							gcCounter = 0;
						}
					}

					// Process fragment shaders

					foreach ((var fragmentProg, var progParamBlobIndex) in Enumerable.Zip(fragmentPrograms, fragmentParams)/*.OrderBy(pair => ProgramPlatformIndex((ShaderGpuProgramType)pair.First.GpuProgramType))*/)
					{
						if (!IsValidGpuType((ShaderGpuProgramType)fragmentProg.GpuProgramType))
							throw new Exception("Unsupported program type");
						
						var platforms = pass.Platforms;
						int platrofmIdx = (int)((ShaderGpuProgramType)fragmentProg.GpuProgramType).ToGPUPlatform(IO.Files.BuildTarget.StandaloneWin64Player);
						int fragmentBlob = platforms.IndexOf((byte)platrofmIdx);

						TableEntry blobEntry = blobIndex[fragmentBlob][(int)fragmentProg.BlobIndex];

						TableEntry parameterEntry = blobIndex[fragmentBlob][(int)progParamBlobIndex];
						ShaderParameters parameters = RudeShaderMiddlemanExtensions.ToShaderParameters(new AssetsFileReader(new MemoryStream(ReadBlob(fragmentBlob, parameterEntry.Segment, ref decompressedParameterBlob, ref decompressedParameterBlobCache), parameterEntry.Offset, parameterEntry.Length, false)), UNITY_VERSION, true);

						int statsALU;
						int statsTex;
						int statsFlow;
						int statsTempRegister;

						int segmentIndex;
						int segmentOffset;
						int segmentLength;

						int sourceMap;
						List<BindChannel> bindings = new List<BindChannel>();

						byte[] shaderSegment = ReadBlob(fragmentBlob, blobEntry.Segment, ref decompressedBlob, ref decompressedBlobCache);
						using (BinaryReader shaderReader = new BinaryReader(new MemoryStream(shaderSegment, blobEntry.Offset, blobEntry.Length, false)))
						{
							int blobVersion = shaderReader.ReadInt32();
							int shaderVersion = shaderReader.ReadInt32();

							statsALU = shaderReader.ReadInt32();
							statsTex = shaderReader.ReadInt32();
							statsFlow = shaderReader.ReadInt32();
							statsTempRegister = shaderReader.ReadInt32();

							List<string> keywords = new List<string>();
							int globalKeywordCount = shaderReader.ReadInt32();
							for (int i = 0; i < globalKeywordCount; i++)
							{
								string keyword = shaderReader.ReadCountString();
								keywords.Add(keyword);
								shaderReader.Align();
							}

							var actualKeywords = fragmentProg.KeywordIndices;

							int programDataSize = shaderReader.ReadInt32();
							byte[] programData = shaderReader.ReadBytes(programDataSize);
							shaderReader.Align();

							if (programData.Length + currentSegment.Length >= MB)
								AddSegment();

							segmentIndex = currentSegmentIndex;
							segmentOffset = (int)currentSegment.Length;
							segmentLength = programData.Length;
							currentSegment.Write(programData);

							sourceMap = shaderReader.ReadInt32();
							int bindingCount = shaderReader.ReadInt32();
							for (int i = 0; i < bindingCount; i++)
							{
								int source = shaderReader.ReadInt32();
								int target = shaderReader.ReadInt32();
								bindings.Add(new BindChannel(source, target));
							}

							if (shaderReader.BaseStream.Position != shaderReader.BaseStream.Length)
								throw new Exception("Invalid");
						}

						long keywordMask = 0;
						foreach (var keywordIdx in fragmentProg.KeywordIndices)
						{
							keywordMask |= (long)1 << keywordIdx;
						}

						VariantEntry varEntry = new VariantEntry();
						varEntry.type = fragmentProg.GpuProgramType;
						varEntry.statsAlu = statsALU;
						varEntry.statsTex = statsTex;
						varEntry.statsFlow = statsFlow;
						varEntry.statsTempRegister = statsTempRegister;
						varEntry.segment = segmentIndex;
						varEntry.offset = segmentOffset;
						varEntry.length = segmentLength;
						varEntry.sourceMap = sourceMap;
						varEntry.inputBindings = bindings;
						varEntry.parameters = parameters;
						varEntry.keywords = keywordMask;

						variantMap[(guid, passNum, ProgramPlatformIndex((ShaderGpuProgramType)fragmentProg.GpuProgramType))].Add(varEntry);

						if (++gcCounter >= 100)
						{
							GC.Collect();
							gcCounter = 0;
						}
					}

					entry.shaderPasses.Add(passEntry);
				}

				if (entry.shaderPasses.Count == 0)
					continue;

				entries[guid] = entry;
				GC.Collect();
			}

			if (currentSegment.Length != 0)
				AddSegment();

			// Write table file

			string tablePath = Path.Combine(settings.ProjectRootPath, "table.zip");

			using (ZipArchive archive = new ZipArchive(File.Open(tablePath, FileMode.OpenOrCreate, FileAccess.Write), ZipArchiveMode.Create))
			{
				ShaderTableFile file = new ShaderTableFile(archive);
				foreach (var pair in entries)
					file.AddShaderEntry(pair.Key, pair.Value);

				foreach (((string guid, int pass, ShaderPlatformProgramType programType), List<VariantEntry> varEntries) in variantMap)
				{
					ZipArchiveEntry variantEntry;
					bool encounteredDx11Vertex = false;
					bool encounteredDx11Fragment = false;

					switch (programType)
					{
						case ShaderPlatformProgramType.Dx11_Vertex:
							if (encounteredDx11Vertex)
								continue;
							encounteredDx11Vertex = true;
							variantEntry = archive.CreateEntry($"{guid}/pass_{pass}/dx11_vert.bin");
							break;

						case ShaderPlatformProgramType.Dx11_Pixel:
							if (encounteredDx11Fragment)
								continue;
							encounteredDx11Fragment = true;
							variantEntry = archive.CreateEntry($"{guid}/pass_{pass}/dx11_frag.bin");
							break;

						case ShaderPlatformProgramType.Glcore:
							variantEntry = archive.CreateEntry($"{guid}/pass_{pass}/glcore.bin");
							break;

						case ShaderPlatformProgramType.Vulkan:
							variantEntry = archive.CreateEntry($"{guid}/pass_{pass}/vulkan.bin");
							break;

						default:
							throw new Exception("Invalid variant");
					}

					using (BinaryWriter writer = new BinaryWriter(variantEntry.Open()))
					{
						writer.Write(varEntries.Count);
						foreach (var entry in varEntries)
						{
							entry.Serialize(writer, file.shaderTable.nameMap);
						}
					}
				}

				file.RewriteShaderTable();
			}

			GC.Collect();
		}
	}
}
