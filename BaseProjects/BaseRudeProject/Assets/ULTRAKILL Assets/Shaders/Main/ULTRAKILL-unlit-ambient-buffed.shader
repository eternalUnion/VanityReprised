Shader "psx/unlit/ambient-buffed" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_BuffTex ("Buff Tex (RGB)", 2D) = "white" {}
		_VertexWarpScale ("Vertex Warping Scalar", Range(0, 10)) = 1
		[Toggle] _Outline ("Assist Outline", Float) = 0
		_InflateStrength ("Buff Inflate Strength", Range(0, 1)) = 0.1
	}
	SubShader {
		Pass {
			LOD 200
			Tags { "LIGHTMODE" = "FORWARDBASE" "PASSFLAGS" = "OnlyDirectional" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend 0 One OneMinusSrcAlpha, One OneMinusSrcAlpha
			Blend 1 OneMinusDstColor One, OneMinusDstColor One
			BlendOp 1 Max, Max
			ZWrite Off
			Cull Off
			GpuProgramID 43954
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
				float3 texcoord7 : TEXCOORD7;
				float3 texcoord8 : TEXCOORD8;
				float3 texcoord9 : TEXCOORD9;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
				float2 sv_target1 : SV_Target1;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _BuffTex_ST;
			float4 _Color;
			float4 unity_FogStart;
			float4 unity_FogEnd;
			float _VertexWarping;
			float _VertexWarpScale;
			float _TextureWarping;
			float _InflateStrength;
			float4 _MeshScale;
			float4 _MeshCenter;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _BuffTex_TexelSize;
			float _Outline;
			float _ShouldForceOutlines;
			float _ForceOutline;
			float _OiledAmount;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _SandTex;
			sampler2D _BuffTex;
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
                float4 tmp6;
                tmp0.x = max(_ScreenParams.y, _ScreenParams.x);
                tmp0.xy = _ScreenParams.xy / tmp0.xx;
                tmp1 = v.tangent.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * v.tangent.xxxx + tmp1;
                tmp1 = unity_ObjectToWorld._m02_m12_m22_m32 * v.tangent.zzzz + tmp1;
                tmp0.z = dot(tmp1, tmp1);
                tmp0.z = rsqrt(tmp0.z);
                tmp1.xyz = tmp0.zzz * tmp1.xyz;
                tmp2.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp2.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp2.xyz;
                tmp2.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp2.xyz;
                tmp2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp2.xyz;
                tmp3.xyz = tmp1.xyz * _InflateStrength.xxx + tmp2.xyz;
                o.texcoord9.xyz = tmp1.xyz;
                tmp1.xyz = tmp2.xyz - _MeshCenter.xyz;
                tmp1.xyz = tmp1.xyz / _MeshScale.xyz;
                tmp1.xyz = tmp1.xyz + float3(0.5, 0.5, 0.5);
                tmp2.xyz = _WorldSpaceCameraPos - tmp3.xyz;
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
                tmp4 = tmp3.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp4 = unity_MatrixVP._m00_m10_m20_m30 * tmp3.xxxx + tmp4;
                tmp4 = unity_MatrixVP._m02_m12_m22_m32 * tmp3.zzzz + tmp4;
                tmp4 = tmp4 + unity_MatrixVP._m03_m13_m23_m33;
                tmp5.xyz = tmp4.xyz / tmp4.www;
                tmp6.xy = tmp0.xy * tmp5.xy;
                tmp6.xy = floor(tmp6.xy);
                tmp6.xy = tmp6.xy + float2(0.5, 0.5);
                tmp5.xy = tmp6.xy / tmp0.xy;
                tmp0.xyz = tmp4.www * tmp5.xyz;
                tmp1.w = _VertexWarping != 0.0;
                o.position.xyz = tmp1.www ? tmp0.xyz : tmp4.xyz;
                o.position.w = tmp4.w;
                tmp0.x = max(tmp4.w, 0.02);
                tmp0.x = tmp0.x - 0.5;
                tmp0.y = min(_TextureWarping, 1.0);
                tmp0.y = tmp0.y * 0.5;
                tmp0.x = tmp0.y * tmp0.x + 0.5;
                tmp0.yz = v.texcoord.xy * _BuffTex_ST.xy + _BuffTex_ST.zw;
                o.texcoord.xy = tmp0.xx * tmp0.yz;
                o.texcoord5.x = tmp0.x;
                o.texcoord1.xyz = tmp3.xyz;
                o.texcoord7.xyz = tmp3.xyz - unity_ObjectToWorld._m03_m13_m23;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp1.w = dot(tmp0.xyz, tmp0.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp0.xyz = tmp0.xyz * tmp1.www;
                tmp1.w = dot(-tmp2.xyz, tmp0.xyz);
                tmp1.w = tmp1.w + tmp1.w;
                tmp3.xyz = tmp0.xyz * -tmp1.www + -tmp2.xyz;
                tmp1.w = dot(tmp3.xyz, tmp3.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp4.xyz = tmp3.xyz * tmp1.www + -tmp3.xyz;
                tmp1.w = _TextureWarping + _TextureWarping;
                tmp1.w = min(tmp1.w, 1.0);
                o.texcoord2.xyz = tmp1.www * tmp4.xyz + tmp3.xyz;
                o.texcoord3.xyz = tmp0.xyz;
                tmp0.x = dot(tmp0.xyz, tmp2.xyz);
                o.texcoord4.xyz = tmp2.xyz;
                tmp0.x = 1.0 - abs(tmp0.x);
                tmp0.x = dot(tmp0.xy, tmp0.xy);
                o.texcoord4.w = min(tmp0.x, 1.0);
                o.color.xyz = v.color.xyz * _Color.xyz;
                o.color.w = v.color.w;
                tmp0.x = unity_FogEnd.x - unity_FogStart.x;
                o.color1.w = saturate(tmp0.w / tmp0.x);
                o.color1.xyz = unity_FogColor.xyz;
                tmp0.xyz = v.vertex.xyz - _MeshCenter.xyz;
                tmp0.xyz = tmp0.xyz / _MeshScale.xyz;
                tmp0.xyz = tmp0.xyz + float3(0.5, 0.5, 0.5);
                tmp0.w = _MeshCenter.w == 0.0;
                o.texcoord8.xyz = tmp0.www ? tmp0.xyz : tmp1.xyz;
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
                tmp0.x = dot(inp.texcoord3.xyz, inp.texcoord3.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp0.x = tmp0.x * inp.texcoord3.y;
                tmp0.y = min(abs(inp.texcoord7.z), abs(inp.texcoord7.x));
                tmp0.z = max(abs(inp.texcoord7.z), abs(inp.texcoord7.x));
                tmp0.z = 1.0 / tmp0.z;
                tmp0.y = tmp0.z * tmp0.y;
                tmp0.z = tmp0.y * tmp0.y;
                tmp0.w = tmp0.z * 0.0208351 + -0.085133;
                tmp0.w = tmp0.z * tmp0.w + 0.180141;
                tmp0.w = tmp0.z * tmp0.w + -0.3302995;
                tmp0.z = tmp0.z * tmp0.w + 0.999866;
                tmp0.w = tmp0.z * tmp0.y;
                tmp1.x = abs(inp.texcoord7.z) < abs(inp.texcoord7.x);
                tmp0.w = tmp0.w * -2.0 + 1.570796;
                tmp0.w = tmp1.x ? tmp0.w : 0.0;
                tmp0.y = tmp0.y * tmp0.z + tmp0.w;
                tmp0.z = inp.texcoord7.z < -inp.texcoord7.z;
                tmp0.z = tmp0.z ? -3.141593 : 0.0;
                tmp0.y = tmp0.z + tmp0.y;
                tmp0.z = min(inp.texcoord7.z, inp.texcoord7.x);
                tmp0.w = max(inp.texcoord7.z, inp.texcoord7.x);
                tmp0.z = tmp0.z < -tmp0.z;
                tmp0.w = tmp0.w >= -tmp0.w;
                tmp0.z = tmp0.w ? tmp0.z : 0.0;
                tmp0.y = tmp0.z ? -tmp0.y : tmp0.y;
                tmp1.x = tmp0.y * 2.0;
                tmp0.yzw = inp.texcoord7.xzy * float3(0.25, 0.25, 0.25);
                tmp1.yw = _Time.yy * float2(0.5, 0.5) + tmp0.wz;
                tmp2 = tex2D(_BuffTex, tmp1.xy);
                tmp1.z = tmp0.y;
                tmp1 = tex2D(_BuffTex, tmp1.zw);
                tmp0.x = saturate(abs(tmp0.x) * 5.0 + -3.0);
                tmp0.yzw = tmp1.xyz - tmp2.xyz;
                tmp0.xyz = tmp0.xxx * tmp0.yzw + tmp2.xyz;
                tmp0.w = inp.texcoord4.w * inp.texcoord4.w;
                tmp1.x = tmp0.w * 0.75 + 0.25;
                tmp1.y = facing.x ? 0.5 : 1.5;
                tmp0.w = tmp0.w * tmp1.y;
                tmp1.w = tmp1.y * tmp1.x;
                tmp2.xyz = tmp0.www * tmp0.xyz;
                tmp2.xyz = tmp2.xyz * float3(0.75, 0.75, 0.75);
                tmp1.xyz = tmp1.www * tmp0.xyz + tmp2.xyz;
                tmp0.x = _OiledAmount > 0.0;
                if (tmp0.x) {
                    tmp0.xy = inp.texcoord.xy / inp.texcoord5.xx;
                    tmp0.xy = tmp0.xy * _BuffTex_TexelSize.zw;
                    tmp0.xy = tmp0.xy * float2(0.0078125, 0.0078125);
                    tmp0 = tex2D(_BuffTex, tmp0.xy);
                    tmp0.x = saturate(tmp0.x);
                    tmp0.x = tmp0.x * 2.0 + -1.5;
                    tmp0.y = dot(inp.texcoord9.xyz, inp.texcoord9.xyz);
                    tmp0.y = rsqrt(tmp0.y);
                    tmp0.yzw = tmp0.yyy * inp.texcoord9.xyz;
                    tmp2.x = dot(-inp.texcoord4.xyz, tmp0.xyz);
                    tmp2.x = tmp2.x + tmp2.x;
                    tmp2.xy = tmp0.yz * -tmp2.xx + -inp.texcoord4.xy;
                    tmp0.y = dot(tmp0.xyz, inp.texcoord4.xyz);
                    tmp0.y = 1.0 - abs(tmp0.y);
                    tmp0.zw = tmp2.xy * float2(0.05, 0.05);
                    tmp2 = tex2D(_BuffTex, tmp0.zw);
                    tmp0.z = tmp2.x * 0.01 + tmp0.y;
                    tmp0.z = max(tmp0.z, 0.01);
                    tmp0.z = min(tmp0.z, 0.99);
                    tmp3.x = tmp0.z * tmp0.z;
                    tmp0.z = tmp0.x * 2.0;
                    tmp3.y = tmp2.x * 5.0 + tmp0.z;
                    tmp2 = tex2D(_BuffTex, tmp3.xy);
                    tmp0.y = log(tmp0.y);
                    tmp0.y = tmp0.y * 0.75;
                    tmp0.y = exp(tmp0.y);
                    tmp0.y = saturate(tmp0.y * 3.0 + -0.2);
                    tmp0.yzw = tmp2.xyz * tmp0.yyy;
                    tmp2.x = inp.texcoord8.y - _OiledAmount;
                    tmp2.x = tmp0.x * 0.4 + tmp2.x;
                    tmp2.x = _OiledAmount * -tmp2.x + tmp2.x;
                    tmp2.x = tmp2.x > 0.0;
                    tmp0.yzw = tmp2.xxx ? tmp1.xyz : tmp0.yzw;
                    tmp0.x = tmp0.x * 0.4 + -0.05;
                    tmp0.x = ceil(tmp0.x);
                    tmp0.x = tmp0.x * 0.1 + 0.25;
                    tmp2.xyz = tmp1.xyz - tmp0.yzw;
                    tmp1.xyz = tmp0.xxx * tmp2.xyz + tmp0.yzw;
                }
                tmp0.x = _ForceOutline * 0.5;
                o.sv_target1.x = max(tmp0.x, _Outline);
                o.sv_target = tmp1;
                o.sv_target1.y = _ShouldForceOutlines;
                return o;
			}
			ENDCG
		}
	}
}