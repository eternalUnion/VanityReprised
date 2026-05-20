using AsmResolver;
using AssetRipper.AccurateShaders;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Extensions;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files.BundleFiles;
using AssetRipper.IO.Files.Utils;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;
using AssetsTools.NET;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using EasyCompressor;
using LibCpp2IL;
using Mono.Unix;
using RudeShaderMiddleman.Common.BlobTable;
using RudeShaderMiddleman.Common.Metadata;
using RudeShaderMiddleman.Common.ShaderTable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
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

		private static string GetMD5(string filePath)
		{
			MD5 md5 = MD5.Create();
			using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
			}
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

		record SegmentCompressRequest(MemoryStream segment, MemoryStream compressedSegment, int segmentIndex);

		record SegmentWriteRequest(MemoryStream segment, MemoryStream compressedSegment, int segmentIndex);
		#endregion

		public void DoPostExport(GameData gameData, LibraryConfiguration settings)
		{
			Logger.Info($"Installing accurate shaders for Unity Editor {definition.Editor}");

			if (!CopyMiddleman())
			{
				Logger.Error("Skipping shader processing since copying middleman failed!");
				return;
			}

			if (!alreadyExportedBinaries)
			{
				alreadyExportedBinaries = true;
				MakeBinaries(gameData, settings);
			}
		}

		public bool CopyMiddleman()
		{
			if (!definition.EditorInstalled)
			{
				Logger.Error($"Cannot install accurate shaders for Unity {definition.Editor}, because the editor is not installed!");
				return false;
			}

			if (!definition.CanModify)
			{
				Logger.Error($"Cannot install accurate shaders for Unity {definition.Editor}, because Vanity is not running with elevated privileges!");
				return false;
			}

#if OS_LINUX
			string middlemanPath = "Resources/UnityShaderCompiler";
#else
			string middlemanPath = "Resources/UnityShaderCompiler.exe";
#endif

			if (!File.Exists(middlemanPath))
			{
				Logger.Error($"Cannot install accurate shaders for Unity {definition.Editor}, because the file {middlemanPath} does not exist!");
				return false;
			}

#if OS_LINUX
			string alteredPath = Path.Combine(definition.ToolsPath, "_UnityShaderCompiler");
#else
			string alteredPath = Path.Combine(definition.ToolsPath, "_UnityShaderCompiler.exe");
#endif

			if (File.Exists(alteredPath) && GetMD5(alteredPath) == definition.ShaderCompilerMD5)
			{
				Logger.Info($"Shader compiler already installed, overwriting");

				try
				{
					File.Copy(middlemanPath, definition.ShaderCompilerPath, true);
				}
				catch (Exception ex)
				{
					Logger.Error(ex);
					Logger.Error("Attempting to recover the original compiler name...");

					try
					{
						if (File.Exists(definition.ShaderCompilerPath))
							File.Move(definition.ShaderCompilerPath, Path.Combine(definition.ToolsPath, FileUtils.GetUniqueName(definition.ToolsPath, "__UnityShaderCompiler", 32)));

						File.Move(alteredPath, definition.ShaderCompilerPath);
					}
					catch (Exception innerEx)
					{
						Logger.Error(innerEx);
						Logger.Error("Cannot recover");
						return false;
					}

					return false;
				}

				return true;
			}

			if (!File.Exists(definition.ShaderCompilerPath) || GetMD5(definition.ShaderCompilerPath) != definition.ShaderCompilerMD5)
			{
				Logger.Error("Could not locate the original shader compiler!");
				return false;
			}

			try
			{
				File.Move(definition.ShaderCompilerPath, alteredPath);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				return false;
			}

			try
			{
				File.Copy(middlemanPath, definition.ShaderCompilerPath, false);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);

				if (File.Exists(alteredPath) && GetMD5(alteredPath) == definition.ShaderCompilerMD5)
				{
					Logger.Error("Attempting to move back the original file");

					try
					{
						File.Move(alteredPath, definition.ShaderCompilerPath, true);
					}
					catch (Exception innerEx)
					{
						Logger.Error(innerEx);
						return false;
					}
				}

				return false;
			}

#if OS_LINUX
			try
			{
				var unixFileInfo = new Mono.Unix.UnixFileInfo(definition.ShaderCompilerPath);
				if (unixFileInfo.Exists)
				{
					unixFileInfo.FileAccessPermissions |= FileAccessPermissions.UserExecute;
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
#endif

			return true;
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
			string blobsPath = Path.Combine(settings.ProjectRootPath, "blobs.bin");
			if (File.Exists(blobsPath))
				File.Delete(blobsPath);
			using FileStream blobFile = File.Open(blobsPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			blobFile.Seek(0, SeekOrigin.Begin);
			blobFile.Write(BitConverter.GetBytes(1024 * 1024 / 2));
			blobFile.Seek(4 + 8 * (1024 * 1024 / 2), SeekOrigin.Begin);

			// Thread communication data
			Stack<(MemoryStream segment, MemoryStream decompressedSegment)> segments = new();	// Memory pool
			Stack<SegmentCompressRequest> compressRequests = new();								// Main thread => Compressors
			PriorityQueue<SegmentWriteRequest, int> writeRequests = new();						// Compressors => Writer
			
			Thread writeThread = null;
			List<Thread> compressThreads = new();

			MemoryStream currentSegment = new(MB);
			MemoryStream compressedSegment = new(MB);

			bool terminateWriteThread = false;
			void BlobWriteThread()
			{
				int currentSegmentPosition = 4 + 8 * (1024 * 1024 / 2);
				int nextExpectedSegment = 0;

				while (true)
				{
					SegmentWriteRequest req = null;

					lock (writeRequests)
					{
						// assert (terminateWriteThread && writeRequests.Count == 0) || (writeRequests.Count != 0 && writeRequests.Peek().segmentIndex == nextExpectedSegment)
						while ((!terminateWriteThread || writeRequests.Count != 0) && (writeRequests.Count == 0 || writeRequests.Peek().segmentIndex != nextExpectedSegment))
							Monitor.Wait(writeRequests);

						if (terminateWriteThread && writeRequests.Count == 0)
							return;

						req = writeRequests.Dequeue();
						nextExpectedSegment += 1;
					}

					// Write segment position and size
					blobFile.Seek(4 + 8 * req.segmentIndex, SeekOrigin.Begin);
					blobFile.Write(BitConverter.GetBytes(currentSegmentPosition));
					blobFile.Write(BitConverter.GetBytes(req.compressedSegment.Length));

					// Write the binary data
					blobFile.Seek(currentSegmentPosition, SeekOrigin.Begin);
					req.compressedSegment.Seek(0, SeekOrigin.Begin);
					req.compressedSegment.CopyTo(blobFile);

					// Update the variables
					req.segment.Seek(0, SeekOrigin.Begin);
					req.segment.SetLength(0);
					currentSegmentPosition += (int)req.compressedSegment.Length;

					lock (segments)
					{
						segments.Push((req.segment, req.compressedSegment));
					}
				}
			}

			bool terminateCompressThread = false;
			void BlobCompressThread()
			{
				while (true)
				{
					SegmentCompressRequest req = null;

					lock (compressRequests)
					{
						// assert terminateCompressThread || compressRequests.Count != 0
						while (!terminateCompressThread && compressRequests.Count == 0)
							Monitor.Wait(compressRequests);

						if (terminateCompressThread && compressRequests.Count == 0)
							return;

						req = compressRequests.Pop();
					}

					LZMACompressor compressor = new LZMACompressor(LZMACompressionLevel.Fast);
					req.segment.Seek(0, SeekOrigin.Begin);
					req.compressedSegment.Seek(0, SeekOrigin.Begin);
					req.compressedSegment.SetLength(0);
					compressor.Compress(req.segment, req.compressedSegment);

					lock (writeRequests)
					{
						writeRequests.Enqueue(new SegmentWriteRequest(req.segment, req.compressedSegment, req.segmentIndex), req.segmentIndex);
						Monitor.Pulse(writeRequests);
					}
				}
			}

			int currentSegmentIndex = 0;
			void AddSegment()
			{
				if (currentSegmentIndex % 25 == 0 || currentSegmentIndex == 1838)
					Logger.Info($"Adding segment {currentSegmentIndex}/1838");

				lock (compressRequests)
				{
					compressRequests.Push(new SegmentCompressRequest(currentSegment, compressedSegment, currentSegmentIndex++));
					Monitor.Pulse(compressRequests);
				}

				currentSegment = null;
				compressedSegment = null;

				while (true)
				{
					lock (segments)
					{
						if (segments.Count != 0)
						{
							(currentSegment, compressedSegment) = segments.Pop();
							break;
						}
					}

					// Busy wait
					for (int i = 0; i < 100; i++)
						;
				}
			}

			// Create blob write thread
			writeThread = new Thread(BlobWriteThread);
			writeThread.Start();

			// Create segment compress threads
			for (int i = 0; i < Math.Max(1, Environment.ProcessorCount); i++)
			{
				Thread compressThread = new Thread(BlobCompressThread);
				compressThread.Start();
				compressThreads.Add(compressThread);

				segments.Push((new(MB), new(MB)));
				segments.Push((new(MB), new(MB)));
				segments.Push((new(MB), new(MB)));
			}

			byte[] decompressedParameterBlob = new byte[MB];
			byte[] decompressedBlob = new byte[MB];
			int gcCounter = 0;

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
					Logger.Info($"Skipping {shader.Name} (No GUID)");
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

						if (++gcCounter >= 500)
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

						if (++gcCounter >= 500)
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
				
				if (++gcCounter >= 500)
				{
					GC.Collect();
					gcCounter = 0;
				}
			}

			if (currentSegment.Length != 0)
				AddSegment();

			// Join all threads

			Logger.Info("Waiting for threads to finish...");

			while (true)
			{
				lock (compressRequests)
				{
					if (compressRequests.Count == 0)
					{
						terminateCompressThread = true;
						Monitor.PulseAll(compressRequests);
						break;
					}
				}

				for (int i = 0; i < 100; i++)
					;
			}

			foreach (Thread thread in compressThreads)
				thread.Join();

			lock (writeRequests)
			{
				terminateWriteThread = true;
				Monitor.Pulse(writeRequests);
			}
			writeThread.Join();

			// Write table file

			Logger.Info("Writing table.zip");

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

			Logger.Info("Finished processing shader blob data!");

			if (currentSegment != null)
				currentSegment.Dispose();
			if (compressedSegment != null)
				compressedSegment.Dispose();

			foreach ((MemoryStream segment, MemoryStream compressed) in segments)
			{
				segment.Dispose();
				compressed.Dispose();
			}

			GC.Collect();
		}
	}
}
