Shader "psx/unlit/unlit" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_VertexWarpScale ("Vertex Warping Scalar", Range(0, 10)) = 1
		[Toggle] _Outline ("Assist Outline", Float) = 0
	}
	SubShader {
		LOD 200
		Tags { "LIGHTMODE" = "FORWARDBASE" "PASSFLAGS" = "OnlyDirectional" "RenderType" = "Opaque" }
		Pass {
			LOD 200
			Tags { "LIGHTMODE" = "FORWARDBASE" "PASSFLAGS" = "OnlyDirectional" "RenderType" = "Opaque" }
			GpuProgramID 24871
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float texcoord5 : TEXCOORD5;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float4 color : COLOR0;
				float4 color1 : COLOR1;
				float3 texcoord8 : TEXCOORD8;
				float3 texcoord9 : TEXCOORD9;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
				float2 sv_target1 : SV_Target1;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			float4 _Color;
			float4 unity_FogStart;
			float4 unity_FogEnd;
			float _VertexWarping;
			float _VertexWarpScale;
			float _TextureWarping;
			float4 _MeshScale;
			float4 _MeshCenter;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _MainTex_TexelSize;
			float _Outline;
			float _ShouldForceOutlines;
			float _ForceOutline;
			float _HasSandBuff;
			float _OiledAmount;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
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
                float4 tmp4;
                float4 tmp5;
                tmp0.x = max(_ScreenParams.y, _ScreenParams.x);
                tmp0.xy = _ScreenParams.xy / tmp0.xx;
                tmp1.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp1.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp1.xyz;
                tmp2.xyz = _WorldSpaceCameraPos - tmp1.xyz;
                tmp0.z = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = sqrt(tmp0.z);
                tmp0.z = rsqrt(tmp0.z);
                tmp2.xyz = tmp0.zzz * tmp2.xyz;
                tmp0.z = tmp0.w - 200.0;
                tmp0.w = unity_FogEnd.x - tmp0.w;
                tmp0.z = max(tmp0.z, 0.0);
                tmp0.z = log(tmp0.z);
                tmp0.z = tmp0.z * 0.2;
                tmp0.z = exp(tmp0.z);
                tmp0.z = tmp0.z * 0.1 + _VertexWarpScale;
                tmp0.z = tmp0.z * _VertexWarping;
                tmp0.xy = tmp0.zz * tmp0.xy;
                tmp3 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp3 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp3;
                tmp3 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp3;
                tmp3 = tmp3 + unity_MatrixVP._m03_m13_m23_m33;
                tmp4.xyz = tmp3.xyz / tmp3.www;
                tmp5.xy = tmp0.xy * tmp4.xy;
                tmp5.xy = floor(tmp5.xy);
                tmp5.xy = tmp5.xy + float2(0.5, 0.5);
                tmp4.xy = tmp5.xy / tmp0.xy;
                tmp0.xyz = tmp3.www * tmp4.xyz;
                tmp1.w = _VertexWarping != 0.0;
                o.position.xyz = tmp1.www ? tmp0.xyz : tmp3.xyz;
                o.position.w = tmp3.w;
                tmp0.x = max(tmp3.w, 0.02);
                tmp0.x = tmp0.x - 0.5;
                tmp0.y = min(_TextureWarping, 1.0);
                tmp0.y = tmp0.y * 0.5;
                tmp0.x = tmp0.y * tmp0.x + 0.5;
                tmp0.yz = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.texcoord.xy = tmp0.xx * tmp0.yz;
                o.texcoord5.x = tmp0.x;
                o.texcoord1.xyz = tmp1.xyz;
                tmp0.xyz = tmp1.xyz - _MeshCenter.xyz;
                tmp0.xyz = tmp0.xyz / _MeshScale.xyz;
                tmp0.xyz = tmp0.xyz + float3(0.5, 0.5, 0.5);
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp1.w = dot(-tmp2.xyz, tmp1.xyz);
                tmp1.w = tmp1.w + tmp1.w;
                tmp3.xyz = tmp1.xyz * -tmp1.www + -tmp2.xyz;
                tmp1.w = dot(tmp3.xyz, tmp3.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp4.xyz = tmp3.xyz * tmp1.www + -tmp3.xyz;
                tmp1.w = _TextureWarping + _TextureWarping;
                tmp1.w = min(tmp1.w, 1.0);
                o.texcoord2.xyz = tmp1.www * tmp4.xyz + tmp3.xyz;
                o.texcoord3.xyz = tmp1.xyz;
                tmp1.x = dot(tmp1.xyz, tmp2.xyz);
                o.texcoord4.xyz = tmp2.xyz;
                tmp1.x = 1.0 - abs(tmp1.x);
                tmp1.x = dot(tmp1.xy, tmp1.xy);
                o.texcoord4.w = min(tmp1.x, 1.0);
                o.color.xyz = v.color.xyz * _Color.xyz;
                o.color.w = v.color.w;
                tmp1.x = unity_FogEnd.x - unity_FogStart.x;
                o.color1.w = saturate(tmp0.w / tmp1.x);
                o.color1.xyz = unity_FogColor.xyz;
                tmp1.xyz = v.vertex.xyz - _MeshCenter.xyz;
                tmp1.xyz = tmp1.xyz / _MeshScale.xyz;
                tmp1.xyz = tmp1.xyz + float3(0.5, 0.5, 0.5);
                tmp0.w = _MeshCenter.w == 0.0;
                o.texcoord8.xyz = tmp0.www ? tmp1.xyz : tmp0.xyz;
                tmp0 = v.tangent.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.tangent.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.tangent.zzzz + tmp0;
                tmp0.w = dot(tmp0, tmp0);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord9.xyz = tmp0.www * tmp0.xyz;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp, float facing: VFACE)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                tmp0.xy = inp.texcoord.xy / inp.texcoord5.xx;
                tmp1 = tex2D(_MainTex, tmp0.xy);
                o.sv_target.w = tmp1.w * inp.color.w;
                tmp1.xyz = tmp1.xyz * inp.color.xyz + -inp.color1.xyz;
                tmp1.xyz = inp.color1.www * tmp1.xyz + inp.color1.xyz;
                tmp0.z = _HasSandBuff > 0.0;
                if (tmp0.z) {
                    tmp0.z = dot(inp.texcoord3.xyz, inp.texcoord3.xyz);
                    tmp0.z = rsqrt(tmp0.z);
                    tmp2.xyz = tmp0.zzz * inp.texcoord3.xyz;
                    tmp3 = _Time * float4(0.1, 0.5, 0.5, 0.1) + inp.texcoord1.xyyz;
                    tmp0.zw = max(abs(tmp2.zy), abs(tmp2.xx));
                    tmp0.zw = tmp0.zw < abs(tmp2.yz);
                    tmp2.xyz = tmp0.zzz ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                    tmp2.xyz = tmp0.www ? float3(0.0, 0.0, 1.0) : tmp2.xyz;
                    tmp4 = tmp2.zzxx * tmp3;
                    tmp0.zw = tmp4.zw + tmp4.xy;
                    tmp0.zw = tmp3.xw * tmp2.yy + tmp0.zw;
                    tmp0.zw = tmp0.zw * float2(0.25, 0.25);
                    tmp2 = tex2D(_MainTex, tmp0.zw);
                    tmp0.z = rsqrt(inp.texcoord4.w);
                    tmp0.z = 1.0 / tmp0.z;
                    tmp0.w = tmp0.z * 0.75 + 0.25;
                    tmp3.xyz = tmp2.xyz - tmp1.xyz;
                    tmp3.xyz = tmp0.www * tmp3.xyz + tmp1.xyz;
                    tmp2.xyz = tmp0.zzz * tmp2.xyz;
                    tmp1.xyz = tmp2.xyz * float3(0.75, 0.75, 0.75) + tmp3.xyz;
                }
                tmp0.z = _OiledAmount > 0.0;
                if (tmp0.z) {
                    tmp0.xy = tmp0.xy * _MainTex_TexelSize.zw;
                    tmp0.xy = tmp0.xy * float2(0.0078125, 0.0078125);
                    tmp0 = tex2D(_MainTex, tmp0.xy);
                    tmp0.x = saturate(tmp0.x);
                    tmp0.x = tmp0.x * 2.0 + -1.5;
                    tmp0.y = dot(inp.texcoord9.xyz, inp.texcoord9.xyz);
                    tmp0.y = rsqrt(tmp0.y);
                    tmp0.yzw = tmp0.yyy * inp.texcoord9.xyz;
                    tmp1.w = dot(-inp.texcoord4.xyz, tmp0.xyz);
                    tmp1.w = tmp1.w + tmp1.w;
                    tmp2.xy = tmp0.yz * -tmp1.ww + -inp.texcoord4.xy;
                    tmp0.y = dot(tmp0.xyz, inp.texcoord4.xyz);
                    tmp0.y = 1.0 - abs(tmp0.y);
                    tmp0.zw = tmp2.xy * float2(0.05, 0.05);
                    tmp2 = tex2D(_MainTex, tmp0.zw);
                    tmp0.z = tmp2.x * 5.0;
                    tmp0.w = tmp2.x * 0.01 + tmp0.y;
                    tmp0.w = max(tmp0.w, 0.01);
                    tmp0.w = min(tmp0.w, 0.99);
                    tmp2.x = tmp0.w * tmp0.w;
                    tmp2.y = tmp0.x * 2.0 + tmp0.z;
                    tmp2 = tex2D(_MainTex, tmp2.xy);
                    tmp0.y = log(tmp0.y);
                    tmp0.y = tmp0.y * 0.75;
                    tmp0.y = exp(tmp0.y);
                    tmp0.y = saturate(tmp0.y * 3.0 + -0.2);
                    tmp0.yzw = tmp2.xyz * tmp0.yyy;
                    tmp1.w = inp.texcoord8.y - _OiledAmount;
                    tmp1.w = tmp0.x * 0.4 + tmp1.w;
                    tmp1.w = _OiledAmount * -tmp1.w + tmp1.w;
                    tmp1.w = tmp1.w > 0.0;
                    tmp0.yzw = tmp1.www ? tmp1.xyz : tmp0.yzw;
                    tmp0.x = tmp0.x * 0.4 + -0.05;
                    tmp0.x = ceil(tmp0.x);
                    tmp0.x = tmp0.x * 0.1 + 0.25;
                    tmp2.xyz = tmp1.xyz - tmp0.yzw;
                    o.sv_target.xyz = tmp0.xxx * tmp2.xyz + tmp0.yzw;
                } else {
                    o.sv_target.xyz = tmp1.xyz;
                }
                tmp0.x = _ForceOutline * 0.5;
                o.sv_target1.x = max(tmp0.x, _Outline);
                o.sv_target1.y = _ShouldForceOutlines;
                return o;
			}
			ENDCG
		}
	}
}