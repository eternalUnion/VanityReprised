Shader "psx/railgun" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_EmissiveColor ("Emissive Color (RGB)", Vector) = (0,0,0,0)
		_EmissiveIntensity ("Emissive Strength", Float) = 1
		_EmissivePosition ("Emissive Position", Range(0, 5)) = 0
		_VertexWarpScale ("Vertex Warping Scalar", Range(0, 10)) = 1
		[Toggle] _Outline ("Assist Outline", Float) = 0
	}
	SubShader {
		LOD 200
		Tags { "RenderType" = "Opaque" }
		Pass {
			LOD 200
			Tags { "RenderType" = "Opaque" }
			Lighting On
			GpuProgramID 12173
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float texcoord5 : TEXCOORD5;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float4 color : COLOR0;
				float4 color1 : COLOR1;
				float3 texcoord8 : TEXCOORD8;
				float3 texcoord9 : TEXCOORD9;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			float4 _Color;
			float4 unity_FogStart;
			float4 unity_FogEnd;
			float _VertexWarping;
			float _VertexWarpScale;
			float _TextureWarping;
			float4 _MeshScale;
			float4 _MeshCenter;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _EmissiveColor;
			float _EmissiveIntensity;
			float _EmissivePosition;
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
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                float4 tmp6;
                float4 tmp7;
                float4 tmp8;
                tmp0.xyz = unity_WorldToObject._m01_m11_m21 * unity_MatrixInvV._m10_m10_m10;
                tmp0.xyz = unity_WorldToObject._m00_m10_m20 * unity_MatrixInvV._m00_m00_m00 + tmp0.xyz;
                tmp0.xyz = unity_WorldToObject._m02_m12_m22 * unity_MatrixInvV._m20_m20_m20 + tmp0.xyz;
                tmp0.xyz = unity_WorldToObject._m03_m13_m23 * unity_MatrixInvV._m30_m30_m30 + tmp0.xyz;
                tmp1.xyz = unity_WorldToObject._m01_m11_m21 * unity_MatrixInvV._m11_m11_m11;
                tmp1.xyz = unity_WorldToObject._m00_m10_m20 * unity_MatrixInvV._m01_m01_m01 + tmp1.xyz;
                tmp1.xyz = unity_WorldToObject._m02_m12_m22 * unity_MatrixInvV._m21_m21_m21 + tmp1.xyz;
                tmp1.xyz = unity_WorldToObject._m03_m13_m23 * unity_MatrixInvV._m31_m31_m31 + tmp1.xyz;
                tmp2.xyz = unity_WorldToObject._m01_m11_m21 * unity_MatrixInvV._m12_m12_m12;
                tmp2.xyz = unity_WorldToObject._m00_m10_m20 * unity_MatrixInvV._m02_m02_m02 + tmp2.xyz;
                tmp2.xyz = unity_WorldToObject._m02_m12_m22 * unity_MatrixInvV._m22_m22_m22 + tmp2.xyz;
                tmp2.xyz = unity_WorldToObject._m03_m13_m23 * unity_MatrixInvV._m32_m32_m32 + tmp2.xyz;
                tmp3.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp3.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp3.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp3.xyz, tmp3.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp3.xyz = tmp0.www * tmp3.xyz;
                tmp4.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp4.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp4.xyz;
                tmp4.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp4.xyz;
                tmp4.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp4.xyz;
                tmp0.w = _MeshCenter.w == 0.0;
                tmp5.xyz = v.vertex.xyz - _MeshCenter.xyz;
                tmp5.xyz = tmp5.xyz / _MeshScale.xyz;
                tmp5.xyz = tmp5.xyz + float3(0.5, 0.5, 0.5);
                tmp6.xyz = tmp4.xyz - _MeshCenter.xyz;
                tmp6.xyz = tmp6.xyz / _MeshScale.xyz;
                tmp6.xyz = tmp6.xyz + float3(0.5, 0.5, 0.5);
                o.texcoord8.xyz = tmp0.www ? tmp5.xyz : tmp6.xyz;
                tmp5 = v.tangent.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp5 = unity_ObjectToWorld._m00_m10_m20_m30 * v.tangent.xxxx + tmp5;
                tmp5 = unity_ObjectToWorld._m02_m12_m22_m32 * v.tangent.zzzz + tmp5;
                tmp0.w = dot(tmp5, tmp5);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord9.xyz = tmp0.www * tmp5.xyz;
                tmp5.xyz = _WorldSpaceCameraPos - tmp4.xyz;
                tmp0.w = dot(tmp5.xyz, tmp5.xyz);
                tmp1.w = sqrt(tmp0.w);
                tmp0.w = rsqrt(tmp0.w);
                tmp5.xyz = tmp0.www * tmp5.xyz;
                tmp0.w = dot(tmp3.xyz, tmp5.xyz);
                tmp0.w = 1.0 - abs(tmp0.w);
                tmp0.w = dot(tmp0.xy, tmp0.xy);
                o.texcoord4.w = min(tmp0.w, 1.0);
                tmp0.w = dot(-tmp5.xyz, tmp3.xyz);
                tmp0.w = tmp0.w + tmp0.w;
                tmp6.xyz = tmp3.xyz * -tmp0.www + -tmp5.xyz;
                tmp0.w = _TextureWarping + _TextureWarping;
                tmp0.w = min(tmp0.w, 1.0);
                tmp2.w = dot(tmp6.xyz, tmp6.xyz);
                tmp2.w = rsqrt(tmp2.w);
                tmp7.xyz = tmp6.xyz * tmp2.www + -tmp6.xyz;
                o.texcoord2.xyz = tmp0.www * tmp7.xyz + tmp6.xyz;
                tmp6 = tmp4.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp6 = unity_MatrixVP._m00_m10_m20_m30 * tmp4.xxxx + tmp6;
                tmp6 = unity_MatrixVP._m02_m12_m22_m32 * tmp4.zzzz + tmp6;
                tmp6 = tmp6 + unity_MatrixVP._m03_m13_m23_m33;
                tmp0.w = _VertexWarping != 0.0;
                tmp7.xyz = tmp6.xyz / tmp6.www;
                tmp2.w = tmp1.w - 200.0;
                tmp2.w = max(tmp2.w, 0.0);
                tmp2.w = log(tmp2.w);
                tmp2.w = tmp2.w * 0.2;
                tmp2.w = exp(tmp2.w);
                tmp2.w = tmp2.w * 0.1 + _VertexWarpScale;
                tmp3.w = max(_ScreenParams.y, _ScreenParams.x);
                tmp8.xy = _ScreenParams.xy / tmp3.ww;
                tmp2.w = tmp2.w * _VertexWarping;
                tmp8.xy = tmp2.ww * tmp8.xy;
                tmp8.zw = tmp7.xy * tmp8.xy;
                tmp8.zw = floor(tmp8.zw);
                tmp8.zw = tmp8.zw + float2(0.5, 0.5);
                tmp7.xy = tmp8.zw / tmp8.xy;
                tmp7.xyz = tmp6.www * tmp7.xyz;
                o.position.xyz = tmp0.www ? tmp7.xyz : tmp6.xyz;
                tmp0.w = min(_TextureWarping, 1.0);
                tmp0.w = tmp0.w * 0.5;
                tmp2.w = max(tmp6.w, 0.02);
                tmp2.w = tmp2.w - 0.5;
                tmp0.w = tmp0.w * tmp2.w + 0.5;
                tmp6.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.texcoord.xy = tmp0.ww * tmp6.xy;
                tmp1.w = unity_FogEnd.x - tmp1.w;
                tmp2.w = unity_FogEnd.x - unity_FogStart.x;
                o.color1.w = saturate(tmp1.w / tmp2.w);
                tmp6.xyz = v.color.xyz * _Color.xyz;
                tmp7 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp7 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp7;
                tmp7 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp7;
                tmp7 = tmp7 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp8.xyz = tmp7.yyy * unity_MatrixV._m01_m11_m21;
                tmp8.xyz = unity_MatrixV._m00_m10_m20 * tmp7.xxx + tmp8.xyz;
                tmp7.xyz = unity_MatrixV._m02_m12_m22 * tmp7.zzz + tmp8.xyz;
                tmp7.xyz = unity_MatrixV._m03_m13_m23 * tmp7.www + tmp7.xyz;
                tmp0.x = dot(tmp0.xyz, v.normal.xyz);
                tmp0.y = dot(tmp1.xyz, v.normal.xyz);
                tmp0.z = dot(tmp2.xyz, v.normal.xyz);
                tmp1.x = dot(tmp0.xyz, tmp0.xyz);
                tmp1.x = rsqrt(tmp1.x);
                tmp0.xyz = tmp0.xyz * tmp1.xxx;
                tmp1.xyz = glstate_lightmodel_ambient.xyz + glstate_lightmodel_ambient.xyz;
                tmp2.xyz = tmp1.xyz;
                tmp1.w = 0.0;
                for (int i = tmp1.w; i < 8; i += 1) {
                    tmp8.xyz = -tmp7.xyz * unity_LightPosition[i].www + unity_LightPosition[i].xyz;
                    tmp2.w = dot(tmp8.xyz, tmp8.xyz);
                    tmp2.w = max(tmp2.w, 0.000001);
                    tmp3.w = rsqrt(tmp2.w);
                    tmp8.xyz = tmp3.www * tmp8.xyz;
                    tmp2.w = tmp2.w * unity_LightAtten[i].z + 1.0;
                    tmp2.w = 1.0 / tmp2.w;
                    tmp3.w = dot(tmp8.xyz, unity_SpotDirection[i].xyz);
                    tmp3.w = max(tmp3.w, 0.0);
                    tmp3.w = tmp3.w - unity_LightAtten[i].x;
                    tmp3.w = saturate(tmp3.w * unity_LightAtten[i].y);
                    tmp2.w = tmp2.w * tmp3.w;
                    tmp3.w = dot(tmp0.xyz, tmp8.xyz);
                    tmp3.w = max(tmp3.w, 0.0);
                    tmp2.w = tmp2.w * tmp3.w;
                    tmp2.xyz = unity_LightColor[i].xyz * tmp2.www + tmp2.xyz;
                }
                o.color.xyz = tmp2.xyz * tmp6.xyz;
                o.position.w = tmp6.w;
                o.texcoord4.xyz = tmp5.xyz;
                o.color.w = v.color.w;
                o.color1.xyz = unity_FogColor.xyz;
                o.texcoord5.x = tmp0.w;
                o.texcoord1.xyz = tmp4.xyz;
                o.texcoord3.xyz = tmp3.xyz;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xy = inp.texcoord.xy / inp.texcoord5.xx;
                tmp0.z = tmp0.x - 4.0;
                tmp1 = tex2D(_MainTex, tmp0.xy);
                tmp1 = tmp1 * inp.color;
                tmp1 = tmp1 * inp.color1.wwww;
                tmp0.x = floor(tmp0.z);
                tmp0.x = saturate(tmp0.x + _EmissivePosition);
                tmp0.x = tmp0.x * _EmissiveIntensity;
                tmp0.x = saturate(tmp1.w * tmp0.x);
                tmp0.y = 1.0 - inp.color1.w;
                tmp0.yzw = inp.color1.xyz * tmp0.yyy + tmp1.xyz;
                o.sv_target.xyz = _EmissiveColor.xyz * tmp0.xxx + tmp0.yzw;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
	}
}