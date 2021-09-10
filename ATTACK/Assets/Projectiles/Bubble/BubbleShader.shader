Shader "Bubble"
{
    //http://forum.unity3d.com/threads/179374-Soap-bubble-shader
    Properties
    {
        _Color("_Color", Color) = (0.0,1.0,0.0,1.0)
        _Inside("_Inside", Range(0.0,2.0)) = 0.0
        _Rim("_Rim", Range(0.0,2.0)) = 1.2
        _Texture("_Texture", 2D) = "white" {}
        _Speed("_Speed", Range(0.5,5.0)) = 0.5
        _Tile("_Tile", Range(1.0,10.0)) = 5.0
        _Strength("_Strength", Range(0.0,5.0)) = 1.5
        _Albedo("Albedo", Range(0.1, 1.0)) = 1.0
        _Glow("Glow", Range(1, 100)) = 1
        _Cube("Cubemap", Cube) = "" {}
    }
        SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        Cull Back
        ZWrite On
        ZTest LEqual

        CGPROGRAM
        #pragma surface surf BlinnPhong alpha
        #pragma target 3.0

        fixed4 _Color;
        fixed4 _ReflectColor;
        fixed _Inside;
        fixed _Rim;
        fixed _Speed;
        fixed _Tile;
        fixed _Strength;
        fixed _Albedo;
        half _Glow;
        half _Shininess;
        sampler2D _CameraDepthTexture;
        sampler2D _Texture;
        samplerCUBE _Cube;

        struct Input
        {
            float4 screenPos;
            float3 viewDir;
            float2 uv_Texture;
            float3 worldRefl;
            INTERNAL_DATA
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = fixed3(0.0,0.0,0.0);
            o.Normal = fixed3(0.0,0.0,1.0);
            o.Emission = 0.0;
            o.Gloss = 0.0;
            o.Specular = 0.0;
            o.Alpha = 1.0;
            float4 ScreenDepthDiff0 = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)).r) - IN.screenPos.z;
            float4 Saturate0 = 0.3 * fixed4(1.0,1.0,1.0,1.0);
            float4 Fresnel0_1_NoInput = fixed4(0,0,1.0,1.0);
            float f = 1.0 - dot(normalize(IN.viewDir), normalize(Fresnel0_1_NoInput.xyz));
            float4 Fresnel0 = float4(f,f,f,f);
            float4 Step0 = step(Fresnel0,float4(1.0,1.0,1.0,1.0));
            float4 Clamp0 = clamp(Step0,_Inside.xxxx,float4(1.0,1.0,1.0,1.0));
            float4 Pow0 = pow(Fresnel0,_Rim.xxxx);
            float4 Multiply5 = _Time * _Speed.xxxx;
            float4 UV_Pan0 = float4((IN.uv_Texture.xyxy).x,(IN.uv_Texture.xyxy).y + Multiply5.x,(IN.uv_Texture.xyxy).z,(IN.uv_Texture.xyxy).w);
            float4 Multiply1 = UV_Pan0 * _Tile.xxxx;
            float4 Tex2D0 = tex2D(_Texture,Multiply1.xy);
            float4 Multiply2 = Tex2D0 * _Strength.xxxx;
            float4 Multiply0 = Pow0 * Multiply2;
            float4 Multiply3 = Clamp0 * Multiply0;
            float4 Multiply4 = Saturate0 * Multiply3;

            o.Alpha = Multiply3.w * _Color.a;
            o.Emission = Multiply4.xyz * _Color.rgb * texCUBE(_Cube, IN.worldRefl).xyz * _Glow;
            o.Albedo = _Albedo;

        }
        ENDCG

    }
        //Fallback "Diffuse"
}