Shader "psx/vertexlit/vertexlit" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_VertexWarpScale ("Vertex Warping Scalar", Range(0, 10)) = 1
		[Toggle] _Outline ("Assist Outline", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="Vertex" }
		LOD 200


		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag 
			#include "UnityCG.cginc"

			sampler2D _MainTex; float4 _MainTex_ST; float4 _MainTex_TexelSize;
			fixed4 _Color;

			struct appdata 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
			    float4 vertex : SV_POSITION;
				float4 color : COLOR0;
				float2 uv_MainTex : TEXCOORD0;
			};

			float3 HandleLighting (float4 vertex, float3 normal)
			{
				float3 viewpos = UnityObjectToViewPos (vertex.xyz);
				float3 viewN = normalize (mul ((float3x3)UNITY_MATRIX_IT_MV, normal));

				float3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
				for (int i = 0; i < 8; i++) 
				{
					float3 toLight = unity_LightPosition[i].xyz - viewpos.xyz * unity_LightPosition[i].w;
					float lengthSq = dot(toLight, toLight);

					// don't produce NaNs if some vertex position overlaps with the light
					lengthSq = max(lengthSq, 0.000001);

					toLight *= rsqrt(lengthSq);

					float atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[i].z);

					float rho = max (0, dot(toLight, unity_SpotDirection[i].xyz));
					float spotAtt = (rho - unity_LightAtten[i].x) * unity_LightAtten[i].y;
					atten *= saturate(spotAtt);
				

					float diff = max (0, dot (viewN, toLight));
					lightColor += unity_LightColor[i].rgb * (diff * atten);
				}
				return lightColor;
			}

			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);

				o.color = v.color;
				o.color.rgb *= _Color.rgb;

				o.color.rgb *= HandleLighting(v.vertex, v.normal);


				return o;
			}

			float4 frag(v2f IN) : COLOR
			{
				float4 color = tex2D(_MainTex, IN.uv_MainTex);
				color.rgb *= IN.color;
				color.a = 1.0;
				return color;
			}
		
			ENDCG
		}
	}
}