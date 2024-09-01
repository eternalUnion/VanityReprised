Shader "ULTRAKILL/SplashSprite-VertexLit" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_TransparencyScale ("Transparency Scale", Float) = 2
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Vector) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		_Offset ("Depth Offset", Float) = -1000
	}
	SubShader {
		Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "Vertex" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "Vertex" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend One OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			Cull Off
			Stencil {
				Ref 2
				Comp Equal
				Pass Keep
				Fail Keep
				ZFail Keep
			}
			GpuProgramID 42670
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Color;
			// $Globals ConstantBuffers for Fragment Shader
			float _TransparencyScale;
			// Custom ConstantBuffers for Vertex Shader
			CBUFFER_START(UnityPerDrawSprite)
				float4 _RendererColor;
				float2 _Flip;
			CBUFFER_END
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
                tmp3.xy = v.vertex.xy * _Flip;
                tmp4 = tmp3.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp3 = unity_ObjectToWorld._m00_m10_m20_m30 * tmp3.xxxx + tmp4;
                tmp3 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp3;
                tmp3 = tmp3 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp4 = tmp3.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp4 = unity_MatrixVP._m00_m10_m20_m30 * tmp3.xxxx + tmp4;
                tmp4 = unity_MatrixVP._m02_m12_m22_m32 * tmp3.zzzz + tmp4;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp3.wwww + tmp4;
                tmp4 = v.color * _Color;
                tmp4 = tmp4 * _RendererColor;
                tmp5.xyz = tmp3.yyy * unity_MatrixV._m01_m11_m21;
                tmp5.xyz = unity_MatrixV._m00_m10_m20 * tmp3.xxx + tmp5.xyz;
                tmp3.xyz = unity_MatrixV._m02_m12_m22 * tmp3.zzz + tmp5.xyz;
                tmp3.xyz = unity_MatrixV._m03_m13_m23 * tmp3.www + tmp3.xyz;
                tmp0.x = dot(tmp0.xyz, v.normal.xyz);
                tmp0.y = dot(tmp1.xyz, v.normal.xyz);
                tmp0.z = dot(tmp2.xyz, v.normal.xyz);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = glstate_lightmodel_ambient.xyz + glstate_lightmodel_ambient.xyz;
                tmp2.xyz = tmp1.xyz;
                tmp0.w = 0.0;
                for (int i = tmp0.w; i < 8; i += 1) {
                    tmp5.xyz = -tmp3.xyz * unity_LightPosition[i].www + unity_LightPosition[i].xyz;
                    tmp1.w = dot(tmp5.xyz, tmp5.xyz);
                    tmp1.w = max(tmp1.w, 0.000001);
                    tmp2.w = rsqrt(tmp1.w);
                    tmp5.xyz = tmp2.www * tmp5.xyz;
                    tmp1.w = tmp1.w * unity_LightAtten[i].z + 1.0;
                    tmp1.w = 1.0 / tmp1.w;
                    tmp2.w = dot(tmp5.xyz, unity_SpotDirection[i].xyz);
                    tmp2.w = max(tmp2.w, 0.0);
                    tmp2.w = tmp2.w - unity_LightAtten[i].x;
                    tmp2.w = saturate(tmp2.w * unity_LightAtten[i].y);
                    tmp1.w = tmp1.w * tmp2.w;
                    tmp2.w = dot(tmp0.xyz, tmp5.xyz);
                    tmp2.w = max(tmp2.w, 0.0);
                    tmp1.w = tmp1.w * tmp2.w;
                    tmp2.xyz = unity_LightColor[i].xyz * tmp1.www + tmp2.xyz;
                }
                tmp0.xyz = tmp2.xyz + float3(0.1, 0.1, 0.1);
                tmp0.w = 1.1;
                o.color = tmp0 * tmp4;
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = tex2D(_MainTex, inp.texcoord.xy);
                tmp0 = tmp0 * inp.color;
                tmp1.x = tmp0.w * _TransparencyScale;
                o.sv_target.xyz = tmp0.xyz * tmp1.xxx;
                o.sv_target.w = tmp0.w;
                return o;
			}
			ENDCG
		}
	}
}