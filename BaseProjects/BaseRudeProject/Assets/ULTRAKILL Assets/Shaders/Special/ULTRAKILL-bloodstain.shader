Shader "ULTRAKILL/bloodstain" {
	Properties {
		_SplatterAtlas ("Atlas", 2D) = "white" {}
		_VertexWarpScale ("Vertex Warping Scalar", Range(0, 10)) = 1
	}
	SubShader {
		Tags { "LIGHTMODE" = "FORWARDBASE" "PASSFLAGS" = "OnlyDirectional" "QUEUE" = "Transparent-1" "RenderType" = "Transparent" }
		Pass {
			Tags { "LIGHTMODE" = "FORWARDBASE" "PASSFLAGS" = "OnlyDirectional" "QUEUE" = "Transparent-1" "RenderType" = "Transparent" }
			Blend SrcAlpha SrcColor, SrcAlpha SrcColor
			ZWrite Off
			GpuProgramID 55632
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float texcoord1 : TEXCOORD1;
				float4 position : SV_POSITION0;
				float3 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float3 texcoord4 : TEXCOORD4;
				float3 texcoord5 : TEXCOORD5;
				float4 color1 : COLOR1;
			};
			struct fout
			{
				float4 sv_target : SV_TARGET0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 unity_FogEnd;
			float4 unity_FogStart;
			float _VertexWarping;
			float _VertexWarpScale;
			float _TextureWarping;
			// $Globals ConstantBuffers for Fragment Shader
			float _IsOil;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _SplatterAtlas;
			sampler2D _SandTex;
			sampler2D _OilSlick;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0.x = min(_TextureWarping, 1.0);
                tmp0.x = tmp0.x * 0.5;
                tmp1 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp1;
                tmp1 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp1;
                tmp2 = tmp1 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp0.yzw = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp1.xyz;
                tmp1 = tmp2.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp2.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp2.zzzz + tmp1;
                tmp1 = unity_MatrixVP._m03_m13_m23_m33 * tmp2.wwww + tmp1;
                tmp3.x = max(tmp1.w, 0.02);
                tmp3.x = tmp3.x - 0.5;
                tmp0.x = tmp0.x * tmp3.x + 0.5;
                o.texcoord.xy = tmp0.xx * v.texcoord.xy;
                o.texcoord1.x = tmp0.x;
                tmp3.xyz = tmp2.yyy * unity_MatrixV._m01_m11_m21;
                tmp3.xyz = unity_MatrixV._m00_m10_m20 * tmp2.xxx + tmp3.xyz;
                tmp2.xyz = unity_MatrixV._m02_m12_m22 * tmp2.zzz + tmp3.xyz;
                tmp2.xyz = unity_MatrixV._m03_m13_m23 * tmp2.www + tmp2.xyz;
                tmp0.x = dot(tmp2.xyz, tmp2.xyz);
                tmp0.x = sqrt(tmp0.x);
                tmp2.x = tmp0.x - 200.0;
                tmp0.x = unity_FogEnd.x - tmp0.x;
                tmp2.x = max(tmp2.x, 0.0);
                tmp2.x = log(tmp2.x);
                tmp2.x = tmp2.x * 0.2;
                tmp2.x = exp(tmp2.x);
                tmp2.x = tmp2.x * 0.1 + _VertexWarpScale;
                tmp2.x = tmp2.x * _VertexWarping;
                tmp2.y = max(_ScreenParams.y, _ScreenParams.x);
                tmp2.yz = _ScreenParams.xy / tmp2.yy;
                tmp2.xy = tmp2.xx * tmp2.yz;
                tmp3.xyz = tmp1.xyz / tmp1.www;
                tmp2.zw = tmp2.xy * tmp3.xy;
                tmp2.zw = floor(tmp2.zw);
                tmp2.zw = tmp2.zw + float2(0.5, 0.5);
                tmp3.xy = tmp2.zw / tmp2.xy;
                tmp2.xyz = tmp1.www * tmp3.xyz;
                tmp2.w = _VertexWarping != 0.0;
                o.position.xyz = tmp2.www ? tmp2.xyz : tmp1.xyz;
                o.position.w = tmp1.w;
                tmp1.x = _TextureWarping + _TextureWarping;
                tmp1.x = min(tmp1.x, 1.0);
                tmp2 = v.normal.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp2 = unity_ObjectToWorld._m00_m10_m20_m30 * v.normal.xxxx + tmp2;
                tmp2 = unity_ObjectToWorld._m02_m12_m22_m32 * v.normal.zzzz + tmp2;
                tmp1.y = dot(tmp2, tmp2);
                tmp1.y = rsqrt(tmp1.y);
                tmp1.yzw = tmp1.yyy * tmp2.xyz;
                tmp2.xyz = _WorldSpaceCameraPos - tmp0.yzw;
                o.texcoord4.xyz = tmp0.yzw;
                tmp0.y = dot(-tmp2.xyz, tmp1.xyz);
                tmp0.y = tmp0.y + tmp0.y;
                tmp0.yzw = tmp1.yzw * -tmp0.yyy + -tmp2.xyz;
                tmp2.w = dot(tmp0.xyz, tmp0.xyz);
                tmp2.w = rsqrt(tmp2.w);
                tmp3.xyz = tmp0.yzw * tmp2.www + -tmp0.yzw;
                o.texcoord2.xyz = tmp1.xxx * tmp3.xyz + tmp0.yzw;
                tmp0.y = dot(tmp1.xyz, tmp1.xyz);
                tmp0.y = rsqrt(tmp0.y);
                tmp0.yzw = tmp0.yyy * tmp1.yzw;
                o.texcoord5.xyz = tmp1.yzw;
                tmp1.x = dot(tmp2.xyz, tmp2.xyz);
                tmp1.x = rsqrt(tmp1.x);
                tmp1.xyz = tmp1.xxx * tmp2.xyz;
                o.texcoord3.xyz = tmp2.xyz;
                tmp0.y = dot(tmp0.xyz, tmp1.xyz);
                tmp0.y = 1.0 - abs(tmp0.y);
                tmp0.y = dot(tmp0.xy, tmp0.xy);
                o.texcoord3.w = min(tmp0.y, 1.0);
                tmp0.y = unity_FogEnd.x - unity_FogStart.x;
                o.color1.w = saturate(tmp0.x / tmp0.y);
                o.color1.xyz = unity_FogColor.xyz;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xy = inp.texcoord.xy / inp.texcoord1.xx;
                tmp0.xy = tmp0.xy * float2(0.2, 1.0);
                tmp0 = tex2D(_SplatterAtlas, tmp0.xy);
                tmp0.y = tmp0.w != 1.0;
                if (tmp0.y) {
                    discard;
                }
                tmp0.y = _IsOil != 0.0;
                if (tmp0.y) {
                    tmp1 = inp.texcoord4.xyyz + float4(0.1, 0.5, 0.5, 0.1);
                    tmp0.y = dot(inp.texcoord5.xyz, inp.texcoord5.xyz);
                    tmp0.y = rsqrt(tmp0.y);
                    tmp0.yzw = tmp0.yyy * inp.texcoord5.xyz;
                    tmp2.xy = max(abs(tmp0.wz), abs(tmp0.yy));
                    tmp0.yz = tmp2.xy < abs(tmp0.zw);
                    tmp2.xyz = tmp0.yyy ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                    tmp0.yzw = tmp0.zzz ? float3(0.0, 0.0, 1.0) : tmp2.xyz;
                    tmp2 = tmp0.wwyy * tmp1;
                    tmp0.yw = tmp2.zw + tmp2.xy;
                    tmp0.yz = tmp1.xw * tmp0.zz + tmp0.yw;
                    tmp0.yz = tmp0.yz * float2(0.1, 0.1);
                    tmp1 = tex2Dlod(_SplatterAtlas, float4(tmp0.yz, 0, 0.0));
                    tmp2.x = tmp1.x + inp.texcoord3.w;
                    tmp2.y = tmp1.x * 5.0;
                    tmp1 = tex2Dlod(_SplatterAtlas, float4(tmp2.xy, 0, 0.0));
                    tmp0.x = tmp0.x * tmp0.x;
                    tmp0.xyz = tmp1.xyz * tmp0.xxx + float3(0.32, 0.0, 0.55);
                } else {
                    tmp0.xyz = float3(1.0, 0.0, 0.0);
                }
                o.sv_target.xyz = tmp0.xyz;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
	}
}