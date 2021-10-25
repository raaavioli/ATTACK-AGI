Shader "Unlit/StatRank"
{
    Properties
    {
        _RankIcon ("Texture", 2D) = "white" {}
        [IntRange] _Rank ("Rank", Range(1, 5)) = 3
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _RankIcon;
            float4 _RankIcon_ST;
            int _Rank;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _RankIcon);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 stars = tex2D(_RankIcon, min(i.uv * fixed2(1, 1.7), fixed2(5, 1)));
                stars = fixed4(_Color.rgb, 1) * stars.a;
                fixed starMask = clamp((i.uv.x < _Rank), 0.1, 1);
                return stars * fixed4(starMask, starMask, starMask, 1);
            }
            ENDCG
        }
    }
}
