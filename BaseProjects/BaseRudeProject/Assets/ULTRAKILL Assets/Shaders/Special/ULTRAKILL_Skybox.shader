Shader "ULTRAKILL/Skybox" {
	Properties {
		_RotateSky ("Sky Rotation", Range(0, 1)) = 0
		[KeywordEnum(Panorama, Cubemap)] _TexMode ("Texture Type", Float) = 0
		_MainTex ("Texture", 2D) = "white" {}
		_Cubemap ("Cubemap", Cube) = "hiwte" {}
		[IntRange] _SkyTile ("Pano Sky Latitude Tile", Range(1, 4)) = 4
		_SkyHeightOffset ("Pano Sky Height Offset", Float) = 0.5
		_SkyStretch ("Pano Sky Stretch", Float) = 1
		_SkyTopSharpness ("Pano Sky Top Sharpness", Float) = 4
	}
	SubShader {
		LOD 100
		Tags { "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "RenderType" = "Opaque" }
			GpuProgramID 45424
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float3 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float _SkyTile;
			float _RotateSky;
			float _SkyHeightOffset;
			float _SkyTopSharpness;
			float _SkyStretch;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			
			// Keywords: _TEXMODE_PANORAMA
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp0.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                o.texcoord1.xyz = _WorldSpaceCameraPos - tmp0.xyz;
                tmp0 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp0 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp0;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp0;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp0;
                return o;
			}
			// Keywords: _TEXMODE_PANORAMA
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = dot(inp.texcoord1.xyz, inp.texcoord1.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp0.xyz = tmp0.xxx * inp.texcoord1.xzy;
                tmp0.w = max(abs(tmp0.y), abs(tmp0.x));
                tmp0.w = 1.0 / tmp0.w;
                tmp1.x = min(abs(tmp0.y), abs(tmp0.x));
                tmp0.w = tmp0.w * tmp1.x;
                tmp1.x = tmp0.w * tmp0.w;
                tmp1.y = tmp1.x * 0.0208351 + -0.085133;
                tmp1.y = tmp1.x * tmp1.y + 0.180141;
                tmp1.y = tmp1.x * tmp1.y + -0.3302995;
                tmp1.x = tmp1.x * tmp1.y + 0.999866;
                tmp1.y = tmp0.w * tmp1.x;
                tmp1.y = tmp1.y * -2.0 + 1.570796;
                tmp1.z = abs(tmp0.y) < abs(tmp0.x);
                tmp1.y = tmp1.z ? tmp1.y : 0.0;
                tmp0.w = tmp0.w * tmp1.x + tmp1.y;
                tmp1.xy = tmp0.yz < -tmp0.yz;
                tmp1.x = tmp1.x ? -3.141593 : 0.0;
                tmp0.w = tmp0.w + tmp1.x;
                tmp1.x = min(tmp0.y, tmp0.x);
                tmp1.x = tmp1.x < -tmp1.x;
                tmp0.x = max(tmp0.y, tmp0.x);
                tmp0.x = tmp0.x >= -tmp0.x;
                tmp0.x = tmp0.x ? tmp1.x : 0.0;
                tmp0.x = tmp0.x ? -tmp0.w : tmp0.w;
                tmp0.x = tmp0.x + 3.141593;
                tmp0.y = 1.0 / _SkyTile;
                tmp0.y = tmp0.y * 6.283185;
                tmp0.x = tmp0.x / tmp0.y;
                tmp0.x = tmp0.x - _RotateSky;
                tmp0.w = abs(tmp0.z) * -0.0187293 + 0.074261;
                tmp0.w = tmp0.w * abs(tmp0.z) + -0.2121144;
                tmp0.w = tmp0.w * abs(tmp0.z) + 1.570729;
                tmp0.z = 1.0 - abs(tmp0.z);
                tmp0.z = sqrt(tmp0.z);
                tmp1.x = tmp0.z * tmp0.w;
                tmp1.x = tmp1.x * -2.0 + 3.141593;
                tmp1.x = tmp1.y ? tmp1.x : 0.0;
                tmp0.z = tmp0.w * tmp0.z + tmp1.x;
                tmp0.w = _SkyStretch * 1.570796;
                tmp0.z = tmp0.z / tmp0.w;
                tmp0.z = saturate(tmp0.z - _SkyHeightOffset);
                tmp0.y = tmp0.z - 0.000001;
                tmp1 = tex2Dlod(_MainTex, float4(tmp0.xy, 0, 0.0));
                tmp0.x = log(tmp0.y);
                tmp0.x = tmp0.x * _SkyTopSharpness;
                tmp0.x = exp(tmp0.x);
                tmp2 = tex2D(_MainTex, float2(0.5, 0.999));
                tmp0.yzw = tmp2.xyz - tmp1.xyz;
                o.sv_target.xyz = tmp0.xxx * tmp0.yzw + tmp1.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
	}
}