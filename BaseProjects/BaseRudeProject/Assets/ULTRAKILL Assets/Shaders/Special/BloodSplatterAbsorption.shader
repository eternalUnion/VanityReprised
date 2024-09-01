Shader "Unlit/BleedSurface" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("NoiseTex", 2D) = "white" {}
		_VisibilityMask ("VisibilityMask", 2D) = "white" {}
	}
	SubShader {
		Pass {
			Blend One One, One One
			Cull Off
			GpuProgramID 15994
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4x4 _RotMat;
			float3 _HitPos;
			float3 _HitNorm;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _NoiseTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                o.position.xy = v.texcoord1.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                o.position.zw = float2(0.0, 1.0);
                o.texcoord.xy = v.texcoord1.xy;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp0 = v.normal.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.normal.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.normal.zzzz + tmp0;
                tmp0.w = dot(tmp0, tmp0);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord2.xyz = tmp0.www * tmp0.xyz;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xz = float2(1.0, 1.0);
                tmp0.w = max(abs(inp.texcoord2.y), 0.2);
                tmp1.x = 1.0 / tmp0.w;
                tmp0.w = tmp0.w - tmp1.x;
                tmp1.yzw = inp.texcoord1.xyz - _HitPos;
                tmp2.x = saturate(-tmp1.z);
                tmp2.x = ceil(tmp2.x);
                tmp0.y = tmp2.x * tmp0.w + tmp1.x;
                tmp0.xyz = tmp0.xyz * tmp1.yzw;
                tmp0.x = dot(tmp0.xyz, tmp0.xyz);
                tmp0.x = tmp0.x * 0.1;
                tmp0.x = log(tmp0.x);
                tmp0.x = tmp0.x * 0.1;
                tmp0.x = exp(tmp0.x);
                tmp0.x = 0.25 / tmp0.x;
                tmp0.x = tmp0.x - 0.25;
                tmp0.yz = tmp1.zz * _RotMat._m01_m11;
                tmp0.yz = _RotMat._m00_m10 * tmp1.yy + tmp0.yz;
                tmp0.yz = _RotMat._m02_m12 * tmp1.ww + tmp0.yz;
                tmp0.yz = tmp0.yz + _RotMat._m03_m13;
                tmp0.yz = saturate(tmp0.yz * float2(0.25, 0.25) + float2(0.5, 0.5));
                tmp0.yz = tmp0.yz * float2(0.05, 0.05) + _Time.yx;
                tmp1 = tex2Dlod(_MainTex, float4(tmp0.yz, 0, 0.0));
                tmp0.x = tmp0.x * tmp1.y;
                tmp0.y = saturate(dot(inp.texcoord2.xyz, -_HitNorm));
                tmp0.y = tmp0.y + 0.5;
                tmp0.y = floor(tmp0.y);
                tmp0.x = tmp0.y * tmp0.x;
                o.sv_target.x = tmp0.x * 0.062745;
                return o;
			}
			ENDCG
		}
		Pass {
			Blend DstColor Zero, DstColor Zero
			GpuProgramID 129297
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                o.sv_target.x = 0.95;
                return o;
			}
			ENDCG
		}
		Pass {
			Blend One One, One One
			GpuProgramID 143196
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _VisibilityMask;
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                o.texcoord.xy = v.texcoord.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.x = inp.texcoord.x;
                tmp0.y = inp.texcoord.y * _ProjectionParams.x;
                tmp0 = tex2D(_VisibilityMask, tmp0.xy);
                tmp1 = tex2D(_MainTex, inp.texcoord.xy);
                o.sv_target.x = saturate(tmp0.x * tmp1.x);
                return o;
			}
			ENDCG
		}
		Pass {
			Blend DstColor Zero, DstColor Zero
			GpuProgramID 209733
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _VisibilityMask;
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                o.texcoord.xy = v.texcoord.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.x = inp.texcoord.x;
                tmp0.y = inp.texcoord.y * _ProjectionParams.x;
                tmp0 = tex2D(_VisibilityMask, tmp0.xy);
                tmp1 = tex2D(_MainTex, inp.texcoord.xy);
                tmp0.x = saturate(tmp0.x * tmp1.x);
                o.sv_target.x = 1.0 - tmp0.x;
                return o;
			}
			ENDCG
		}
		Pass {
			Blend One One, One One
			Cull Off
			GpuProgramID 279639
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float _HitCount;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _NoiseTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                o.position.xy = v.texcoord1.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                o.position.zw = float2(0.0, 1.0);
                o.texcoord.xy = v.texcoord1.xy;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp0 = v.normal.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.normal.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.normal.zzzz + tmp0;
                tmp0.w = dot(tmp0, tmp0);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord2.xyz = tmp0.www * tmp0.xyz;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0.x = max(abs(inp.texcoord2.y), 0.2);
                tmp0.y = 1.0 / tmp0.x;
                tmp0.x = tmp0.x - tmp0.y;
                tmp1.xz = float2(1.0, 1.0);
                tmp0.zw = float2(0.0, 0.0);
                while (true) {
                    tmp1.w = floor(tmp0.w);
                    tmp1.w = tmp1.w >= _HitCount;
                    if (tmp1.w) {
                        break;
                    }
                    tmp2 = ((float4[1])rsc1.Load(tmp0.w))[0];
                    tmp3.xyz = inp.texcoord1.xyz - tmp2.xyz;
                    tmp1.w = saturate(-tmp3.y);
                    tmp1.w = ceil(tmp1.w);
                    tmp1.y = tmp1.w * tmp0.x + tmp0.y;
                    tmp3.xyz = tmp1.xyz * tmp3.xyz;
                    tmp1.y = dot(tmp3.xyz, tmp3.xyz);
                    tmp1.y = -tmp1.y * 5.0 + 1.0;
                    tmp1.y = max(tmp1.y, 0.0);
                    tmp2.xy = inp.texcoord.xy * float2(5.0, 5.0) + tmp2.xy;
                    tmp3 = tex2D(_NoiseTex, tmp2.xy);
                    tmp1.w = saturate(tmp3.x * 5.0 + -2.0);
                    tmp1.y = tmp1.w * tmp1.y;
                    tmp1.w = tmp2.w * 25.0;
                    tmp1.y = tmp1.y / tmp1.w;
                    tmp0.z = tmp0.z + tmp1.y;
                    tmp0.w = tmp0.w + 1;
                }
                o.sv_target.x = saturate(tmp0.z * 50.0);
                return o;
			}
			ENDCG
		}
		Pass {
			GpuProgramID 330884
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                o.texcoord.xy = v.texcoord.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                tmp0 = tex2D(_MainTex, inp.texcoord.xy);
                tmp0.x = saturate(tmp0.x);
                tmp0.x = rsqrt(tmp0.x);
                tmp0.x = 1.0 / tmp0.x;
                tmp0.x = tmp0.x + 0.5;
                o.sv_target.x = floor(tmp0.x);
                return o;
			}
			ENDCG
		}
		Pass {
			Blend One One, One One
			Cull Off
			GpuProgramID 451113
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                o.position.xy = v.texcoord1.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                o.position.zw = float2(0.0, 1.0);
                o.texcoord.xy = v.texcoord1.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                o.sv_target.x = 1.0;
                return o;
			}
			ENDCG
		}
	}
}