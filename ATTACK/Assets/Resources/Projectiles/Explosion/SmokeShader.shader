Shader "Unlit/SmokeShader"
{
    Properties
    {
        _SmokeTex("Texture", 2D) = "white" {}
        _Intensity("Intensity", Range(1, 10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct FragmentInput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : TEXCOORD1;
            };

            sampler2D _SmokeTex;
            float4 _SmokeTex_ST;
            float _Brightness;
            float _Intensity;

            FragmentInput vert (VertexInput v)
            {
                FragmentInput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _SmokeTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(FragmentInput i) : SV_Target
            {
                fixed RadialOpacity = saturate(1 - length(i.uv * 2 - 1));
                fixed4 Smoke = tex2D(_SmokeTex, i.uv);
                fixed Color = saturate(Smoke.r + Smoke.g);
                fixed Alpha = pow(Smoke.b, 1);
                fixed t = i.uv.y;
                fixed Intensity = _Intensity * i.color.a;
                fixed3 OutColor = t * fixed3(0, 0, 0) + (1 - t) * Intensity * fixed3(Color, Color, Color);
                OutColor *= i.color.rgb;
                return fixed4(OutColor, i.color.a * Alpha);
            }
            ENDCG
        }
    }
}
