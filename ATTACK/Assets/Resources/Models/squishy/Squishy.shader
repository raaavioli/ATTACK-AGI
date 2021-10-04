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
        _Scale("Scale", Range(0, 1)) = 1
        _OuterColor("Outer color", Color) = (0.7, 0.2, 0.5, 1.0)
        _InnerColor("Inner color", Color) = (0.3, 0, 0.1, 0.2)
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
            float _Scale;
            float4 _OuterColor;
            float4 _InnerColor;

            float sdSphere(float3 p, float r) {
                return length(p) - r;
            }

            float sdEllipsoid(float3 p, float3 r) 
            {
                float k1 = length(p / r);
                float k2 = length(p / (r * r));
                return k1 * (k1 - 1.0) / k2;
            }

            float sdSolidAngle(float3 p, float angle, float radius) {
                float2 c = float2(sin(angle), cos(angle / 3));
                float2 q = float2(length(p.xz), -p.y);
                float l = length(q) - radius;
                float m = length(q - c * clamp(dot(q, c), 0.0, radius));
                return max(l, m * sign(c.y * q.x - c.x * q.y));
            }

            float sdRoundCone(float3 p, float r1, float r2, float h)
            {
                float2 q = float2(length(p.xz), p.y);

                float b = (r1 - r2) / h;
                float a = sqrt(1.0 - b * b);
                float k = dot(q, float2(-b, a));

                if (k < 0.0) return length(q) - r1;
                if (k > a * h) return length(q - float2(0.0, h)) - r2;

                return dot(q, float2(a, b)) - r1;
            }

            float2 sdEye(float3 p, int dir) {
                float eyewhites = sdSolidAngle(p, PI / 3, 0.1);
                float eyeballs = sdSphere(p + float3(0, 0.06, 0), 0.055);

                float3 Eyelid = p + float3(dir * 0.03, 0.08, -0.14);
                Eyelid.z = Eyelid.z + 5 * Eyelid.x * Eyelid.x;
                Eyelid.xz = mul(float2x2(12, dir * 5, dir * -5, 12) / 13.0, Eyelid.xz);
                float eyelids = sdEllipsoid(Eyelid, float3(0.07, 0.02, 0.02));

                float material = 1.0;
                if (eyewhites < eyelids)
                    material = 2.0;
                if (eyeballs < eyewhites)
                    material = 3.0;
                
                float eyes = smin(eyeballs, eyewhites, 0.001);
                eyes = min(eyes, eyelids);
                return float2(eyes, material);
            }


            float2 sdSquishy(float3 p) {
                p = (p / _Scale);
                float time = _Time.z * PI * 2;
                float3 _SpherePos1 = 0.1 * float3(0, sin(time / 5), 0);
                float3 _SpherePos2 = 0.35 * float3(sin(time / 7), 0.3 + 0.5 * sin(time / 5), sin(time / 3));
                float3 _SpherePos3 = 0.35 * float3(cos(time / 3), 0.3 + 0.5 * sin(time / 5), cos(time / 5));

                // Body
                float3 BodyPos = p + float3(0, 0.2, 0);
                float s1 = length(BodyPos - _SpherePos1) - (_SphereRadius1 + 0.05 * sin(time / 2));
                float s2 = length(BodyPos - _SpherePos2) - _SphereRadius2;
                float s3 = length(BodyPos - _SpherePos3) - _SphereRadius3;
                float Cone = sdRoundCone(BodyPos, 0.05, 0.05, _OctaSize + 0.05 * sin(time / 12));
               
                float body = smin(smin(s1, s2, _SmoothRadius), s3, _SmoothRadius);
                body = smin(body, Cone, _SmoothRadius);

                // Eyes
                float3 EyeRot = mul(rotateX(PI / 2), p);
                float3 Eye1Translation = float3(0.2 + 0.03 * cos(time / 10), 0.03 * cos(time / 10), -0.4 + 0.02 * sin(time / 10));
                float3 Eye2Translation = float3(-0.2 + 0.03 * cos(time / 9), 0.03 * sin(time / 9), -0.4 + 0.03 * sin(time / 9));

                float2 eye1 = sdEye(EyeRot + Eye1Translation, 1);
                float2 eye2 = sdEye(EyeRot + Eye2Translation, -1);
                float eyes = smin(eye1.x, eye2.x, _SmoothRadius);

                
                // Default body material
                float material = 1.0; 
                if (eye1.x < body)
                    material = eye1.y;
                if (eye2.x < body && eye2.x < eye1.x)
                    material = eye2.y;

                float squishy = min(body, eyes);

                float floor = dot(p, float3(0, 1, 0)) + 0.4;
                return float2(max(squishy, -floor) * _Scale, material);
            }

            float3 GetNormal(float3 Pos) {
                float2 e = float2(1e-2, 0);
                // Central difference
                float3 a = float3(
                    sdSquishy(Pos + e.xyy).x,
                    sdSquishy(Pos + e.yxy).x,
                    sdSquishy(Pos + e.yyx).x
                );
                float3 b = float3(
                    sdSquishy(Pos - e.xyy).x,
                    sdSquishy(Pos - e.yxy).x,
                    sdSquishy(Pos - e.yyx).x
                );

                return normalize(a - b);
            }

            #define MAX_DISTANCE 1000
            #define MIN_DISTANCE 1e-3
            #define MAX_STEPS 100
            float2 RayMarch(float3 rOrigin, float3 rDir) {
                float rStepSize = 0;
                for (int i = 0; i < MAX_STEPS; i++) {
                    float3 rPos = rOrigin + rDir * rStepSize;
                    float2 DistToScene = sdSquishy(rPos);
                    rStepSize += DistToScene.x;
                    if (rStepSize > MAX_DISTANCE) break;
                    if (DistToScene.x < MIN_DISTANCE) 
                    {
                        return float2(rStepSize, DistToScene.y);
                    }
                }

                return float2(rStepSize, 0.0);
            }

            fixed4 frag(FragmentInput i) : SV_Target
            {
                float2 uv = (i.uv * 2 - 1);
                float3 rDir = normalize(i.objPos - i.rOrigin);
                float2 March = RayMarch(i.rOrigin, rDir);
                float Dist = March.x;
                float Material = March.y;

                float3 WorldViewDir = normalize(mul(unity_ObjectToWorld, float4(rDir, 0)));

                fixed4 Color = fixed4(0, 0, 0, 0);
                if (Material > 0.0) {
                    float3 Intersection = i.rOrigin + rDir * Dist;
                    float3 WorldNormal = normalize(mul(unity_ObjectToWorld, float4(GetNormal(Intersection), 0)));
                    float Lambert = max(dot(WorldNormal, _WorldSpaceLightPos0), 0.0);
                    float Fresnel = 1 - dot(WorldNormal, -WorldViewDir);
                    if (Material < 1.5) // Body
                        Color = lerp(_InnerColor, _OuterColor, Fresnel);
                    else if (Material < 2.5) // Eyes
                        Color = lerp(float4(1.0, 1.0, 1.0, 0.8), _OuterColor, Fresnel);
                    else if (Material < 3.5) // Eye balls
                        Color = float4(0, 0, 0, 0.8);
                }

                return Color;
            }
            ENDCG
        }
    }
}
