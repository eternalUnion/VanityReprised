Shader "ULTRAKILL/PostProcessV2" {
	Properties {
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
		[NoScaleOffset] _HudTex ("Hud Texture", 2D) = "white" {}
		[NoScaleOffset] _OutlineTex ("Outline Texture", 2D) = "black" {}
		[NoScaleOffset] _Dither ("Dither Texture", 2D) = "grey" {}
		[Header(Underwater Controls)] [NoScaleOffset] _NoiseTex ("Tiling Noise", 2D) = "white" {}
		_UnderWaterScale ("Underwater Warp Scaling", Float) = 1
		_UnderWaterStrength ("Underwater Warp Strength", Float) = 1
		_UnderWaterSpeed ("Underwater Warp Speed", Float) = 1
		_JFADistance ("JFA Distance", Float) = 1
	}
	SubShader {
		LOD 100
		Tags { "LIGHTMODE" = "ALWAYS" "PASSFLAGS" = "OnlyDirectional" "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "LIGHTMODE" = "ALWAYS" "PASSFLAGS" = "OnlyDirectional" "RenderType" = "Opaque" }
			GpuProgramID 37166
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float2 _VirtualRes;
			int _ColorPrecision;
			float _DitherStrength;
			float _OutlinesEnabled;
			float _Gamma;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _HudTex;
			sampler2D _OutlineTex;
			sampler2D _Dither;
			
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
                tmp0 = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.position = tmp0;
                tmp0.y = tmp0.y * _ProjectionParams.x;
                tmp1.xzw = tmp0.xwy * float3(0.5, 0.5, 0.5);
                o.texcoord.zw = tmp0.zw;
                o.texcoord.xy = tmp1.zz + tmp1.xw;
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
                tmp0 = _ScreenParams / _VirtualRes.xyxy;
                tmp1 = float4(1.0, 1.0, 1.0, 1.0) / _ScreenParams;
                tmp0 = tmp0 * tmp1;
                tmp1.xy = inp.texcoord.xy / inp.texcoord.ww;
                tmp2 = tmp0.zwzw * float4(1.0, 0.0, -1.0, 0.0) + tmp1.xyxy;
                tmp0 = tmp0 * float4(0.0, 1.0, 0.0, -1.0) + tmp1.xyxy;
                tmp3 = tex2D(_MainTex, tmp2.xy);
                tmp2 = tex2D(_MainTex, tmp2.zw);
                tmp1.zw = max(tmp2.xy, tmp3.xy);
                tmp2 = tex2D(_MainTex, tmp0.xy);
                tmp0 = tex2D(_MainTex, tmp0.zw);
                tmp0.xy = max(tmp0.xy, tmp2.xy);
                tmp0.xy = max(tmp0.xy, tmp1.zw);
                tmp0.w = _OutlinesEnabled == 1.0;
                tmp2 = tex2D(_MainTex, tmp1.xy);
                tmp0.z = tmp2.y;
                tmp0.yz = tmp0.ww ? float2(1.0, 1.0) : tmp0.zy;
                tmp0.x = min(tmp0.z, tmp0.x);
                tmp0.y = min(tmp0.y, tmp2.x);
                tmp0.xy = ceil(tmp0.xy);
                tmp0.x = tmp0.x - tmp0.y;
                tmp0.x = saturate(1.0 - tmp0.x);
                tmp2 = tex2D(_MainTex, tmp1.xy);
                tmp2.xyz = tmp0.xxx * tmp2.xyz;
                tmp0 = tex2D(_MainTex, tmp1.xy);
                tmp1.xy = tmp1.xy * _VirtualRes;
                tmp1.xy = tmp1.xy * float2(0.0625, 0.0625);
                tmp1 = tex2D(_MainTex, tmp1.xy);
                tmp3 = tmp0 - tmp2;
                tmp0 = saturate(tmp0.wwww * tmp3 + tmp2);
                tmp2.xyz = tmp0.xyz * tmp1.xyz;
                tmp1.xyz = -tmp1.xyz * float3(2.0, 2.0, 2.0) + float3(1.0, 1.0, 1.0);
                tmp2.xyz = tmp2.xyz + tmp2.xyz;
                tmp3.xyz = tmp0.xyz * tmp0.xyz;
                tmp1.xyz = tmp3.xyz * tmp1.xyz + tmp2.xyz;
                tmp1.xyz = tmp1.xyz - tmp0.xyz;
                tmp1.w = saturate(_DitherStrength);
                tmp0.xyz = tmp1.www * tmp1.xyz + tmp0.xyz;
                o.sv_target.w = tmp0.w;
                tmp0.w = max(_ColorPrecision, 2);
                tmp0.w = floor(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.xyz = floor(tmp0.xyz);
                tmp0.xyz = tmp0.xyz / tmp0.www;
                tmp0.xyz = log(tmp0.xyz);
                tmp0.w = -_Gamma * 0.5 + 1.0;
                tmp0.w = tmp0.w + tmp0.w;
                tmp0.w = max(tmp0.w, 0.01);
                tmp0.xyz = tmp0.xyz * tmp0.www;
                o.sv_target.xyz = exp(tmp0.xyz);
                return o;
			}
			ENDCG
		}
	}
}