Shader "UI/Slider_UK" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_FillAmount ("Fill Amount", Float) = 1
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 8
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}
	SubShader {
		Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Name "Default"
			Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend One OneMinusSrcAlpha, One OneMinusSrcAlpha
			ColorMask 0 -1
			ZWrite Off
			Cull Off
			Stencil {
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
				Comp[_StencilComp]
				Pass Keep
				Fail Keep
				ZFail Keep
			}
			GpuProgramID 61801
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _MainTex_TexelSize;
			float _FillAmount;
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
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.color = v.color;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.texcoord1 = v.vertex;
                o.texcoord2 = v.vertex;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = _MainTex_TexelSize.z * 5.0;
                tmp0.y = 7.0 / tmp0.x;
                tmp0.y = max(tmp0.y, _FillAmount);
                tmp0.z = tmp0.x * inp.texcoord.x;
                tmp0.w = tmp0.z > 4.0;
                tmp0.y = 1.0 - tmp0.y;
                tmp0.y = tmp0.y * tmp0.x + tmp0.z;
                tmp0.y = tmp0.w ? tmp0.y : tmp0.z;
                tmp0.z = tmp0.y > 4.0;
                tmp0.w = _MainTex_TexelSize.z * 5.0 + -4.0;
                tmp0.w = tmp0.y < tmp0.w;
                tmp0.z = tmp0.w ? tmp0.z : 0.0;
                tmp1.x = tmp0.z ? 4.0 : tmp0.y;
                tmp0.x = tmp0.x < tmp1.x;
                tmp1.z = saturate(tmp1.x);
                tmp1.yw = inp.texcoord.yy;
                tmp0.yz = tmp1.xy * _MainTex_TexelSize.zw;
                tmp0.yz = tmp0.yz >= -tmp0.yz;
                tmp0.yz = tmp0.yz ? _MainTex_TexelSize.zw : -_MainTex_TexelSize.zw;
                tmp2.xy = float2(1.0, 1.0) / tmp0.yz;
                tmp1.xy = tmp1.xy * tmp2.xy;
                tmp1.xy = frac(tmp1.xy);
                tmp0.yz = tmp0.yz * tmp1.xy;
                tmp0.xy = tmp0.xx ? tmp1.zw : tmp0.yz;
                tmp1.x = _MainTex_TexelSize.z;
                tmp1.y = 1.0;
                tmp0.xy = tmp0.xy / tmp1.xy;
                tmp0 = tex2D(_MainTex, tmp0.xy);
                tmp0.w = tmp0.w <= 0.0;
                if (tmp0.w) {
                    discard;
                }
                o.sv_target.xyz = tmp0.xyz * inp.color.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
	}
}