Shader "ULTRAKILL/PaletteToLUT" {
	Properties {
		_Palette ("Palette", 2D) = "white" {}
	}
	SubShader {
		LOD 100
		Tags { "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "RenderType" = "Opaque" }
			GpuProgramID 65413
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
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			int progress;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _LastLUT;
			sampler2D _Palette;
			
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
                float4 tmp4;
                float4 tmp5;
                float4 tmp6;
                float4 tmp7;
                tmp0.xy = inp.texcoord.xy * float2(4096.0, 4096.0);
                tmp0.xy = floor(tmp0.xy);
                tmp1 = tmp0.xyxy * float4(256.0, 256.0, 256.0, 256.0);
                tmp1 = tmp1 >= -tmp1.zwzw;
                tmp1 = tmp1 ? float4(256.0, 256.0, 0.0039063, 0.0039063) : float4(-256.0, -256.0, -0.0039063, -0.0039063);
                tmp0.zw = tmp0.xy * tmp1.zw;
                tmp0.zw = frac(tmp0.zw);
                tmp2.xy = tmp0.zw * tmp1.xy;
                tmp0.xy = -tmp1.xy * tmp0.zw + tmp0.xy;
                tmp0.xy = tmp0.xy * float2(0.0039063, 0.0625);
                tmp2.z = tmp0.y + tmp0.x;
                tmp0.xyz = tmp2.xyz * float3(0.0039216, 0.0039216, 0.0039216);
                tmp1 = tex2D(_LastLUT, inp.texcoord.xy);
                tmp1 = progress.xxxx ? tmp1 : float4(4.0, 4.0, 4.0, 0.0);
                tmp0.w = float1(int1(progress) << 5);
                tmp2.w = tmp0.w + 32;
                tmp3.y = 1.0;
                tmp4.xyz = tmp0.xyz;
                tmp5.xy = float2(4.0, 0.0);
                tmp3.z = tmp0.w;
                for (int i = tmp3.z; i < tmp2.w; i += 1) {
                    tmp3.w = floor(i);
                    tmp5.z = tmp3.w * 0.03125;
                    tmp6.x = tmp3.w * 0.03125 + 0.015625;
                    tmp3.w = floor(tmp5.z);
                    tmp3.w = tmp3.w + 0.015625;
                    tmp6.y = tmp3.w * 0.03125;
                    tmp6 = tex2Dlod(_Palette, float4(tmp6.xy, 0, 0.0));
                    tmp3.w = tmp6.w != 0.0;
                    tmp7.xyz = tmp2.xyz * float3(0.0039216, 0.0039216, 0.0039216) + -tmp6.xyz;
                    tmp3.x = dot(tmp7.xyz, tmp7.xyz);
                    tmp5.z = tmp3.x < tmp5.x;
                    tmp6.xyz = tmp5.zzz ? tmp6.xyz : tmp4.xyz;
                    tmp5.zw = tmp5.zz ? tmp3.xy : tmp5.xy;
                    tmp4.xyz = tmp3.www ? tmp6.xyz : tmp4.xyz;
                    tmp5.xy = tmp3.ww ? tmp5.zw : tmp5.xy;
                }
                tmp0.xyz = -tmp2.xyz * float3(0.0039216, 0.0039216, 0.0039216) + tmp4.xyz;
                tmp0.x = dot(tmp0.xyz, tmp0.xyz);
                tmp0.yzw = -tmp2.xyz * float3(0.0039216, 0.0039216, 0.0039216) + tmp1.xyz;
                tmp0.y = dot(tmp0.xyz, tmp0.xyz);
                tmp0.xy = sqrt(tmp0.xy);
                tmp0.x = tmp0.x < tmp0.y;
                tmp0.y = tmp5.y != 0.0;
                tmp0.x = tmp0.y ? tmp0.x : 0.0;
                tmp4.w = 1.0;
                o.sv_target = tmp0.xxxx ? tmp4 : tmp1;
                return o;
			}
			ENDCG
		}
	}
}