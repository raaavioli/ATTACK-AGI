// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _SphereRadius1("Sphere radius 1", Range(0, 1)) = 0.5
        _SphereRadius2("Sphere radius 2", Range(0, 1)) = 0.5
        _SphereRadius3("Sphere radius 3", Range(0, 1)) = 0.5
        _OctaSize("Octa size", Range(0, 1)) = 0.5
        _SmoothRadius("Smooth radius", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            blend SrcAlpha OneMinusSrcAlpha    
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 rOrigin : TEXCOORD1;
                float3 objPos : TEXCOORD2;
            };

            #define PI 3.14159265359


            FragmentInput vert (VertexInput v)
            {

                FragmentInput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.rOrigin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
                o.objPos = v.vertex;
                return o;
            }

            fixed3x3 rotateX(float theta) {
                fixed c = cos(theta);
                fixed s = sin(theta);

                return float3x3(
                    fixed3(1, 0, 0),
                    fixed3(0, c, -s),
                    fixed3(0, s, c)
                );
            }

            float smin(float a, float b, float k)
            {
                float h = max(k - abs(a - b), 0.0);
                return min(a, b) - h * h * (1.0 / (k * 4.0));
            }

            float _SphereRadius1;
            float _SphereRadius2;
            float _SphereRadius3;
            float _OctaSize;
            float _SmoothRadius;

            float SolidAngle(float3 p, float angle, float radius) {
                float2 c = float2(sin(angle), cos(angle / 3));
                float2 q = float2(length(p.xz), p.y);
                float l = length(q) - radius;
                float m = length(q - c * clamp(dot(q, c), 0.0, radius));
                return max(l, m * sign(c.y * q.x - c.x * q.y));
            }

            float RoundCone(float3 p, float r1, float r2, float h)
            {
                float2 q = float2(length(p.xz), p.y);

                float b = (r1 - r2) / h;
                float a = sqrt(1.0 - b * b);
                float k = dot(q, float2(-b, a));

                if (k < 0.0) return length(q) - r1;
                if (k > a * h) return length(q - float2(0.0, h)) - r2;

                return dot(q, float2(a, b)) - r1;
            }

            float GetClosestDist(float3 p) {
                // Move this to script code //
                float time = _Time.z * PI * 2;
                float3 _SpherePos1 = 0.1 * float3(0, sin(time / 5), 0);
                float3 _SpherePos2 = 0.35 * float3(sin(time / 7), 0.3 + 0.5 * sin(time / 5), sin(time / 3));
                float3 _SpherePos3 = 0.35 * float3(cos(time / 3), 0.3 + 0.5 * sin(time / 5), cos(time / 5));

                float3 BodyPos = p + float3(0, 0.2, 0);
                float s1 = length(BodyPos - _SpherePos1) - (_SphereRadius1 + 0.05 * sin(time / 2));
                float s2 = length(BodyPos - _SpherePos2) - _SphereRadius2;
                float s3 = length(BodyPos - _SpherePos3) - _SphereRadius3;
                float Cone = RoundCone(BodyPos, 0.05, 0.05, _OctaSize + 0.05 * sin(time / 12));
               
                float body = smin(smin(s1, s2, _SmoothRadius), s3, _SmoothRadius);
                body = smin(body, Cone, _SmoothRadius);

                float3 EyeRot = mul(rotateX(PI / 2), p);
                float3 Eye1 = EyeRot + float3(0.2 + 0.02 * cos(time / 10), 0, -0.4 + 0.02 * sin(time / 10));
                float3 Eye2 = EyeRot + float3(-0.2 + 0.02 * cos(time / 9), 0, -0.4 + 0.03 * sin(time / 9));
                
                float head = smin(SolidAngle(Eye1, PI / 3, 0.1), SolidAngle(Eye2, PI / 3, 0.1), _SmoothRadius);
                float squishy = smin(body, head, _SmoothRadius);
                float floor = dot(p, float3(0, 1, 0)) + 0.4;
                return max(squishy, -floor);
            }

            float3 GetNormal(float3 Pos) {
                float2 e = float2(1e-2, 0);
                // Central difference
                float3 a = float3(
                    GetClosestDist(Pos + e.xyy),
                    GetClosestDist(Pos + e.yxy),
                    GetClosestDist(Pos + e.yyx)
                );
                float3 b = float3(
                    GetClosestDist(Pos - e.xyy),
                    GetClosestDist(Pos - e.yxy),
                    GetClosestDist(Pos - e.yyx)
                );

                return normalize(a - b);
            }

            #define MAX_DISTANCE 10000
            #define MIN_DISTANCE 1e-3
            #define MAX_STEPS 100
            float RayMarch(float3 rOrigin, float3 rDir) {
                float rStepSize = 0;
                for (int i = 0; i < MAX_STEPS; i++) {
                    float3 rPos = rOrigin + rDir * rStepSize;
                    float DistToScene = GetClosestDist(rPos);
                    rStepSize += DistToScene;
                    if (rStepSize > MAX_DISTANCE || DistToScene < MIN_DISTANCE) break;
                }

                return rStepSize;
            }

            fixed4 frag(FragmentInput i) : SV_Target
            {
                float2 uv = (i.uv * 2 - 1);
                float3 rDir = normalize(i.objPos - i.rOrigin);
                float Dist = RayMarch(i.rOrigin, rDir);

                float3 WorldViewDir = normalize(mul(unity_ObjectToWorld, float4(rDir, 0)));

                fixed4 Color = fixed4(0, 0, 0, 0);
                if (Dist < MAX_DISTANCE) {
                    float3 Intersection = i.rOrigin + rDir * Dist;
                    float3 WorldNormal = normalize(mul(unity_ObjectToWorld, float4(GetNormal(Intersection), 0)));
                    float Lambert = max(dot(WorldNormal, _WorldSpaceLightPos0), 0.0);
                    float Fresnel = 1 - dot(WorldNormal, -WorldViewDir);
                    float4 OuterColor = float4(0.7, 0.2, 0.5, 1.0);
                    float4 InnerColor = float4(0.3, 0, 0.1, 0.2);
                    Color = lerp(InnerColor, OuterColor, Fresnel);
                }

                return Color;
            }
            ENDCG
        }
    }
}
