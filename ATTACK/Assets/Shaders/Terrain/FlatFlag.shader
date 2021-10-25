Shader "Unlit/FlatFlag"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PolyScale("PolyScale", Range(1, 20)) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct FragInput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PolyScale;

            FragInput vert (VertexInput v)
            {
                FragInput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex).xyz;
                return o;
            }

            fixed4 frag(FragInput i) : SV_Target
            {
                float3 modelForward = mul(UNITY_MATRIX_M, float4(0, 1, 0, 0)).xyz;
                float3 dFdxPos = ddx(i.worldPos);
                float3 dFdyPos = ddy(i.worldPos);
                float3 Normal = normalize(cross(dFdyPos, dFdxPos));
                float PolyFactor = saturate(pow(abs(1 - dot(Normal, normalize(modelForward))), _PolyScale));
                float lambert = saturate(dot(Normal, normalize(_WorldSpaceLightPos0)));
                fixed4 Color = tex2D(_MainTex, i.uv);
                float Ambient = abs(PolyFactor) / 2.0;
                return Color * (Ambient + lambert);
            }
            ENDCG
        }
    }
}
