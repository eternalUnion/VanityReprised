Shader "psx/vertexlit/radialdistortion" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_VertexWarpScale ("Vertex Warping Scalar", Range(0, 10)) = 1
		_VertexNoise ("Vertex Noise Texture Lookup", 2D) = "black" {}
		_VertexNoiseScale ("Vertex Distortion Density", Range(0, 10)) = 1
		_VertexNoiseSpeed ("Vertex Distortion Speed", Range(0, 10)) = 1
		_VertexNoiseAmplitude ("Vertex Distortion Amplitude", Range(0, 10)) = 1
		_VertexScale ("Vertex Inflation Scale", Range(0, 1)) = 0
		_FlowDirection ("Vertex Distortion Flow Direction (Normalized XYZ)", Vector) = (0,1,0,1)
		[Toggle] _LocalOffset ("Use Local Space Offset", Float) = 0
		[Toggle] _Outline ("Assist Outline", Float) = 0
	}
	SubShader {
		LOD 200
		Tags { "LIGHTMODE" = "Vertex" "RenderType" = "Opaque" }
		Pass {
			LOD 200
			Tags { "LIGHTMODE" = "Vertex" "RenderType" = "Opaque" }
			Lighting On
			GpuProgramID 7111
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
				float2 sv_target1 : SV_Target1;
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
			float4 _FlowDirection;
			float _VertexNoiseScale;
			float _VertexNoiseSpeed;
			float _VertexNoiseAmplitude;
			float _LocalOffset;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _MainTex_TexelSize;
			float _Outline;
			float _ShouldForceOutlines;
			float _ForceOutline;
			float _HasSandBuff;
			float _OiledAmount;
			float _ScrollMainTextureX;
			float _ScrollMainTextureY;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			sampler2D _VertexNoise;
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _SandTex;
			sampler2D _OilSlick;
			
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
                tmp5.xyz = max(_FlowDirection.xyz, float3(0.0, 0.0, 0.0));
                tmp0.w = dot(tmp5.xyz, tmp5.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp5.xyz = tmp0.www * tmp5.xyz;
                tmp0.w = _VertexNoiseSpeed * _Time.x;
                tmp1.w = _LocalOffset == 1.0;
                tmp6.xyz = tmp4.xyz - unity_ObjectToWorld._m03_m13_m23;
                tmp2.w = dot(tmp6.xyz, tmp5.xyz);
                tmp2.w = tmp2.w * _VertexNoiseScale;
                tmp2.w = tmp2.w * 0.1 + tmp0.w;
                tmp3.w = dot(tmp4.xyz, tmp5.xyz);
                tmp3.w = tmp3.w * _VertexNoiseScale;
                tmp0.w = tmp3.w * 0.1 + tmp0.w;
                tmp0.w = tmp1.w ? tmp2.w : tmp0.w;
                tmp6 = tex2Dgrad(_VertexNoise, tmp0.ww, 0, 0);
                tmp0.w = tmp6.x - 0.5;
                tmp0.w = tmp0.w * _VertexNoiseAmplitude;
                tmp4.xyz = tmp5.xyz * tmp0.www + tmp4.xyz;
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
			fout frag(v2f inp, float facing: VFACE)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                tmp0.xy = inp.texcoord.xy / inp.texcoord5.xx;
                tmp1.x = _ScrollMainTextureX * _Time.y;
                tmp1.y = _ScrollMainTextureY * _Time.y;
                tmp0.xy = tmp0.xy + tmp1.xy;
                tmp1 = tex2D(_MainTex, tmp0.xy);
                o.sv_target.w = tmp1.w * inp.color.w;
                tmp1.xyz = tmp1.xyz * inp.color.xyz + -inp.color1.xyz;
                tmp1.xyz = inp.color1.www * tmp1.xyz + inp.color1.xyz;
                tmp0.z = _HasSandBuff > 0.0;
                if (tmp0.z) {
                    tmp0.z = dot(inp.texcoord3.xyz, inp.texcoord3.xyz);
                    tmp0.z = rsqrt(tmp0.z);
                    tmp2.xyz = tmp0.zzz * inp.texcoord3.xyz;
                    tmp3 = _Time * float4(0.1, 0.5, 0.5, 0.1) + inp.texcoord1.xyyz;
                    tmp0.zw = max(abs(tmp2.zy), abs(tmp2.xx));
                    tmp0.zw = tmp0.zw < abs(tmp2.yz);
                    tmp2.xyz = tmp0.zzz ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                    tmp2.xyz = tmp0.www ? float3(0.0, 0.0, 1.0) : tmp2.xyz;
                    tmp4 = tmp2.zzxx * tmp3;
                    tmp0.zw = tmp4.zw + tmp4.xy;
                    tmp0.zw = tmp3.xw * tmp2.yy + tmp0.zw;
                    tmp0.zw = tmp0.zw * float2(0.25, 0.25);
                    tmp2 = tex2D(_MainTex, tmp0.zw);
                    tmp0.z = rsqrt(inp.texcoord4.w);
                    tmp0.z = 1.0 / tmp0.z;
                    tmp0.w = tmp0.z * 0.75 + 0.25;
                    tmp3.xyz = tmp2.xyz - tmp1.xyz;
                    tmp3.xyz = tmp0.www * tmp3.xyz + tmp1.xyz;
                    tmp2.xyz = tmp0.zzz * tmp2.xyz;
                    tmp1.xyz = tmp2.xyz * float3(0.75, 0.75, 0.75) + tmp3.xyz;
                }
                tmp0.z = _OiledAmount > 0.0;
                if (tmp0.z) {
                    tmp0.xy = tmp0.xy * _MainTex_TexelSize.zw;
                    tmp0.xy = tmp0.xy * float2(0.0078125, 0.0078125);
                    tmp0 = tex2D(_MainTex, tmp0.xy);
                    tmp0.x = saturate(tmp0.x);
                    tmp0.x = tmp0.x * 2.0 + -1.5;
                    tmp0.y = dot(inp.texcoord9.xyz, inp.texcoord9.xyz);
                    tmp0.y = rsqrt(tmp0.y);
                    tmp0.yzw = tmp0.yyy * inp.texcoord9.xyz;
                    tmp1.w = dot(-inp.texcoord4.xyz, tmp0.xyz);
                    tmp1.w = tmp1.w + tmp1.w;
                    tmp2.xy = tmp0.yz * -tmp1.ww + -inp.texcoord4.xy;
                    tmp0.y = dot(tmp0.xyz, inp.texcoord4.xyz);
                    tmp0.y = 1.0 - abs(tmp0.y);
                    tmp0.zw = tmp2.xy * float2(0.05, 0.05);
                    tmp2 = tex2D(_MainTex, tmp0.zw);
                    tmp0.z = tmp2.x * 5.0;
                    tmp0.w = tmp2.x * 0.01 + tmp0.y;
                    tmp0.w = max(tmp0.w, 0.01);
                    tmp0.w = min(tmp0.w, 0.99);
                    tmp2.x = tmp0.w * tmp0.w;
                    tmp2.y = tmp0.x * 2.0 + tmp0.z;
                    tmp2 = tex2D(_MainTex, tmp2.xy);
                    tmp0.y = log(tmp0.y);
                    tmp0.y = tmp0.y * 0.75;
                    tmp0.y = exp(tmp0.y);
                    tmp0.y = saturate(tmp0.y * 3.0 + -0.2);
                    tmp0.yzw = tmp2.xyz * tmp0.yyy;
                    tmp1.w = inp.texcoord8.y - _OiledAmount;
                    tmp1.w = tmp0.x * 0.4 + tmp1.w;
                    tmp1.w = _OiledAmount * -tmp1.w + tmp1.w;
                    tmp1.w = tmp1.w > 0.0;
                    tmp0.yzw = tmp1.www ? tmp1.xyz : tmp0.yzw;
                    tmp0.x = tmp0.x * 0.4 + -0.05;
                    tmp0.x = ceil(tmp0.x);
                    tmp0.x = tmp0.x * 0.1 + 0.25;
                    tmp2.xyz = tmp1.xyz - tmp0.yzw;
                    o.sv_target.xyz = tmp0.xxx * tmp2.xyz + tmp0.yzw;
                } else {
                    o.sv_target.xyz = tmp1.xyz;
                }
                tmp0.x = _ForceOutline * 0.5;
                o.sv_target1.x = max(tmp0.x, _Outline);
                o.sv_target1.y = _ShouldForceOutlines;
                return o;
			}
			ENDCG
		}
	}
}