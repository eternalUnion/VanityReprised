// Upgrade NOTE: replaced 'glstate_matrix_projection' with 'UNITY_MATRIX_P'

Shader "psx/unlit/transparent/nocull-nofog-billboard" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_OpacScale ("Transparency Scalar", Range(0, 1)) = 1
		_VertexWarpScale ("Vertex Warping Scalar", Range(0, 10)) = 1
		_DepthOffset ("Depth Offset Scalar", Float) = 1
	}
	SubShader {
		LOD 200
		Tags { "LIGHTMODE" = "FORWARDBASE" "PASSFLAGS" = "OnlyDirectional" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			LOD 200
			Tags { "LIGHTMODE" = "FORWARDBASE" "PASSFLAGS" = "OnlyDirectional" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
			GpuProgramID 63538
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
			float _VertexWarping;
			float _VertexWarpScale;
			float _TextureWarping;
			float4 _MeshScale;
			float4 _MeshCenter;
			float _DepthOffset;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _MainTex_TexelSize;
			float _Outline;
			float _ShouldForceOutlines;
			float _ForceOutline;
			float _HasSandBuff;
			float _OiledAmount;
			float _OpacScale;
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
                tmp0.xyz = unity_ObjectToWorld._m13_m13_m13 * unity_MatrixV._m01_m11_m21;
                tmp0.xyz = unity_MatrixV._m00_m10_m20 * unity_ObjectToWorld._m03_m03_m03 + tmp0.xyz;
                tmp0.xyz = unity_MatrixV._m02_m12_m22 * unity_ObjectToWorld._m23_m23_m23 + tmp0.xyz;
                tmp0.xyz = unity_MatrixV._m03_m13_m23 * unity_ObjectToWorld._m33_m33_m33 + tmp0.xyz;
                tmp0.w = dot(unity_ObjectToWorld._m00_m10_m20, unity_ObjectToWorld._m00_m10_m20);
                tmp1.x = sqrt(tmp0.w);
                tmp0.w = dot(unity_ObjectToWorld._m01_m11_m21, unity_ObjectToWorld._m01_m11_m21);
                tmp1.y = sqrt(tmp0.w);
                tmp2.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp2.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp2.xyz;
                tmp2.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp2.xyz;
                tmp2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp2.xyz;
                tmp1.zw = tmp2.yy * unity_WorldToObject._m01_m11;
                tmp1.zw = unity_WorldToObject._m00_m10 * tmp2.xx + tmp1.zw;
                tmp1.zw = unity_WorldToObject._m02_m12 * tmp2.zz + tmp1.zw;
                tmp1.zw = tmp1.zw + unity_WorldToObject._m03_m13;
                tmp1.xy = tmp1.xy * tmp1.zw;
                tmp1.z = 0.0;
                tmp0.xyz = tmp0.xyz + tmp1.xyz;
                tmp1 = tmp0.yyyy * UNITY_MATRIX_P._m01_m11_m21_m31;
                tmp1 = UNITY_MATRIX_P._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = UNITY_MATRIX_P._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                tmp0 = UNITY_MATRIX_P._m03_m13_m23_m33 * _DepthOffset.xxxx + tmp0;
                tmp1.xyz = tmp0.xyz / tmp0.www;
                tmp3.xyz = _WorldSpaceCameraPos - tmp2.xyz;
                tmp1.w = dot(tmp3.xyz, tmp3.xyz);
                tmp2.w = sqrt(tmp1.w);
                tmp1.w = rsqrt(tmp1.w);
                tmp3.xyz = tmp1.www * tmp3.xyz;
                tmp1.w = tmp2.w - 200.0;
                tmp1.w = max(tmp1.w, 0.0);
                tmp1.w = log(tmp1.w);
                tmp1.w = tmp1.w * 0.2;
                tmp1.w = exp(tmp1.w);
                tmp1.w = tmp1.w * 0.1 + _VertexWarpScale;
                tmp1.w = tmp1.w * _VertexWarping;
                tmp2.w = max(_ScreenParams.y, _ScreenParams.x);
                tmp4.xy = _ScreenParams.xy / tmp2.ww;
                tmp4.xy = tmp1.ww * tmp4.xy;
                tmp4.zw = tmp1.xy * tmp4.xy;
                tmp4.zw = floor(tmp4.zw);
                tmp4.zw = tmp4.zw + float2(0.5, 0.5);
                tmp1.xy = tmp4.zw / tmp4.xy;
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp1.w = _VertexWarping != 0.0;
                o.position.xyz = tmp1.www ? tmp1.xyz : tmp0.xyz;
                o.position.w = tmp0.w;
                tmp0.x = max(tmp0.w, 0.02);
                tmp0.x = tmp0.x - 0.5;
                tmp0.y = min(_TextureWarping, 1.0);
                tmp0.y = tmp0.y * 0.5;
                tmp0.x = tmp0.y * tmp0.x + 0.5;
                tmp0.yz = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.texcoord.xy = tmp0.xx * tmp0.yz;
                o.texcoord5.x = tmp0.x;
                o.texcoord1.xyz = tmp2.xyz;
                tmp0.xyz = tmp2.xyz - _MeshCenter.xyz;
                tmp0.xyz = tmp0.xyz / _MeshScale.xyz;
                tmp0.xyz = tmp0.xyz + float3(0.5, 0.5, 0.5);
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.w = dot(-tmp3.xyz, tmp1.xyz);
                tmp0.w = tmp0.w + tmp0.w;
                tmp2.xyz = tmp1.xyz * -tmp0.www + -tmp3.xyz;
                tmp0.w = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp4.xyz = tmp2.xyz * tmp0.www + -tmp2.xyz;
                tmp0.w = _TextureWarping + _TextureWarping;
                tmp0.w = min(tmp0.w, 1.0);
                o.texcoord2.xyz = tmp0.www * tmp4.xyz + tmp2.xyz;
                o.texcoord3.xyz = tmp1.xyz;
                tmp0.w = dot(tmp1.xyz, tmp3.xyz);
                o.texcoord4.xyz = tmp3.xyz;
                tmp0.w = 1.0 - abs(tmp0.w);
                tmp0.w = dot(tmp0.xy, tmp0.xy);
                o.texcoord4.w = min(tmp0.w, 1.0);
                o.color.xyz = v.color.xyz * _Color.xyz;
                o.color.w = v.color.w;
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
                float4 tmp5;
                tmp0.xy = inp.texcoord.xy / inp.texcoord5.xx;
                tmp1 = tex2D(_MainTex, tmp0.xy);
                tmp2 = tmp1.wxyz * inp.color.wxyz;
                tmp2.x = saturate(tmp2.x);
                o.sv_target.w = tmp2.x * _OpacScale;
                tmp0.z = _HasSandBuff > 0.0;
                if (tmp0.z) {
                    tmp0.z = dot(inp.texcoord3.xyz, inp.texcoord3.xyz);
                    tmp0.z = rsqrt(tmp0.z);
                    tmp3.xyz = tmp0.zzz * inp.texcoord3.xyz;
                    tmp4 = _Time * float4(0.1, 0.5, 0.5, 0.1) + inp.texcoord1.xyyz;
                    tmp0.zw = max(abs(tmp3.zy), abs(tmp3.xx));
                    tmp0.zw = tmp0.zw < abs(tmp3.yz);
                    tmp3.xyz = tmp0.zzz ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                    tmp3.xyz = tmp0.www ? float3(0.0, 0.0, 1.0) : tmp3.xyz;
                    tmp5 = tmp3.zzxx * tmp4;
                    tmp0.zw = tmp5.zw + tmp5.xy;
                    tmp0.zw = tmp4.xw * tmp3.yy + tmp0.zw;
                    tmp0.zw = tmp0.zw * float2(0.25, 0.25);
                    tmp3 = tex2D(_MainTex, tmp0.zw);
                    tmp0.z = rsqrt(inp.texcoord4.w);
                    tmp0.z = 1.0 / tmp0.z;
                    tmp0.w = tmp0.z * 0.75 + 0.25;
                    tmp1.xyz = -tmp1.xyz * inp.color.xyz + tmp3.xyz;
                    tmp1.xyz = tmp0.www * tmp1.xyz + tmp2.yzw;
                    tmp3.xyz = tmp0.zzz * tmp3.xyz;
                    tmp2.yzw = tmp3.xyz * float3(0.75, 0.75, 0.75) + tmp1.xyz;
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
                    tmp1.x = dot(-inp.texcoord4.xyz, tmp0.xyz);
                    tmp1.x = tmp1.x + tmp1.x;
                    tmp1.xy = tmp0.yz * -tmp1.xx + -inp.texcoord4.xy;
                    tmp0.y = dot(tmp0.xyz, inp.texcoord4.xyz);
                    tmp0.y = 1.0 - abs(tmp0.y);
                    tmp0.zw = tmp1.xy * float2(0.05, 0.05);
                    tmp1 = tex2D(_MainTex, tmp0.zw);
                    tmp0.z = tmp1.x * 5.0;
                    tmp0.w = tmp1.x * 0.01 + tmp0.y;
                    tmp0.w = max(tmp0.w, 0.01);
                    tmp0.w = min(tmp0.w, 0.99);
                    tmp1.x = tmp0.w * tmp0.w;
                    tmp1.y = tmp0.x * 2.0 + tmp0.z;
                    tmp1 = tex2D(_MainTex, tmp1.xy);
                    tmp0.y = log(tmp0.y);
                    tmp0.y = tmp0.y * 0.75;
                    tmp0.y = exp(tmp0.y);
                    tmp0.y = saturate(tmp0.y * 3.0 + -0.2);
                    tmp0.yzw = tmp1.xyz * tmp0.yyy;
                    tmp1.x = inp.texcoord8.y - _OiledAmount;
                    tmp1.x = tmp0.x * 0.4 + tmp1.x;
                    tmp1.x = _OiledAmount * -tmp1.x + tmp1.x;
                    tmp1.x = tmp1.x > 0.0;
                    tmp0.yzw = tmp1.xxx ? tmp2.yzw : tmp0.yzw;
                    tmp0.x = tmp0.x * 0.4 + -0.05;
                    tmp0.x = ceil(tmp0.x);
                    tmp0.x = tmp0.x * 0.1 + 0.25;
                    tmp1.xyz = tmp2.yzw - tmp0.yzw;
                    o.sv_target.xyz = tmp0.xxx * tmp1.xyz + tmp0.yzw;
                } else {
                    o.sv_target.xyz = tmp2.yzw;
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