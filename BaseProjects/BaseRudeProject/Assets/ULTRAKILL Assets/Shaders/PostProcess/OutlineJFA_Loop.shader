Shader "PostProcess/OutlineJFA_Loop" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Distance ("Distance", Float) = 0
		_ResolutionDifference ("Virtual Screen Size", Vector) = (0,0,0,0)
	}
	SubShader {
		LOD 100
		Tags { "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "RenderType" = "Opaque" }
			GpuProgramID 6722
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
				float2 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float _Distance;
			float4 _ResolutionDifference;
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
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0 = tex2D(_MainTex, inp.texcoord.xy);
                tmp1.xy = tmp0.xy - inp.texcoord.xy;
                tmp0.w = dot(tmp1.xy, tmp1.xy);
                tmp1.x = floor(tmp0.x);
                tmp0.w = tmp0.w + abs(tmp1.x);
                tmp0.z = min(tmp0.w, 1.0);
                tmp1.xy = _Distance.xx / _ResolutionDifference.xy;
                tmp1.xy = tmp1.xy / _ScreenParams.xy;
                tmp1.zw = saturate(tmp1.xy + inp.texcoord.xy);
                tmp2 = tex2Dlod(_MainTex, float4(tmp1.zw, 0, 0.0));
                tmp1.zw = floor(tmp2.xy);
                tmp0.w = tmp1.w + tmp1.z;
                tmp1.zw = inp.texcoord.xy - tmp2.xy;
                tmp1.z = dot(tmp1.xy, tmp1.xy);
                tmp2.z = abs(tmp0.w) * 5.0 + tmp1.z;
                tmp0.w = tmp0.z >= tmp2.z;
                tmp0.xyz = tmp0.www ? tmp2.xyz : tmp0.xyz;
                tmp2 = saturate(tmp1.xyxy * float4(1.0, 0.0, 1.0, -1.0) + inp.texcoord.xyxy);
                tmp3 = tex2Dlod(_MainTex, float4(tmp2.xy, 0, 0.0));
                tmp2 = tex2Dlod(_MainTex, float4(tmp2.zw, 0, 0.0));
                tmp1.zw = floor(tmp3.xy);
                tmp0.w = tmp1.w + tmp1.z;
                tmp1.zw = inp.texcoord.xy - tmp3.xy;
                tmp1.z = dot(tmp1.xy, tmp1.xy);
                tmp3.z = abs(tmp0.w) * 5.0 + tmp1.z;
                tmp0.w = tmp0.z >= tmp3.z;
                tmp0.xyz = tmp0.www ? tmp3.xyz : tmp0.xyz;
                tmp1.zw = floor(tmp2.xy);
                tmp0.w = tmp1.w + tmp1.z;
                tmp1.zw = inp.texcoord.xy - tmp2.xy;
                tmp1.z = dot(tmp1.xy, tmp1.xy);
                tmp2.z = abs(tmp0.w) * 5.0 + tmp1.z;
                tmp0.w = tmp0.z >= tmp2.z;
                tmp0.xyz = tmp0.www ? tmp2.xyz : tmp0.xyz;
                tmp2 = saturate(tmp1.xyxy * float4(0.0, 1.0, 0.0, -1.0) + inp.texcoord.xyxy);
                tmp3 = tex2Dlod(_MainTex, float4(tmp2.xy, 0, 0.0));
                tmp2 = tex2Dlod(_MainTex, float4(tmp2.zw, 0, 0.0));
                tmp1.zw = floor(tmp3.xy);
                tmp0.w = tmp1.w + tmp1.z;
                tmp1.zw = inp.texcoord.xy - tmp3.xy;
                tmp1.z = dot(tmp1.xy, tmp1.xy);
                tmp3.z = abs(tmp0.w) * 5.0 + tmp1.z;
                tmp0.w = tmp0.z >= tmp3.z;
                tmp0.xyz = tmp0.www ? tmp3.xyz : tmp0.xyz;
                tmp1.zw = floor(tmp2.xy);
                tmp0.w = tmp1.w + tmp1.z;
                tmp1.zw = inp.texcoord.xy - tmp2.xy;
                tmp1.z = dot(tmp1.xy, tmp1.xy);
                tmp2.z = abs(tmp0.w) * 5.0 + tmp1.z;
                tmp0.w = tmp0.z >= tmp2.z;
                tmp0.xyz = tmp0.www ? tmp2.xyz : tmp0.xyz;
                tmp2 = saturate(tmp1.xyxy * float4(-1.0, 1.0, -1.0, 0.0) + inp.texcoord.xyxy);
                tmp1.xy = saturate(inp.texcoord.xy - tmp1.xy);
                tmp1 = tex2Dlod(_MainTex, float4(tmp1.xy, 0, 0.0));
                tmp3 = tex2Dlod(_MainTex, float4(tmp2.xy, 0, 0.0));
                tmp2 = tex2Dlod(_MainTex, float4(tmp2.zw, 0, 0.0));
                tmp1.zw = floor(tmp3.xy);
                tmp0.w = tmp1.w + tmp1.z;
                tmp1.zw = inp.texcoord.xy - tmp3.xy;
                tmp1.z = dot(tmp1.xy, tmp1.xy);
                tmp3.z = abs(tmp0.w) * 5.0 + tmp1.z;
                tmp0.w = tmp0.z >= tmp3.z;
                tmp0.xyz = tmp0.www ? tmp3.xyz : tmp0.xyz;
                tmp1.zw = floor(tmp2.xy);
                tmp0.w = tmp1.w + tmp1.z;
                tmp1.zw = inp.texcoord.xy - tmp2.xy;
                tmp1.z = dot(tmp1.xy, tmp1.xy);
                tmp2.z = abs(tmp0.w) * 5.0 + tmp1.z;
                tmp0.w = tmp0.z >= tmp2.z;
                tmp0.xyz = tmp0.www ? tmp2.xyz : tmp0.xyz;
                tmp1.zw = floor(tmp1.xy);
                tmp0.w = tmp1.w + tmp1.z;
                tmp1.zw = inp.texcoord.xy - tmp1.xy;
                tmp1.z = dot(tmp1.xy, tmp1.xy);
                tmp0.w = abs(tmp0.w) * 5.0 + tmp1.z;
                tmp0.z = tmp0.z >= tmp0.w;
                o.sv_target.xy = tmp0.zz ? tmp1.xy : tmp0.xy;
                return o;
			}
			ENDCG
		}
	}
}