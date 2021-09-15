Shader "Unlit/CompleteLaserShader"
{
    Properties
    {
        _Repeat("Repeat", Range(1, 20)) = 1

        _CoreColor("Core color", Color) = (1, 1, 1, 1)
        _CoreSpeed("Speed", Range(0.0, 30.0)) = 1.0
        _CoreIntensity("Intensity", Range(0.0, 5.0)) = 1

        [NoScaleOffset] _SwirlMask("Swirl mask", 2D) = "black" {}

        _OuterSwirlColor("Outer swirl color", Color) = (1, 1, 1, 1)
        _OuterSwirlSpeed("Outer swirl speed", Range(0.0, 10.0)) = 1.0
        _OuterSwirlIntensity("Outer swirl intensity", Range(1.0, 10.0)) = 1.0
        _OuterSwirlTiling("Inner: Tile-X, Tile-Y, Offset-X, Offset-Y", Vector) = (1, 1, 0, 0)
        _OuterRadius("Outer radius", Range(0.0, 5.0)) = 1.0

        _InnerSwirlColor("Inner swirl color", Color) = (1, 1, 1, 1)
        _InnerSwirlSpeed("Inner swirl speed", Range(0.0, 10.0)) = 1.0
        _InnerSwirlIntensity("Inner swirl intensity", Range(1.0, 10.0)) = 1.0
        _InnerSwirlTiling("Inner: Tile-X, Tile-Y, Offset-X, Offset-Y", Vector) = (1, 1, 0, 0)
        _InnerRadius("Inner radius", Range(0.0, 5.0)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/Utilities.cginc" 

            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 unwrapped_uv : TEXCOORD1;
                float3 model_normal : TEXCOORD2;
            };

            half4 _CoreColor;
            float _CoreSpeed;
            int _Repeat;
            float _CoreIntensity;

            #define PI 3.1415926535
            #define TAU 6.2831853071795

            FragmentInput vert(VertexInput v)
            {
                FragmentInput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = float2(v.uv.x, _Repeat * v.uv.y);
                o.unwrapped_uv = v.uv;
                o.model_normal = v.normal;
                return o;
            }

            fixed4 frag(FragmentInput i) : SV_Target{
                float Distortion = cos(i.uv.x * 30 + i.uv.y * 5 * TAU) * 0.5;

                float WavePatternY = sin(_Time.z * 3) * _CoreIntensity * sin(-_Time.z * _CoreSpeed + i.uv.y * 10 * TAU) * 0.5 + 0.5;
                WavePatternY += sin(-_Time.z * _CoreSpeed + i.uv.y * 13 * TAU);
                WavePatternY += Distortion;
                //return WavePatternY;
                float CylinderCapCull = (abs(i.model_normal.y) < 0.99);
                return _CoreColor * WavePatternY * CylinderCapCull * EdgeTransparency(0.1, 0.9, i.unwrapped_uv.y);
            }
            ENDCG
        }

        Pass
        {
            ZWrite Off
            Cull Off
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define M_TILE_OFFSET _OuterSwirlTiling
            #define M_COLOR _OuterSwirlColor
            #define M_SPEED _OuterSwirlSpeed
            #define M_INTENSITY _OuterSwirlIntensity
            #define M_XZ_SCALE _OuterRadius

            #include "Assets/Shaders/SwirlLaser.cginc" 

            ENDCG
        }

        Pass
        {
            ZWrite Off
            Cull Off
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define M_TILE_OFFSET _InnerSwirlTiling
            #define M_COLOR _InnerSwirlColor
            #define M_SPEED _InnerSwirlSpeed
            #define M_INTENSITY _InnerSwirlIntensity
            #define M_XZ_SCALE _InnerRadius


            #include "Assets/Shaders/SwirlLaser.cginc" 

            ENDCG
        }
    }
}
