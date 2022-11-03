Shader "Projector/Fog Of War" {
	Properties {
		_OldFogTex ("Old Fog Texture", 2D) = "gray" {}
		_FogTex ("Fog Texture", 2D) = "gray" {}
		_Color ("Color", Color) = (0,0,0,0)
	}
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off //어두운 안개는 모든 물체를 가릴 것이므로 z 버퍼를 관리할 필요가 없다. 따라서 Off 해버리도록 한다.
			
			// 유니티 공식 docs에서 Multiplicative를 하는 블렌드 타입이라 소개하고 있다. 
			//Multiplicative 셰이더는 RGB의 검은 영역을 하얗게 바꿔준다고 한다.
			Blend DstColor Zero 
			
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 uvShadow : TEXCOORD0;
				UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uvShadow = mul (unity_Projector, vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			sampler2D _OldFogTex;
			sampler2D _FogTex;
			fixed4 _Color;
			uniform float _Blend;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed a1 = tex2Dproj (_OldFogTex, UNITY_PROJ_COORD(i.uvShadow)).a;
				fixed a2 = tex2Dproj (_FogTex, UNITY_PROJ_COORD(i.uvShadow)).a;

				fixed a = lerp(a1, a2, _Blend);
				fixed4 col = lerp(_Color, fixed4(1,1,1,1), a);

				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1));
				return col;
			}
			ENDCG
		}
	}
}