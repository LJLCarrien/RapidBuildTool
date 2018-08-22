Shader "Unlit/CopyTwo"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		LOD 200


		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		// make fog work
		#pragma multi_compile_fog

		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float2 horUv : TEXCOORD1;//上下翻转
			float4 vertex : SV_POSITION;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			float newu = (v.uv.y - 1)*(-1);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			float2 newuv = float2(v.uv.x, newu);
			o.horUv = TRANSFORM_TEX(newuv, _MainTex);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{

			fixed4 colNor = tex2D(_MainTex, i.uv);
			fixed4 colFan = tex2D(_MainTex, i.horUv);
			fixed4 col;
			if (i.uv.y < 1) {
				col = colFan;
			}
			else {
				col = colNor;
			}
			return col;
		}
		ENDCG
	}
	}
}
