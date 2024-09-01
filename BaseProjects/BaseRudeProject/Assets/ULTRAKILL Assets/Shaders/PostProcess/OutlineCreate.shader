Shader "Unlit/OutlineCreate" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		LOD 100
		Tags { "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "RenderType" = "Opaque" }
			GpuProgramID 15160
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float2 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			Texture2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
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
                tmp0.xy = asint(inp.position.xy);
                tmp0.zw = float2(0.0, 0.0);
                tmp0 = _MainTex.Load(tmp0.xyz);
                tmp0.y = min(tmp0.y, tmp0.x);
                tmp0.x = floor(tmp0.x);
                tmp0.y = ceil(tmp0.y);
                tmp0.x = max(tmp0.y, tmp0.x);
                tmp0.x = tmp0.x < 1.0;
                o.sv_target.xy = tmp0.xx ? float2(1.0, 1.0) : float2(0.516129, 0.516129);
                return o;
			}
			ENDCG
		}
		Pass {
			LOD 100
			Tags { "RenderType" = "Opaque" }
			GpuProgramID 123627
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float2 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float2 _Resolution;
			float _Distance;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			Texture2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
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
                tmp0.zw = float2(0.0, 0.0);
                tmp1.xy = trunc(inp.position.xy);
                tmp1.zw = asint(tmp1.xy);
                tmp2.xy = max(tmp1.zw, int2(0, 0));
                tmp2.zw = asint(_Resolution);
                tmp2.zw = tmp2.zw - int2(1, 1);
                tmp0.xy = min(tmp2.zw, tmp2.xy);
                tmp0 = _MainTex.Load(tmp0.xyz);
                tmp0.yz = tmp0.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp0.x = tmp0.x != 1.0;
                tmp0.yz = floor(tmp0.yz);
                tmp0.yz = asint(tmp0.yz);
                tmp3.xy = tmp1.zw + tmp0.yz;
                tmp0.yz = asint(inp.position.xy);
                tmp1.zw = tmp3.xy - tmp0.yz;
                tmp1.zw = tmp1.zw * tmp1.zw;
                tmp0.w = tmp1.w + tmp1.z;
                tmp0.w = max(-tmp0.w, tmp0.w);
                tmp0.w = floor(tmp0.w);
                tmp1.z = tmp0.w <= 9999.0;
                tmp0.x = tmp0.x ? tmp1.z : 0.0;
                tmp3.z = tmp0.x ? tmp0.w : 9999.0;
                tmp4.zw = float2(0.0, 0.0);
                tmp0.xw = tmp1.xy + _Distance.xx;
                tmp0.xw = asint(tmp0.xw);
                tmp1.zw = max(tmp0.xw, int2(0, 0));
                tmp4.xy = min(tmp2.zw, tmp1.zw);
                tmp4 = _MainTex.Load(tmp4.xyz);
                tmp1.zw = tmp4.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp2.x = tmp4.x != 1.0;
                tmp1.zw = floor(tmp1.zw);
                tmp1.zw = asint(tmp1.zw);
                tmp4.xy = tmp0.xw + tmp1.zw;
                tmp0.xw = tmp4.xy - tmp0.yz;
                tmp0.xw = tmp0.xw * tmp0.xw;
                tmp0.x = tmp0.w + tmp0.x;
                tmp0.x = max(-tmp0.x, tmp0.x);
                tmp4.z = floor(tmp0.x);
                tmp0.x = tmp3.z >= tmp4.z;
                tmp0.x = tmp2.x ? tmp0.x : 0.0;
                tmp3.xyz = tmp0.xxx ? tmp4.xyz : tmp3.xyz;
                tmp4.zw = float2(0.0, 0.0);
                tmp5 = _Distance.xxxx * float4(1.0, 0.0, 1.0, -1.0) + tmp1.xyxy;
                tmp5 = asint(tmp5);
                tmp6 = max(tmp5, int4(0, 0, 0, 0));
                tmp6 = min(tmp2.zwzw, tmp6.zwxy);
                tmp4.xy = tmp6.zw;
                tmp4 = _MainTex.Load(tmp4.xyz);
                tmp0.xw = tmp4.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp1.z = tmp4.x != 1.0;
                tmp0.xw = floor(tmp0.xw);
                tmp0.xw = asint(tmp0.xw);
                tmp4.xy = tmp5.xy + tmp0.xw;
                tmp0.xw = tmp4.xy - tmp0.yz;
                tmp0.xw = tmp0.xw * tmp0.xw;
                tmp0.x = tmp0.w + tmp0.x;
                tmp0.x = max(-tmp0.x, tmp0.x);
                tmp4.z = floor(tmp0.x);
                tmp0.x = tmp3.z >= tmp4.z;
                tmp0.x = tmp1.z ? tmp0.x : 0.0;
                tmp3.xyz = tmp0.xxx ? tmp4.xyz : tmp3.xyz;
                tmp6.zw = float2(0.0, 0.0);
                tmp4 = _MainTex.Load(tmp6.xyz);
                tmp0.xw = tmp4.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp1.z = tmp4.x != 1.0;
                tmp0.xw = floor(tmp0.xw);
                tmp0.xw = asint(tmp0.xw);
                tmp4.xy = tmp5.zw + tmp0.xw;
                tmp0.xw = tmp4.xy - tmp0.yz;
                tmp0.xw = tmp0.xw * tmp0.xw;
                tmp0.x = tmp0.w + tmp0.x;
                tmp0.x = max(-tmp0.x, tmp0.x);
                tmp4.z = floor(tmp0.x);
                tmp0.x = tmp3.z >= tmp4.z;
                tmp0.x = tmp1.z ? tmp0.x : 0.0;
                tmp3.xyz = tmp0.xxx ? tmp4.xyz : tmp3.xyz;
                tmp4.zw = float2(0.0, 0.0);
                tmp5 = _Distance.xxxx * float4(0.0, 1.0, -1.0, 1.0) + tmp1.xyxy;
                tmp5 = asint(tmp5);
                tmp6 = max(tmp5, int4(0, 0, 0, 0));
                tmp6 = min(tmp2.zwzw, tmp6.zwxy);
                tmp4.xy = tmp6.zw;
                tmp4 = _MainTex.Load(tmp4.xyz);
                tmp0.xw = tmp4.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp1.z = tmp4.x != 1.0;
                tmp0.xw = floor(tmp0.xw);
                tmp0.xw = asint(tmp0.xw);
                tmp4.xy = tmp5.xy + tmp0.xw;
                tmp0.xw = tmp4.xy - tmp0.yz;
                tmp0.xw = tmp0.xw * tmp0.xw;
                tmp0.x = tmp0.w + tmp0.x;
                tmp0.x = max(-tmp0.x, tmp0.x);
                tmp4.z = floor(tmp0.x);
                tmp0.x = tmp3.z >= tmp4.z;
                tmp0.x = tmp1.z ? tmp0.x : 0.0;
                tmp3.xyz = tmp0.xxx ? tmp4.xyz : tmp3.xyz;
                tmp4 = _Distance.xxxx * float4(0.0, -1.0, -1.0, 0.0) + tmp1.xyxy;
                tmp0.xw = _Distance.xx * float2(-1.0, -1.0) + tmp1.xy;
                tmp0.xw = asint(tmp0.xw);
                tmp1 = asint(tmp4);
                tmp4 = max(tmp1, int4(0, 0, 0, 0));
                tmp4 = min(tmp2.zwzw, tmp4.zwxy);
                tmp7.xy = tmp4.zw;
                tmp7.zw = float2(0.0, 0.0);
                tmp7 = _MainTex.Load(tmp7.xyz);
                tmp2.xy = tmp7.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp3.w = tmp7.x != 1.0;
                tmp2.xy = floor(tmp2.xy);
                tmp2.xy = asint(tmp2.xy);
                tmp7.xy = tmp1.xy + tmp2.xy;
                tmp1.xy = tmp7.xy - tmp0.yz;
                tmp1.xy = tmp1.xy * tmp1.xy;
                tmp1.x = tmp1.y + tmp1.x;
                tmp1.x = max(-tmp1.x, tmp1.x);
                tmp7.z = floor(tmp1.x);
                tmp1.x = tmp3.z >= tmp7.z;
                tmp1.x = tmp3.w ? tmp1.x : 0.0;
                tmp3.xyz = tmp1.xxx ? tmp7.xyz : tmp3.xyz;
                tmp6.zw = float2(0.0, 0.0);
                tmp6 = _MainTex.Load(tmp6.xyz);
                tmp1.xy = tmp6.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp2.x = tmp6.x != 1.0;
                tmp1.xy = floor(tmp1.xy);
                tmp1.xy = asint(tmp1.xy);
                tmp5.xy = tmp5.zw + tmp1.xy;
                tmp1.xy = tmp5.xy - tmp0.yz;
                tmp1.xy = tmp1.xy * tmp1.xy;
                tmp1.x = tmp1.y + tmp1.x;
                tmp1.x = max(-tmp1.x, tmp1.x);
                tmp5.z = floor(tmp1.x);
                tmp1.x = tmp3.z >= tmp5.z;
                tmp1.x = tmp2.x ? tmp1.x : 0.0;
                tmp3.xyz = tmp1.xxx ? tmp5.xyz : tmp3.xyz;
                tmp4.zw = float2(0.0, 0.0);
                tmp4 = _MainTex.Load(tmp4.xyz);
                tmp1.xy = tmp4.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp2.x = tmp4.x != 1.0;
                tmp1.xy = floor(tmp1.xy);
                tmp1.xy = asint(tmp1.xy);
                tmp1.xy = tmp1.zw + tmp1.xy;
                tmp4.xy = tmp1.xy - tmp0.yz;
                tmp4.xy = tmp4.xy * tmp4.xy;
                tmp1.w = tmp4.y + tmp4.x;
                tmp1.w = max(-tmp1.w, tmp1.w);
                tmp1.z = floor(tmp1.w);
                tmp1.w = tmp3.z >= tmp1.z;
                tmp1.w = tmp2.x ? tmp1.w : 0.0;
                tmp1.xyz = tmp1.www ? tmp1.xyz : tmp3.xyz;
                tmp2.xy = max(tmp0.xw, int2(0, 0));
                tmp2.xy = min(tmp2.zw, tmp2.xy);
                tmp2.zw = float2(0.0, 0.0);
                tmp2 = _MainTex.Load(tmp2.xyz);
                tmp2.yz = tmp2.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp1.w = tmp2.x != 1.0;
                tmp2.xy = floor(tmp2.yz);
                tmp2.xy = asint(tmp2.xy);
                tmp2.xy = tmp0.xw + tmp2.xy;
                tmp0.xw = tmp2.xy - tmp0.yz;
                tmp0.xw = tmp0.xw * tmp0.xw;
                tmp0.x = tmp0.w + tmp0.x;
                tmp0.x = max(-tmp0.x, tmp0.x);
                tmp2.z = floor(tmp0.x);
                tmp0.x = tmp1.z >= tmp2.z;
                tmp0.x = tmp1.w ? tmp0.x : 0.0;
                tmp1.xyz = tmp0.xxx ? tmp2.xyz : tmp1.xyz;
                tmp0.xy = tmp1.xy - tmp0.yz;
                tmp0.z = tmp1.z >= 9000.0;
                tmp0.xy = floor(tmp0.xy);
                tmp0.xy = tmp0.xy + float2(16.0, 16.0);
                tmp0.xy = tmp0.xy * float2(0.0322581, 0.0322581);
                o.sv_target.xy = tmp0.zz ? float2(1.0, 1.0) : tmp0.xy;
                return o;
			}
			ENDCG
		}
		Pass {
			LOD 100
			Tags { "RenderType" = "Opaque" }
			GpuProgramID 191360
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float2 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float _Distance;
			float2 _ResolutionDiff;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			Texture2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
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
                tmp0.xy = asint(inp.position.xy);
                tmp0.zw = float2(0.0, 0.0);
                tmp0 = _MainTex.Load(tmp0.xyz);
                tmp0.xy = tmp0.xy * float2(31.0, 31.0) + float2(-15.5, -15.5);
                tmp0.xy = floor(tmp0.xy);
                tmp0.zw = asint(tmp0.xy);
                tmp0.zw = tmp0.zw == int2(0, 0);
                tmp0.z = tmp0.w ? tmp0.z : 0.0;
                tmp0.w = tmp0.x >= 3000.0;
                tmp0.x = dot(tmp0.xy, tmp0.xy);
                tmp0.x = sqrt(tmp0.x);
                tmp0.y = uint1(tmp0.z) | uint1(tmp0.w);
                tmp0.z = max(_ResolutionDiff.y, _ResolutionDiff.x);
                tmp0.x = saturate(-_Distance * tmp0.z + tmp0.x);
                tmp0.x = floor(tmp0.x);
                o.sv_target.xy = tmp0.yy ? float2(1.0, 1.0) : tmp0.xx;
                return o;
			}
			ENDCG
		}
	}
}