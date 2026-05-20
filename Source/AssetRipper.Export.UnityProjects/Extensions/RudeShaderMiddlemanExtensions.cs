using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.Bones;
using AssetRipper.SourceGenerated.NativeEnums.Umbra;
using AssetRipper.SourceGenerated.Subclasses.BufferBinding;
using AssetRipper.SourceGenerated.Subclasses.ConstantBuffer;
using AssetRipper.SourceGenerated.Subclasses.SerializedPass;
using AssetRipper.SourceGenerated.Subclasses.SerializedProgramParameters;
using AssetRipper.SourceGenerated.Subclasses.TextureParameter;
using AssetRipper.SourceGenerated.Subclasses.TextureParameters;
using AssetsTools.NET;
using RudeShaderMiddleman.Common.Metadata;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ConstantBuffer = RudeShaderMiddleman.Common.Metadata.ConstantBuffer;
using ShaderParamType = RudeShaderMiddleman.Common.Metadata.ShaderParamType;
using TextureParameter = RudeShaderMiddleman.Common.Metadata.TextureParameter;

namespace AssetRipper.Export.UnityProjects.Extensions;
public class RudeShaderMiddlemanExtensions
{
	public static ShaderParameters ToShaderParameters(SerializedProgramParameters parameters, ISerializedPass pass)
	{
		ShaderParameters res = new ShaderParameters();

		res.BaseConstantBuffer = null;
		res.ConstantBuffers = parameters.ConstantBuffers.Select(cb => GetCB(cb, pass)).ToList();
		res.TextureParameters = parameters.TextureParams.Select(t => GetTextureParam(t, pass)).ToList();
		res.ConstBindings = parameters.ConstantBufferBindings.Select(b => GetCBBinding(b, pass)).ToList();
		res.Buffers = parameters.BufferParams.Select(b => GetCBBinding(b, pass)).ToList();
		res.UAVs = parameters.UAVParams.Select(u => GetUAVParam(u, pass)).ToList();
		res.Samplers = parameters.Samplers.Select(s => GetSamplerParam(s)).ToList();

		return res;
	}

	public static ShaderParameters ToShaderParameters(AssetsFileReader r, UnityVersion engVer, bool readBlobVersion)
	{
		ShaderParameters res = new ShaderParameters();

		if (readBlobVersion)
		{
			var blobVersion = r.ReadInt32();
		}
		var firstParamsCount = r.ReadInt32();
		if (firstParamsCount > 0)
		{
			res.BaseConstantBuffer = GetCB(r, engVer);
			res.ConstantBuffers = new List<ConstantBuffer>(firstParamsCount - 1);
			for (var i = 1; i < firstParamsCount; i++)
			{
				res.ConstantBuffers.Add(GetCB(r, engVer));
			}
		}
		else
		{
			res.ConstantBuffers = new List<ConstantBuffer>(0);
		}

		res.TextureParameters = new List<TextureParameter>();
		res.ConstBindings = new List<ConstantBufferBinding>();
		res.Buffers = new List<ConstantBufferBinding>();
		res.UAVs = new List<UAVParameter>();
		res.Samplers = new List<SamplerParameter>();

		var secondParamsCount = r.ReadInt32();
		for (var i = 0; i < secondParamsCount; i++)
		{
			var name = r.ReadCountStringInt32();
			r.Align();

			var type = r.ReadInt32();

			if (type == 0)
			{
				res.TextureParameters.Add(GetTextureParam(r, engVer, name));
			}
			else if (type == 1)
			{
				res.ConstBindings.Add(GetCBBinding(r, name));
			}
			else if (type == 2)
			{
				res.Buffers.Add(GetCBBinding(r, name));
			}
			else if (type == 3)
			{
				res.UAVs.Add(GetUAVParam(r, name));
			}
			else if (type == 4)
			{
				res.Samplers.Add(GetSamplerParam(r));
			}
		}

		return res;
	}

	#region Native types to entries
	private static ConstantBuffer GetCB(ConstantBuffer_2020_3_2 cb, ISerializedPass pass)
	{
		ConstantBuffer res = new ConstantBuffer();

		res.Name = "unknown";
		foreach (var pair in pass.NameIndices)
		{
			if (pair.Value == cb.NameIndex)
			{
				res.Name = pair.Key;
				break;
			}
		}

		res.UsedSize = cb.Size;
		res.Partial = cb.IsPartialCB;

		res.CBParams = new List<ConstantBufferParameter>();
		foreach (var parameter in cb.VectorParams)
		{
			ConstantBufferParameter vectorParam = new ConstantBufferParameter();

			vectorParam.ParamName = "unknown";
			foreach (var pair in pass.NameIndices)
			{
				if (pair.Value == parameter.NameIndex)
				{
					vectorParam.ParamName = pair.Key;
					break;
				}
			}

			vectorParam.IsMatrix = false;
			vectorParam.Rows = 1;
			vectorParam.Columns = parameter.Dim;
			vectorParam.ArraySize = parameter.ArraySize;
			vectorParam.ParamType = (ShaderParamType)parameter.Type;
			vectorParam.Index = parameter.Index;

			res.CBParams.Add(vectorParam);
		}
		foreach (var parameter in cb.MatrixParams)
		{
			ConstantBufferParameter matrixParam = new ConstantBufferParameter();

			matrixParam.ParamName = "unknown";
			foreach (var pair in pass.NameIndices)
			{
				if (pair.Value == parameter.NameIndex)
				{
					matrixParam.ParamName = pair.Key;
					break;
				}
			}

			matrixParam.IsMatrix = true;
			matrixParam.Rows = parameter.RowCount;
			matrixParam.Columns = parameter.RowCount;
			matrixParam.ArraySize = parameter.ArraySize;
			matrixParam.ParamType = (ShaderParamType)parameter.Type;
			matrixParam.Index = parameter.Index;

			res.CBParams.Add(matrixParam);
		}

		res.StructParams = new List<StructParameter>();
		foreach (var parameter in cb.StructParams)
		{
			res.StructParams.Add(GetStructParam(parameter, pass));
		}

		return res;
	}

	private static ConstantBufferBinding GetCBBinding(BufferBinding_2020_1_0_a14 binding, ISerializedPass pass)
	{
		ConstantBufferBinding res = new ConstantBufferBinding();

		res.Name = "unknown";
		foreach (var pair in pass.NameIndices)
		{
			if (pair.Value == binding.NameIndex)
			{
				res.Name = pair.Key;
				break;
			}
		}

		res.Index = binding.Index;
		res.ArraySize = binding.ArraySize;

		return res;
	}

	private static TextureParameter GetTextureParam(TextureParameter_2017_3 tex, ISerializedPass pass)
	{
		TextureParameter res = new TextureParameter();

		res.Name = "unknown";
		foreach (var pair in pass.NameIndices)
		{
			if (pair.Value == tex.NameIndex)
			{
				res.Name = pair.Key;
				break;
			}
		}

		res.Index = tex.Index;
		res.SamplerIndex = tex.SamplerIndex;
		res.MultiSampled = tex.MultiSampled;
		res.Dim = (byte)tex.Dim;

		return res;
	}

	private static UAVParameter GetUAVParam(AssetRipper.SourceGenerated.Subclasses.UAVParameter.UAVParameter uav, ISerializedPass pass)
	{
		UAVParameter res = new UAVParameter();

		res.Name = "unknown";
		foreach (var pair in pass.NameIndices)
		{
			if (pair.Value == uav.NameIndex)
			{
				res.Name = pair.Key;
				break;
			}
		}

		res.Index = uav.Index;
		res.OriginalIndex = uav.OriginalIndex;

		return res;
	}

	private static SamplerParameter GetSamplerParam(AssetRipper.SourceGenerated.Subclasses.SamplerParameter.SamplerParameter sampler)
	{
		SamplerParameter res = new SamplerParameter();

		res.Sampler = sampler.Sampler;
		res.BindPoint = sampler.BindPoint;

		return res;
	}

	private static StructParameter GetStructParam(AssetRipper.SourceGenerated.Subclasses.StructParameter.StructParameter structParameter, ISerializedPass pass)
	{
		StructParameter res = new StructParameter();

		res.Name = "unknown";
		foreach (var pair in pass.NameIndices)
		{
			if (pair.Value == structParameter.NameIndex)
			{
				res.Name = pair.Key;
				break;
			}
		}

		res.Index = structParameter.Index;
		res.ArraySize = structParameter.ArraySize;
		res.Size = structParameter.StructSize;

		res.CBParams = new List<ConstantBufferParameter>();
		foreach (var parameter in structParameter.VectorMembers)
		{
			ConstantBufferParameter vectorParam = new ConstantBufferParameter();

			vectorParam.ParamName = "unknown";
			foreach (var pair in pass.NameIndices)
			{
				if (pair.Value == parameter.NameIndex)
				{
					vectorParam.ParamName = pair.Key;
					break;
				}
			}

			vectorParam.IsMatrix = false;
			vectorParam.Rows = 1;
			vectorParam.Columns = parameter.Dim;
			vectorParam.ArraySize = parameter.ArraySize;
			vectorParam.ParamType = (ShaderParamType)parameter.Type;
			vectorParam.Index = parameter.Index;

			res.CBParams.Add(vectorParam);
		}
		foreach (var parameter in structParameter.MatrixMembers)
		{
			ConstantBufferParameter matrixParam = new ConstantBufferParameter();

			matrixParam.ParamName = "unknown";
			foreach (var pair in pass.NameIndices)
			{
				if (pair.Value == parameter.NameIndex)
				{
					matrixParam.ParamName = pair.Key;
					break;
				}
			}

			matrixParam.IsMatrix = true;
			matrixParam.Rows = parameter.RowCount;
			matrixParam.Columns = parameter.RowCount;
			matrixParam.ArraySize = parameter.ArraySize;
			matrixParam.ParamType = (ShaderParamType)parameter.Type;
			matrixParam.Index = parameter.Index;

			res.CBParams.Add(matrixParam);
		}

		return res;
	}
	#endregion

	private static ConstantBuffer GetCB(AssetsFileReader r, UnityVersion engVer)
	{
		ConstantBuffer res = new ConstantBuffer();

		res.Name = r.ReadCountStringInt32();
		r.Align();

		res.UsedSize = r.ReadInt32();
		res.Partial = false;

		var paramCount = r.ReadInt32();
		res.CBParams = new List<ConstantBufferParameter>(paramCount);
		for (var i = 0; i < paramCount; i++)
		{
			res.CBParams.Add(GetCBParameter(r));
		}

		bool hasStructParams = engVer.GreaterThanOrEquals(2017, 3);
		if (hasStructParams)
		{
			var structCount = r.ReadInt32();
			res.StructParams = new List<StructParameter>(structCount);
			for (var i = 0; i < structCount; i++)
			{
				res.StructParams.Add(GetStructParam(r));
			}
		}
		else
		{
			res.StructParams = [];
		}

		return res;
	}

	private static ConstantBufferBinding GetCBBinding(AssetsFileReader r, string name)
	{
		ConstantBufferBinding res = new ConstantBufferBinding();

		res.Name = name;
		res.Index = r.ReadInt32();
		res.ArraySize = r.ReadInt32();

		return res;
	}

	private static ConstantBufferParameter GetCBParameter(AssetsFileReader r, string structName = "")
	{
		ConstantBufferParameter res = new ConstantBufferParameter();

		if (structName != "")
			res.ParamName = $"{structName}.{r.ReadCountStringInt32()}";
		else
			res.ParamName = r.ReadCountStringInt32();

		r.Align();

		res.ParamType = (RudeShaderMiddleman.Common.Metadata.ShaderParamType)r.ReadInt32();
		res.Rows = r.ReadInt32();
		res.Columns = r.ReadInt32();
		res.IsMatrix = r.ReadInt32() > 0;
		res.ArraySize = r.ReadInt32();
		res.Index = r.ReadInt32();

		return res;
	}

	private static TextureParameter GetTextureParam(AssetsFileReader r, UnityVersion engVer, string name)
	{
		TextureParameter res = new TextureParameter();

		var index = r.ReadInt32();
		var extraValue = r.ReadInt32();

		res.Name = name;
		res.Index = index;

		var hasNewTextureParams = engVer.GreaterThanOrEquals(2018, 2);
		var hasMultiSampled = engVer.GreaterThanOrEquals(2017, 3);
		if (hasNewTextureParams)
		{
			var textureExtraValue = r.ReadUInt32();
			res.MultiSampled = (textureExtraValue & 1) == 1;
			res.Dim = (byte)(textureExtraValue >> 1);
			res.SamplerIndex = extraValue;
		}
		else if (hasMultiSampled)
		{
			var textureExtraValue = r.ReadUInt32();
			res.MultiSampled = textureExtraValue == 1;
			res.Dim = unchecked((byte)extraValue);
			res.SamplerIndex = extraValue >> 8;
			if (res.SamplerIndex == 0xFFFFFF)
			{
				res.SamplerIndex = -1;
			}
		}
		else
		{
			res.MultiSampled = false;
			res.Dim = unchecked((byte)extraValue);
			res.SamplerIndex = extraValue >> 8;
			if (res.SamplerIndex == 0xFFFFFF)
			{
				res.SamplerIndex = -1;
			}
		}

		return res;
	}

	private static SamplerParameter GetSamplerParam(AssetsFileReader r)
	{
		SamplerParameter res = new SamplerParameter();
		res.BindPoint = r.ReadInt32();
		res.Sampler = r.ReadUInt32();
		return res;
	}

	private static UAVParameter GetUAVParam(AssetsFileReader r, string name)
	{
		UAVParameter res = new UAVParameter();
		res.Name = name;
		res.Index = r.ReadInt32();
		res.OriginalIndex = r.ReadInt32();
		return res;
	}

	private static StructParameter GetStructParam(AssetsFileReader r)
	{
		StructParameter res = new StructParameter();

		res.Name = r.ReadCountStringInt32();
		r.Align();

		res.Index = r.ReadInt32();
		res.ArraySize = r.ReadInt32();
		res.Size = r.ReadInt32();

		var paramCount = r.ReadInt32();
		res.CBParams = new List<ConstantBufferParameter>(paramCount);
		for (var j = 0; j < paramCount; j++)
		{
			res.CBParams.Add(GetCBParameter(r, res.Name));
		}

		return res;
	}
}
