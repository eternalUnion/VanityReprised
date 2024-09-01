Shader "Unlit/Volumetric" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Center ("Center", Float) = 1
		_Radius ("Radius", Float) = 1
	}
	SubShader {
		LOD 100
		Tags { "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "RenderType" = "Opaque" }
			Cull Front
			GpuProgramID 5614
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float4 position : SV_POSITION0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float _Radius;
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
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord1.xyz = v.vertex.xyz;
                tmp0.xyz = _WorldSpaceCameraPos * unity_WorldToObject._m01_m11_m21;
                tmp0.xyz = unity_WorldToObject._m00_m10_m20 * _WorldSpaceCameraPos + tmp0.xyz;
                tmp0.xyz = unity_WorldToObject._m02_m12_m22 * _WorldSpaceCameraPos + tmp0.xyz;
                o.texcoord2.xyz = tmp0.xyz + unity_WorldToObject._m03_m13_m23;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3 = float4(0, 0, 0, 0);
                tmp0.xyz = inp.texcoord1.xyz - inp.texcoord2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = 0.0;
                tmp1.x = 0.0;
                float i = 0;
                while (true) {
                    tmp1.y = i >= 100;
                    if (tmp1.y) {
                        break;
                    }
                    tmp2.yzw = tmp0.www * -tmp0.xyz + inp.texcoord1.xyz;
                    tmp1.y = dot(tmp2.xy, tmp2.xy);
                    tmp1.y = sqrt(tmp1.y);
                    tmp2.x = tmp1.y - _Radius;
                    tmp1.y = dot(tmp2.xy, tmp2.xy);
                    tmp1.y = sqrt(tmp1.y);
                    tmp1.y = -_Radius * 0.5 + tmp1.y;
                    tmp1.z = tmp0.w + tmp1.y;
                    tmp1.y = tmp1.y < 0.001;
                    tmp1.w = tmp1.z > 100.0;
                    tmp1.y = uint1(tmp1.w) | uint1(tmp1.y);
                    if (tmp1.y) {
                        tmp0.w = tmp1.z;
                        break;
                    }
                    i = i + 1;
                    tmp0.w = tmp1.z;
                }
                tmp1.yzw = -tmp0.xyz * tmp0.www + inp.texcoord1.xyz;
                tmp0.x = dot(tmp1.xy, tmp1.xy);
                tmp0.x = sqrt(tmp0.x);
                tmp1.x = tmp0.x - _Radius;
                tmp0.x = dot(tmp1.xy, tmp1.xy);
                tmp0.x = sqrt(tmp0.x);
                tmp0.x = -_Radius * 0.5 + tmp0.x;
                tmp2.yzw = tmp1.yzw - float3(0.01, 0.0, 0.0);
                tmp0.y = dot(tmp2.xy, tmp2.xy);
                tmp0.y = sqrt(tmp0.y);
                tmp2.x = tmp0.y - _Radius;
                tmp0.y = dot(tmp2.xy, tmp2.xy);
                tmp0.y = sqrt(tmp0.y);
                tmp2.x = -_Radius * 0.5 + tmp0.y;
                tmp3.yzw = tmp1.yzw - float3(0.0, 0.01, 0.0);
                tmp0.y = dot(tmp3.xy, tmp3.xy);
                tmp0.y = sqrt(tmp0.y);
                tmp3.x = tmp0.y - _Radius;
                tmp0.y = dot(tmp3.xy, tmp3.xy);
                tmp0.y = sqrt(tmp0.y);
                tmp2.y = -_Radius * 0.5 + tmp0.y;
                tmp1.yzw = tmp1.yzw - float3(0.0, 0.0, 0.01);
                tmp0.y = dot(tmp1.xy, tmp1.xy);
                tmp0.y = sqrt(tmp0.y);
                tmp1.x = tmp0.y - _Radius;
                tmp0.y = dot(tmp1.xy, tmp1.xy);
                tmp0.y = sqrt(tmp0.y);
                tmp2.z = -_Radius * 0.5 + tmp0.y;
                tmp0.xyz = tmp0.xxx - tmp2.xyz;
                tmp1.x = dot(tmp0.xyz, tmp0.xyz);
                tmp1.x = rsqrt(tmp1.x);
                tmp0.xyz = tmp0.xyz * tmp1.xxx;
                tmp0.w = tmp0.w > 100.0;
                if (tmp0.w) {
                    o.sv_target.xyz = tmp0.xyz;
                    o.sv_target.w = 0.0;
                    return o;
                }
                o.sv_target = saturate(dot(tmp0.xyz, _WorldSpaceLightPos0.xyz));
                return o;
			}
			ENDCG
		}
	}
}