Shader "ULTRAKILL/Master" {
	Properties {
		[MainProp] _Color ("Color", Vector) = (1,1,1,1)
		[MainProp] _MainTex ("Base (RGB)", 2D) = "white" {}
		[MainProp] _VertexWarpScale ("Vertex Warping Scalar", Range(0, 10)) = 1
		[MainProp] [Toggle] _Outline ("Assist Outline", Float) = 0
		[MainProp] [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull", Float) = 2
		[MainProp] [Toggle] _ZWrite ("ZWrite", Float) = 1
		[MainProp] [KeywordEnum(On, Off, Transparent)] _Fog ("Fog Mode", Float) = 0
		_Opacity ("Opacity", Range(0, 1)) = 1
		[Enum(Opaque,0,Cutout,1,Transparent,2,Advanced,3)] _BlendMode ("Blend Mode", Float) = 0
		[Toggle] _VertexLighting ("Vertex Lighting", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dest Blend", Float) = 0
		[Keyword(CAUSTICS)] [NoScaleOffset] _Cells ("Cells", 2D) = "white" {}
		[Keyword(CAUSTICS)] [NoScaleOffset] _Perlin ("Perlin", 2D) = "white" {}
		[Keyword(CAUSTICS)] _CausticColor ("Caustics Color", Vector) = (1,1,1,1)
		[Keyword(CAUSTICS)] _Scale ("World Scale", Float) = 1
		[Keyword(CAUSTICS)] _Falloff ("Mask Falloff", Float) = 1
		[Keyword(CAUSTICS)] _Speed ("Speed", Float) = 0.5
		[Keyword(CUSTOM_COLORS, REFLECTION)] _CubeTex ("Cube Texture", Cube) = "_Skybox" {}
		[Keyword(CUSTOM_COLORS, REFLECTION)] _ReflectionStrength ("Reflection Strength", Float) = 1
		[Keyword(CUSTOM_COLORS)] _IDTex ("ID Texture", 2D) = "white" {}
		[Keyword(CUSTOM_COLORS)] _CustomColor1 ("Custom Color 1", Vector) = (1,1,1,1)
		[Keyword(CUSTOM_COLORS)] _CustomColor2 ("Custom Color 2", Vector) = (1,1,1,1)
		[Keyword(CUSTOM_COLORS)] _CustomColor3 ("Custom Color 3", Vector) = (1,1,1,1)
		[Keyword(CUSTOM_COLORS)] _ReflectionFalloff ("Reflection Falloff", Float) = 1
		[Keyword(CUSTOM_LIGHTMAP)] _LightMapTex ("Light Map Texture", 2D) = "white" {}
		[Keyword(BLOOD_ABSORBER] [NoScaleOffset] _BloodTex ("BloodTex", 2D) = "black" {}
		[Keyword(BLOOD_FILLER)] _BloodNoiseTex ("Blood Noise Texture", 2D) = "white" {}
		[Keyword(CYBER_GRIND, EMISSIVE)] _EmissiveColor ("Emissive Color", Vector) = (1,1,1,1)
		[Keyword(CYBER_GRIND, EMISSIVE)] _EmissiveTex ("Emissive Texture", 2D) = "white" {}
		[Keyword(CYBER_GRIND, EMISSIVE)] _EmissiveIntensity ("Emissive Strength", Float) = 1
		[Keyword(CYBER_GRIND, EMISSIVE)] _EmissiveSaturation ("Emissive Saturation", Float) = 1
		[Keyword(CYBER_GRIND, EMISSIVE)] [Toggle] _UseAlbedoAsEmissive ("Use Albedo As Emissive", Float) = 1
		[Keyword(CYBER_GRIND, EMISSIVE)] [Toggle] _EmissiveReplaces ("Emissive Replaces Instead of Adding to Underlying Color", Float) = 0
		[Keyword(CYBER_GRIND)] _GradientFalloff ("Gradient Falloff", Float) = 1
		[Keyword(CYBER_GRIND)] _GradientScale ("Gradient Scale", Float) = 1
		[Keyword(CYBER_GRIND)] _GradientSpeed ("Gradient Speed", Float) = 1
		[Keyword(CYBER_GRIND)] [Toggle] _PCGamerMode ("PC Gamer Mode", Float) = 0
		[Keyword(CYBER_GRIND)] _RainbowDensity ("Rainbow Density", Float) = 1
		[Keyword(FRESNEL)] _FresnelColor ("Fresnel Color", Vector) = (1,1,1,1)
		[Keyword(FRESNEL)] _FresnelStrength ("Fresnel Strength", Float) = 1
		[Keyword(RADIANCE)] _InflateStrength ("Inflate Strength", Float) = 1
		[Keyword(SCROLLING)] _ScrollSpeed ("Scroll Speed", Vector) = (0,0,0,0)
		[Keyword(VERTEX_DISPLACEMENT)] _VertexNoiseTex ("Vertex Noise Texture Lookup", 2D) = "black" {}
		[Keyword(VERTEX_DISPLACEMENT)] _VertexNoiseScale ("Vertex Distortion Density", Range(0, 10)) = 1
		[Keyword(VERTEX_DISPLACEMENT)] _VertexNoiseSpeed ("Vertex Distortion Speed", Range(0, 10)) = 1
		[Keyword(VERTEX_DISPLACEMENT)] _VertexNoiseAmplitude ("Vertex Distortion Amplitude", Range(0, 50)) = 1
		[Keyword(VERTEX_DISPLACEMENT)] _VertexScale ("Vertex Inflation Scale", Range(0, 1)) = 0
		[Keyword(VERTEX_DISPLACEMENT)] _FlowDirection ("Vertex Distortion Flow Direction (Normalized XYZ)", Vector) = (0,1,0,1)
		[Keyword(VERTEX_DISPLACEMENT)] [Toggle] _ReverseFlow ("Reverse Flow", Float) = 0
		[Keyword(VERTEX_DISPLACEMENT)] [Toggle] _LocalOffset ("Use Local Space Offset", Float) = 0
		[Keyword(VERTEX_DISPLACEMENT)] [Toggle] _RecalculateNormals ("Recalculate Normals", Float) = 0
		[Keyword(VERTEX_DISPLACEMENT)] _NormalOffsetScale ("Normal Offset Scale", Float) = 1
	}
	SubShader {
		//Tags { "LIGHTMODE" = "Vertex" "RenderType" = "Opaque" }

        LOD 200
        Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }

		Pass {
			//Tags { "LIGHTMODE" = "Vertex" "RenderType" = "Opaque" }
			//Blend Zero Zero, Zero Zero
            // ZWrite Off
			// Cull Off
			// GpuProgramID 1969

            LOD 200
            Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
            Lighting On

            GpuProgramID 63289
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				/*float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float4 color1 : COLOR1;
				float2 texcoord : TEXCOORD0;
				float texcoord5 : TEXCOORD5;
				float3 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float4 texcoord4 : texcoord4;*/
                float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float texcoord5 : TEXCOORD5;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float4 color : COLOR0;
				float4 color1 : COLOR1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
				float2 sv_target1 : SV_Target1;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			float4 _Color;
			float4 unity_FogEnd;
			float4 unity_FogStart;
			float _TextureWarping;
			float _VertexWarping;
			float _VertexWarpScale;
			// $Globals ConstantBuffers for Fragment Shader
			bool _ForceOutline;
			bool _HasSandBuff;
			float _OiledAmount;
			float _Outline;
			float _ShouldForceOutlines;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
            float _Opacity;
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _SandTex;
			sampler2D _OilTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                /*v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                tmp0.x = max(_ScreenParams.y, _ScreenParams.x);
                tmp0.xy = _ScreenParams.xy / tmp0.xx;
                tmp1.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp1.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp1.xyz;
                tmp2.xyz = _WorldSpaceCameraPos - tmp1.xyz;
                tmp0.z = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = sqrt(tmp0.z);
                tmp0.z = rsqrt(tmp0.z);
                tmp3.xyz = tmp0.zzz * tmp2.xyz;
                tmp0.z = tmp0.w - 200.0;
                tmp0.w = unity_FogEnd.x - tmp0.w;
                tmp0.z = max(tmp0.z, 0.0);
                tmp0.z = log(tmp0.z);
                tmp0.z = tmp0.z * 0.2;
                tmp0.z = exp(tmp0.z);
                tmp0.z = tmp0.z * 0.1 + _VertexWarpScale;
                tmp0.z = tmp0.z * _VertexWarping;
                tmp0.xy = tmp0.zz * tmp0.xy;
                tmp4 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp4 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp4;
                tmp4 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp4;
                o.texcoord1.xyz = tmp1.xyz;
                tmp1 = tmp4 + unity_MatrixVP._m03_m13_m23_m33;
                tmp4.xyz = tmp1.xyz / tmp1.www;
                tmp5.xy = tmp0.xy * tmp4.xy;
                tmp5.xy = floor(tmp5.xy);
                tmp5.xy = tmp5.xy + float2(0.5, 0.5);
                tmp4.xy = tmp5.xy / tmp0.xy;
                tmp0.xyz = tmp1.www * tmp4.xyz;
                tmp2.w = _VertexWarping != 0.0;
                tmp1.xyz = tmp2.www ? tmp0.xyz : tmp1.xyz;
                tmp0.x = max(tmp1.w, 0.02);
                tmp0.x = tmp0.x - 0.5;
                o.position = tmp1;
                o.color.xyz = v.color.xyz * _Color.xyz;
                o.color.w = v.color.w;
                tmp0.y = unity_FogEnd.x - unity_FogStart.x;
                o.color1.w = saturate(tmp0.w / tmp0.y);
                o.color1.xyz = unity_FogColor.xyz;
                tmp0.y = min(_TextureWarping, 1.0);
                tmp0.y = tmp0.y * 0.5;
                tmp0.x = tmp0.y * tmp0.x + 0.5;
                tmp0.yz = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.texcoord.xy = tmp0.xx * tmp0.yz;
                o.texcoord5.x = tmp0.x;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(-tmp2.xyz, tmp0.xyz);
                tmp0.w = tmp0.w + tmp0.w;
                tmp2.xyz = tmp0.xyz * -tmp0.www + -tmp2.xyz;
                tmp0.w = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp4.xyz = tmp2.xyz * tmp0.www + -tmp2.xyz;
                tmp0.w = _TextureWarping + _TextureWarping;
                tmp0.w = min(tmp0.w, 1.0);
                o.texcoord2.xyz = tmp0.www * tmp4.xyz + tmp2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp3.xyz);
                o.texcoord3.xyz = tmp0.xyz;
                tmp0.x = 1.0 - abs(tmp0.w);
                tmp0.x = dot(tmp0.xy, tmp0.xy);
                o.texcoord2.w = min(tmp0.x, 1.0);
                tmp0.xz = tmp1.xw * float2(0.5, 0.5);
                o.texcoord4.zw = tmp1.zw;
                tmp0.y = tmp1.y * _ProjectionParams.x;
                tmp0.w = tmp0.y * 0.5;
                o.texcoord4.xy = tmp0.zz + tmp0.xw;
                return o;*/

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
                tmp3 = v.normal.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp3 = unity_ObjectToWorld._m00_m10_m20_m30 * v.normal.xxxx + tmp3;
                tmp3 = unity_ObjectToWorld._m02_m12_m22_m32 * v.normal.zzzz + tmp3;
                tmp0.w = dot(tmp3, tmp3);
                tmp0.w = rsqrt(tmp0.w);
                tmp3.xyz = tmp0.www * tmp3.xyz;
                tmp4.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp4.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp4.xyz;
                tmp4.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp4.xyz;
                tmp4.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp4.xyz;
                tmp5.xyz = _WorldSpaceCameraPos - tmp4.xyz;
                tmp0.w = dot(tmp5.xyz, tmp5.xyz);
                tmp1.w = sqrt(tmp0.w);
                tmp2.w = dot(tmp3.xyz, tmp3.xyz);
                tmp2.w = rsqrt(tmp2.w);
                tmp6.xyz = tmp2.www * tmp3.xyz;
                tmp0.w = rsqrt(tmp0.w);
                tmp7.xyz = tmp0.www * tmp5.xyz;
                tmp0.w = dot(tmp6.xyz, tmp7.xyz);
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
                /*fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                tmp0.xy = inp.texcoord.xy / inp.texcoord5.xx;
                tmp0 = tex2Dlod(_MainTex, float4(tmp0.xy, 0, 0.0));
                o.sv_target.w = tmp0.w * inp.color.w;
                tmp1.xyz = tmp0.xyz * inp.color.xyz;
                if (_HasSandBuff) {
                    tmp0.w = dot(inp.texcoord3.xyz, inp.texcoord3.xyz);
                    tmp0.w = rsqrt(tmp0.w);
                    tmp2.xyz = tmp0.www * inp.texcoord3.xyz;
                    tmp3 = _Time * float4(0.1, 0.5, 0.5, 0.1) + inp.texcoord1.xyyz;
                    tmp2.xw = max(abs(tmp2.zy), abs(tmp2.xx));
                    tmp2.xy = tmp2.xw < abs(tmp2.yz);
                    tmp2.xzw = tmp2.xxx ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                    tmp2.xyz = tmp2.yyy ? float3(0.0, 0.0, 1.0) : tmp2.xzw;
                    tmp4 = tmp2.zzxx * tmp3;
                    tmp2.xz = tmp4.zw + tmp4.xy;
                    tmp2.xy = tmp3.xw * tmp2.yy + tmp2.xz;
                    tmp2.xy = tmp2.xy * float2(0.25, 0.25);
                    tmp2 = tex2Dlod(_MainTex, float4(tmp2.xy, 0, 0.0));
                    tmp0.w = rsqrt(inp.texcoord2.w);
                    tmp0.w = 1.0 / tmp0.w;
                    tmp1.w = tmp0.w * 0.75 + 0.25;
                    tmp0.xyz = -tmp0.xyz * inp.color.xyz + tmp2.xyz;
                    tmp0.xyz = tmp1.www * tmp0.xyz + tmp1.xyz;
                    tmp2.xyz = tmp0.www * tmp2.xyz;
                    tmp1.xyz = tmp2.xyz * float3(0.75, 0.75, 0.75) + tmp0.xyz;
                }
                tmp0.xy = inp.texcoord2.xy * float2(0.05, 0.05);
                tmp0 = tex2Dlod(_MainTex, float4(tmp0.xy, 0, 0.0));
                tmp2.x = tmp0.x + inp.texcoord2.w;
                tmp2.y = tmp0.x * 5.0;
                tmp0 = tex2Dlod(_MainTex, float4(tmp2.xy, 0, 0.0));
                tmp0.w = log(inp.texcoord2.w);
                tmp0.w = tmp0.w * 0.75;
                tmp0.w = exp(tmp0.w);
                tmp0.w = saturate(tmp0.w * 3.0 + -0.2);
                tmp0.xyz = tmp0.www * tmp0.xyz + -tmp1.xyz;
                o.sv_target.xyz = _OiledAmount.xxx * tmp0.xyz + tmp1.xyz;
                tmp0.x = _ForceOutline ? 0.5 : 0.0;
                o.sv_target1.x = max(tmp0.x, _Outline);
                o.sv_target1.y = _ShouldForceOutlines;
                return o;*/

                #if defined(CUSTOM_COLORS) && defined(REFLECTION)

                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0.xy = inp.texcoord.xy / inp.texcoord5.xx;
                tmp0 = tex2Dlod(_MainTex, float4(tmp0.xy, 0, 0.0));
                tmp0.w = tmp0.w * inp.color.w;
                tmp0.xyz = tmp0.xyz * inp.color.xyz + -inp.color1.xyz;
                tmp0.xyz = inp.color1.www * tmp0.xyz + inp.color1.xyz;
                tmp0.w = saturate(tmp0.w);
                tmp1.x = _HasSandBuff > 0.0;
                if (tmp1.x) {
                    tmp1.x = dot(inp.texcoord3.xyz, inp.texcoord3.xyz);
                    tmp1.x = rsqrt(tmp1.x);
                    tmp1.xyz = tmp1.xxx * inp.texcoord3.xyz;
                    tmp2 = _Time * float4(0.1, 0.5, 0.5, 0.1) + inp.texcoord1.xyyz;
                    tmp1.xw = max(abs(tmp1.zy), abs(tmp1.xx));
                    tmp1.xy = tmp1.xw < abs(tmp1.yz);
                    tmp1.xzw = tmp1.xxx ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                    tmp1.xyz = tmp1.yyy ? float3(0.0, 0.0, 1.0) : tmp1.xzw;
                    tmp3 = tmp1.zzxx * tmp2;
                    tmp1.xz = tmp3.zw + tmp3.xy;
                    tmp1.xy = tmp2.xw * tmp1.yy + tmp1.xz;
                    tmp1.xy = tmp1.xy * float2(0.25, 0.25);
                    tmp1 = tex2Dlod(_SandTex, float4(tmp1.xy, 0, 0.0));
                    tmp1.w = rsqrt(inp.texcoord4.w);
                    tmp1.w = 1.0 / tmp1.w;
                    tmp2.x = tmp1.w * 0.75 + 0.25;
                    tmp2.yzw = tmp1.xyz - tmp0.xyz;
                    tmp2.xyz = tmp2.xxx * tmp2.yzw + tmp0.xyz;
                    tmp1.xyz = tmp1.www * tmp1.xyz;
                    tmp0.xyz = tmp1.xyz * float3(0.75, 0.75, 0.75) + tmp2.xyz;
                }
                tmp1 = texCUBE(_CubeTex, inp.texcoord2.xyz);
                tmp1.xyz = tmp1.xyz * _ReflectionStrength.xxx;
                tmp1.w = dot(tmp1.xyz, float3(0.333, 0.333, 0.333));
                tmp2.x = 1.0 - _ReflectionStrength;
                o.sv_target.xyz = saturate(tmp0.xyz * tmp2.xxx + tmp1.xyz);
                o.sv_target.w = saturate(tmp0.w * _Opacity + tmp1.w);
                tmp0.x = _ForceOutline * 0.5;
                o.sv_target1.x = max(tmp0.x, _Outline);
                o.sv_target1.y = _ShouldForceOutlines;
                return o;

                #else

                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0.xy = inp.texcoord.xy / inp.texcoord5.xx;
                tmp0 = tex2Dlod(_MainTex, float4(tmp0.xy, 0, 0.0));
                tmp0.w = tmp0.w * inp.color.w;
                tmp0.xyz = tmp0.xyz * inp.color.xyz + -inp.color1.xyz;
                tmp0.xyz = inp.color1.www * tmp0.xyz + inp.color1.xyz;
                tmp0.w = saturate(tmp0.w);
                o.sv_target.w = tmp0.w * _Opacity;
                tmp0.w = _HasSandBuff > 0.0;
                if (tmp0.w) {
                    tmp0.w = dot(inp.texcoord3.xyz, inp.texcoord3.xyz);
                    tmp0.w = rsqrt(tmp0.w);
                    tmp1.xyz = tmp0.www * inp.texcoord3.xyz;
                    tmp2 = _Time * float4(0.1, 0.5, 0.5, 0.1) + inp.texcoord1.xyyz;
                    tmp1.xw = max(abs(tmp1.zy), abs(tmp1.xx));
                    tmp1.xy = tmp1.xw < abs(tmp1.yz);
                    tmp1.xzw = tmp1.xxx ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                    tmp1.xyz = tmp1.yyy ? float3(0.0, 0.0, 1.0) : tmp1.xzw;
                    tmp3 = tmp1.zzxx * tmp2;
                    tmp1.xz = tmp3.zw + tmp3.xy;
                    tmp1.xy = tmp2.xw * tmp1.yy + tmp1.xz;
                    tmp1.xy = tmp1.xy * float2(0.25, 0.25);
                    tmp1 = tex2Dlod(_SandTex, float4(tmp1.xy, 0, 0.0));
                    tmp0.w = rsqrt(inp.texcoord4.w);
                    tmp0.w = 1.0 / tmp0.w;
                    tmp1.w = tmp0.w * 0.75 + 0.25;
                    tmp2.xyz = tmp1.xyz - tmp0.xyz;
                    tmp2.xyz = tmp1.www * tmp2.xyz + tmp0.xyz;
                    tmp1.xyz = tmp0.www * tmp1.xyz;
                    o.sv_target.xyz = tmp1.xyz * float3(0.75, 0.75, 0.75) + tmp2.xyz;
                } else {
                    o.sv_target.xyz = tmp0.xyz;
                }
                tmp0.x = _ForceOutline * 0.5;
                o.sv_target1.x = max(tmp0.x, _Outline);
                o.sv_target1.y = _ShouldForceOutlines;
                return o;

                #endif
			}
			ENDCG
		}
	}
	CustomEditor "ULTRAShaderEditor"
}