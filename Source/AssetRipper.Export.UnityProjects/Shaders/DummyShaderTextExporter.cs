using AssetRipper.AccurateShaders;
using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedPass;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperties;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;
using System.Globalization;

namespace AssetRipper.Export.UnityProjects.Shaders
{
	public sealed class DummyShaderTextExporter : ShaderExporterBase
	{
		private static string FallbackDummyShader { get; } = """

				SubShader{
					Tags { "RenderType" = "Opaque" }
					LOD 200
					CGPROGRAM
			#pragma surface surf Standard fullforwardshadows
			#pragma target 3.0
					sampler2D _MainTex;
					struct Input
					{
						float2 uv_MainTex;
					};
					void surf(Input IN, inout SurfaceOutputStandard o)
					{
						fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
						o.Albedo = c.rgb;
					}
					ENDCG
				}

			""".Replace("\r", "");

		private static string FallbackDummyShaderCode { get; } = """

			            #pragma vertex vert
			            #pragma fragment frag
			            #include "UnityCG.cginc"

			            sampler2D _MainTex;

			            struct appdata
			            {
			                float4 vertex : POSITION;
			                float2 uv : TEXCOORD0;
			            };

			            struct v2f
			            {
			                float4 pos : SV_POSITION;
			                float2 uv : TEXCOORD0;
			            };

			            v2f vert (appdata v)
			            {
			                v2f o;
			                o.pos = UnityObjectToClipPos(v.vertex);
			                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			                return o;
			            }

			            fixed4 frag (v2f i) : SV_Target
			            {
			                fixed4 col = tex2D(_MainTex, i.uv);
			                col.a = 1;
			                return col;
			            }
						ENDCG

			""".Replace("\r", "");

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			using FileStream fileStream = File.Create(path);
			using InvariantStreamWriter writer = new InvariantStreamWriter(fileStream);
			ExportShader((IShader)asset, writer);
			return true;
		}

		public static void ExportShader(IShader shader, TextWriter writer)
		{
			if (shader.Has_ParsedForm())
			{
				if (AccurateShaderDefinition.AtLeastOneExporting && GameData.OriginalGuids.ContainsKey(shader))
				{
					AccurateShaderExport(shader, writer);
					return;
				}

				writer.Write($"Shader \"{shader.ParsedForm.Name}\" {{\n");
				Export(shader.ParsedForm.PropInfo, writer);

				TemplateShader templateShader = TemplateList.GetBestTemplate(shader);
				writer.Write("\t//DummyShaderTextExporter\n");
				if (templateShader != null)
				{
					writer.Write(templateShader.ShaderText);
				}
				else
				{
					writer.WriteIndent(1);
					writer.Write(FallbackDummyShader);
				}
				writer.Write('\n');

				if (shader.ParsedForm.FallbackName != string.Empty)
				{
					writer.WriteIndent(1);
					writer.Write($"Fallback \"{shader.ParsedForm.FallbackName}\"\n");
				}
				if (shader.ParsedForm.CustomEditorName != string.Empty)
				{
					writer.WriteIndent(1);
					writer.Write($"//CustomEditor \"{shader.ParsedForm.CustomEditorName}\"\n");
				}
				writer.Write('}');
			}
			else
			{
				string header = shader.Script.String;
				int subshaderIndex = header.IndexOf("SubShader");
				writer.WriteString(header, 0, subshaderIndex);

				writer.Write("\t//DummyShaderTextExporter\n");
				writer.WriteIndent(1);
				writer.Write(FallbackDummyShader);

				writer.Write('}');
			}
		}

		public static void AccurateShaderExport(IShader shader, TextWriter writer)
		{
			writer.Write($"Shader \"{shader.ParsedForm.Name}\" {{\n");
			Export(shader.ParsedForm.PropInfo, writer);
			writer.Write("\t\n\t// Dummy shader for accurate shaders\n");
			writer.Write("\t//\n");
			writer.Write("\t// If you encounter any problems, delete '<Rude>/Library/ShaderCache' and try opening the project again\n\t\n");

			foreach (var subshader in shader.ParsedForm.SubShaders)
			{
				writer.WriteIndent(1);
				writer.Write("SubShader {\n");

				if (subshader.LOD != 0)
				{
					writer.WriteIndent(2);
					writer.Write("LOD {0}\n", subshader.LOD);
				}

				if (subshader.Tags.Tags.Count != 0)
				{
					writer.WriteIndent(2);
					writer.Write("Tags { ");
					foreach (AssetPair<Utf8String, Utf8String> kvp in subshader.Tags.Tags)
					{
						writer.Write($"\"{kvp.Key}\" = \"{kvp.Value}\" ");
					}
					writer.Write("}\n");
				}

				for (int i = 0; i < subshader.Passes.Count; i++)
				{
					ISerializedPass pass = subshader.Passes[i];
					writer.WriteIndent(2);
					writer.Write($"{((SerializedPassType)pass.Type).ToString()} ");

					if (pass.Type == (int)SerializedPassType.UsePass)
					{
						writer.Write($"\"{pass.UseName}\"\n");
					}
					else
					{
						writer.Write("{\n");

						if (pass.Type == (int)SerializedPassType.GrabPass)
						{
							if (pass.TextureName.Data.Length > 0)
							{
								writer.WriteIndent(3);
								writer.Write($"\"{pass.TextureName}\"\n");
							}
						}
						else if (pass.Type == (int)SerializedPassType.Pass)
						{
							// Commands

							pass.State.Export(writer);
							writer.Write('\n');

							writer.WriteIndent(3);
							writer.Write("CGPROGRAM\n\n");

							// Keywords

							var shaderKeywords = pass.ProgVertex.SerializedKeywordStateMask
								.Concat(pass.ProgFragment.SerializedKeywordStateMask)
								.Distinct()
								.Select(idx => shader.ParsedForm.KeywordNames[idx].String)
								.Except(GameData.GlobalShaderKeywords)
								.Order();

							writer.WriteIndent(3);
							writer.Write("// Shader keywords");
							writer.Write('\n');
							foreach (string keyword in shaderKeywords)
							{
								writer.WriteIndent(3);
								writer.Write($"#pragma shader_feature {keyword}\n");
							}
							writer.Write('\n');

							// Dummy code

							TemplateShader templateShader = TemplateList.GetBestCodeTemplate(shader);
							if (templateShader != null)
							{
								writer.Write(templateShader.ShaderText);
							}
							else
							{
								writer.Write(FallbackDummyShaderCode);
							}

							writer.Write('\n');
						}
						else
						{
							throw new NotSupportedException($"Unsupported pass type {pass.Type}");
						}

						writer.WriteIndent(2);
						writer.Write("}\n");
					}
				}
				writer.WriteIndent(1);
				writer.Write("}\n");
			}

			if (shader.ParsedForm.FallbackName != string.Empty)
			{
				writer.WriteIndent(1);
				writer.Write($"Fallback \"{shader.ParsedForm.FallbackName}\"\n");
			}
			if (shader.ParsedForm.CustomEditorName != string.Empty)
			{
				writer.WriteIndent(1);
				writer.Write($"//CustomEditor \"{shader.ParsedForm.CustomEditorName}\"\n");
			}

			writer.Write("}\n");
			writer.Flush();
		}

		private static void Export(ISerializedProperties _this, TextWriter writer)
		{
			writer.WriteIndent(1);
			writer.Write("Properties {\n");
			foreach (ISerializedProperty prop in _this.Props)
			{
				Export(prop, writer);
			}
			writer.WriteIndent(1);
			writer.Write("}\n");
		}

		private static void Export(ISerializedProperty _this, TextWriter writer)
		{
			writer.WriteIndent(2);
			foreach (Utf8String attribute in _this.Attributes)
			{
				if (attribute.String.StartsWith("Keyword(") && !attribute.String.EndsWith(')'))
					writer.Write($"[{attribute})] ");
				else
					writer.Write($"[{attribute}] ");
			}
			SerializedPropertyFlag flags = (SerializedPropertyFlag)_this.Flags;
			if (flags.IsHideInInspector())
			{
				writer.Write("[HideInInspector] ");
			}
			if (flags.IsPerRendererData())
			{
				writer.Write("[PerRendererData] ");
			}
			if (flags.IsNoScaleOffset())
			{
				writer.Write("[NoScaleOffset] ");
			}
			if (flags.IsNormal())
			{
				writer.Write("[Normal] ");
			}
			if (flags.IsHDR())
			{
				writer.Write("[HDR] ");
			}
			if (flags.IsGamma())
			{
				writer.Write("[Gamma] ");
			}

			writer.Write($"{_this.Name} (\"{_this.Description}\", ");

			switch (_this.GetType_())
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write("Vector");
					break;

				case SerializedPropertyType.Float:
					writer.Write("Float");
					break;

				case SerializedPropertyType.Range:
					writer.Write($"Range({
						_this.DefValue_1_.ToString(CultureInfo.InvariantCulture)}, {
						_this.DefValue_2_.ToString(CultureInfo.InvariantCulture)})");
					break;

				case SerializedPropertyType.Texture:
					switch (_this.DefTexture.TexDim)
					{
						case 1:
							writer.Write("any");
							break;
						case 2:
							writer.Write("2D");
							break;
						case 3:
							writer.Write("3D");
							break;
						case 4:
							writer.Write("Cube");
							break;
						case 5:
							writer.Write("2DArray");
							break;
						case 6:
							writer.Write("CubeArray");
							break;
						default:
							throw new NotSupportedException("Texture dimension isn't supported");

					}
					break;

				case SerializedPropertyType.Int:
					writer.Write("Int");
					break;

				default:
					throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
			}
			writer.Write(") = ");

			switch (_this.GetType_())
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write($"({
						_this.DefValue_0_.ToString(CultureInfo.InvariantCulture)},{
						_this.DefValue_1_.ToString(CultureInfo.InvariantCulture)},{
						_this.DefValue_2_.ToString(CultureInfo.InvariantCulture)},{
						_this.DefValue_3_.ToString(CultureInfo.InvariantCulture)})");
					break;

				case SerializedPropertyType.Float:
				case SerializedPropertyType.Range:
					writer.Write(_this.DefValue_0_.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType.Int:
					writer.Write(((int)_this.DefValue_0_).ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType.Texture:
					writer.Write($"\"{_this.DefTexture.DefaultName}\" {{}}");
					break;

				default:
					throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
			}
			writer.Write('\n');
		}
	}
}
