Shader "ULTRAKILL/BakeVertexLights" {
	Properties {
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			Tags { "RenderType" = "Opaque" }
			GpuProgramID 17720
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			int _LightCount;
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			UNITY_DECLARE_TEX2DARRAY(_DirectionalShadows);
			UNITY_DECLARE_TEXCUBEARRAY(_PointSpotShadows);
			// Texture params for Fragment Shader
			
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
                float4 tmp9;
                float4 tmp10;
                float4 tmp11;
                float4 tmp12;
                float4 tmp13;
                float4 tmp14;
                float4 tmp15;
                float4 tmp16;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp2 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp2 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp2;
                tmp2 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp2;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp2;
                tmp0.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp1 = v.normal.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * v.normal.xxxx + tmp1;
                tmp1 = unity_ObjectToWorld._m02_m12_m22_m32 * v.normal.zzzz + tmp1;
                tmp1.w = dot(tmp1, tmp1);
                tmp1.w = rsqrt(tmp1.w);
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp0.w = 1.0;
                tmp2.z = 0.0;
                tmp3 = float4(0.0, 0.0, 0.0, 0.0);
                for (int i = tmp3.w; i < _LightCount; i += 1) {
                    tmp4 = ((float4[1])rsc2.Load(i))[11];
                    tmp1.w = tmp4.y != 1.0;
                    if (tmp1.w) {
                        tmp5 = ((float4[1])rsc2.Load(i))[0];
                        tmp6 = ((float4[1])rsc2.Load(i))[1];
                        tmp7 = ((float4[1])rsc2.Load(i))[2];
                        tmp8 = ((float4[1])rsc2.wxyz.Load(i))[3];
                        tmp9.x = tmp5.w;
                        tmp9.yz = tmp6.xy;
                        tmp6.xyz = -tmp0.xyz * tmp6.zzz + tmp9.xyz;
                        tmp1.w = dot(tmp6.xyz, tmp6.xyz);
                        tmp1.w = max(tmp1.w, 0.000001);
                        tmp2.w = rsqrt(tmp1.w);
                        tmp6.xyz = tmp2.www * tmp6.xyz;
                        tmp1.w = tmp1.w * tmp7.y + 1.0;
                        tmp1.w = 1.0 / tmp1.w;
                        tmp10.xy = tmp7.zw;
                        tmp10.z = tmp8.y;
                        tmp2.w = dot(tmp6.xyz, tmp10.xyz);
                        tmp2.w = max(tmp2.w, 0.0);
                        tmp2.w = tmp2.w - tmp6.w;
                        tmp2.w = saturate(tmp7.x * tmp2.w);
                        tmp1.w = tmp1.w * tmp2.w;
                        tmp2.w = dot(tmp1.xyz, tmp6.xyz);
                        tmp2.w = max(tmp2.w, 0.0);
                        tmp1.w = tmp1.w * tmp2.w;
                        tmp5.xyz = tmp1.www * tmp5.xyz;
                        tmp1.w = tmp4.z == 1.0;
                        if (tmp1.w) {
                            tmp6 = ((float4[1])rsc2.Load(i))[4];
                            tmp7 = ((float4[1])rsc2.yxzw.Load(i))[5];
                            tmp10 = ((float4[1])rsc2.Load(i))[6];
                            tmp11.xyz = ((float4[1])rsc2.zxyx.Load(i))[7];
                            tmp12.xyz = ((float4[1])rsc2.xyzx.Load(i))[8];
                            tmp13.xyz = ((float4[1])rsc2.xyzx.Load(i))[9];
                            tmp14.xyz = ((float4[1])rsc2.yzxx.Load(i))[10];
                            tmp15.x = tmp8.z;
                            tmp15.y = tmp6.y;
                            tmp15.z = tmp7.x;
                            tmp15.w = tmp10.y;
                            tmp15.x = dot(tmp15, tmp0);
                            tmp16.x = tmp8.w;
                            tmp16.y = tmp6.z;
                            tmp16.z = tmp7.z;
                            tmp16.w = tmp10.z;
                            tmp15.y = dot(tmp16, tmp0);
                            tmp8.y = tmp6.w;
                            tmp8.z = tmp7.w;
                            tmp8.w = tmp10.w;
                            tmp15.z = dot(tmp8, tmp0);
                            tmp7.x = tmp6.x;
                            tmp7.z = tmp10.x;
                            tmp7.w = tmp11.y;
                            tmp15.w = dot(tmp7, tmp0);
                            tmp6.x = tmp11.z;
                            tmp6.y = tmp12.y;
                            tmp6.z = tmp13.y;
                            tmp6.w = tmp14.x;
                            tmp6.x = dot(tmp6, tmp15);
                            tmp11.y = tmp12.z;
                            tmp11.z = tmp13.z;
                            tmp11.w = tmp14.y;
                            tmp6.y = dot(tmp11, tmp15);
                            tmp14.x = tmp12.x;
                            tmp14.y = tmp13.x;
                            tmp14.w = tmp4.x;
                            tmp1.w = dot(tmp14, tmp15);
                            tmp6.xy = tmp6.xy / tmp1.ww;
                            tmp2.xy = tmp6.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                            tmp1.w = tmp15.z - tmp10.w;
                            tmp2.x = tex2Dlod(_MainTex, float4(tmp2.xyz, tmp4.w));
                            tmp1.w = tmp2.x - tmp1.w;
                            tmp1.w = tmp1.w < 1.0;
                            tmp1.w = tmp1.w ? 1.0 : 0.0;
                            tmp5.xyz = tmp1.www * tmp5.xyz;
                        } else {
                            tmp1.w = tmp4.z == 2.0;
                            if (tmp1.w) {
                                tmp2.xyw = tmp0.xyz - tmp9.xyz;
                                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                                tmp5.w = rsqrt(tmp1.w);
                                tmp4.xyz = tmp2.xyw * tmp5.www;
                                tmp2.x = tex2Dlod(_MainTex, float4(tmp4, 0.0));
                                tmp1.w = sqrt(tmp1.w);
                                tmp2.x = tmp2.x + 0.1;
                                tmp1.w = tmp1.w >= tmp2.x;
                                tmp1.w = tmp1.w ? 0.0 : 1.0;
                                tmp5.xyz = tmp1.www * tmp5.xyz;
                            }
                        }
                        tmp3.xyz = tmp3.xyz + tmp5.xyz;
                    }
                }
                tmp3.w = 50.0;
                //unsupported_store_structured;
                o.color = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                o.sv_target = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			ENDCG
		}
	}
}