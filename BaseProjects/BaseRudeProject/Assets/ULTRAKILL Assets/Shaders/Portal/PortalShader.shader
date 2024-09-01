Shader "Unlit/PortalShader" {
	Properties {
		_FogColor ("FogColor", Vector) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_InfiniteTex ("InfiniteTex", 2D) = "grey" {}
	}
	SubShader {
		LOD 100
		Tags { "LIGHTMODE" = "ALWAYS" "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "LIGHTMODE" = "ALWAYS" "RenderType" = "Opaque" }
			Cull Off
			GpuProgramID 46787
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 position : SV_POSITION0;
				float2 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4x4 PORTAL_MATRIX_VP;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _FogColor;
			float _Iteration;
			float _RecursionIteration;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _InfiniteTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xz = float2(0.5, 0.5);
                tmp0.y = _ProjectionParams.x;
                tmp1 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp1;
                tmp1 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp1;
                tmp1 = tmp1 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp2 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp2 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp2;
                tmp2 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp2;
                tmp2 = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp2;
                tmp0.xyz = tmp0.xyz * tmp2.xyw;
                tmp0.w = tmp0.y * 0.5;
                o.texcoord.xy = tmp0.zz + tmp0.xw;
                o.texcoord.zw = tmp2.zw;
                o.position = tmp2;
                tmp0 = tmp1.yyyy * PORTAL_MATRIX_VP._m01_m11_m21_m31;
                tmp0 = PORTAL_MATRIX_VP._m00_m10_m20_m30 * tmp1.xxxx + tmp0;
                tmp0 = PORTAL_MATRIX_VP._m02_m12_m22_m32 * tmp1.zzzz + tmp0;
                tmp0 = PORTAL_MATRIX_VP._m03_m13_m23_m33 * tmp1.wwww + tmp0;
                tmp1.xz = tmp0.xw;
                o.texcoord1.zw = tmp0.zw;
                tmp1.y = _ProjectionParams.x * _ProjectionParams.x;
                tmp0.xz = float2(0.5, 0.5);
                tmp0.xyz = tmp0.xyz * tmp1.xyz;
                tmp0.w = tmp0.y * 0.5;
                o.texcoord1.xy = tmp0.zz + tmp0.xw;
                o.texcoord2.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = _RecursionIteration == _Iteration;
                if (tmp0.x) {
                    tmp0.xy = inp.texcoord1.xy / inp.texcoord1.ww;
                    o.sv_target = tex2D(_MainTex, tmp0.xy);
                    return o;
                }
                tmp0.xy = inp.texcoord.xy / inp.texcoord.ww;
                tmp0.z = inp.texcoord.w - 99999.0;
                tmp0.z = saturate(tmp0.z * 0.0004);
                tmp1 = tex2D(_MainTex, tmp0.xy);
                tmp2 = _FogColor - tmp1;
                o.sv_target = tmp0.zzzz * tmp2 + tmp1;
                return o;
			}
			ENDCG
		}
	}
}