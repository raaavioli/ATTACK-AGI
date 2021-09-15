
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

sampler2D _SwirlMask;
float _Repeat;
float4 M_TILE_OFFSET;
half4 M_COLOR;
float M_SPEED;
float M_INTENSITY;
float M_XZ_SCALE;

#define PI 3.1415926535
#define TAU 6.2831853071795

FragmentInput vert(VertexInput v)
{
    FragmentInput o;
#ifdef M_XZ_SCALE
    float3x3 scale = {
        M_XZ_SCALE, 0, 0,
        0, 1, 0,
        0, 0, M_XZ_SCALE,
    };
    v.vertex = float4(mul(scale, v.vertex.xyz), v.vertex.w);
    o.vertex = UnityObjectToClipPos(v.vertex);
#else
    o.vertex = UnityObjectToClipPos(v.vertex);
#endif

#ifdef M_TILE_OFFSET
    o.uv = v.uv * M_TILE_OFFSET.xy + M_TILE_OFFSET.zw;
#else
    o.uv = v.uv;
#endif
    o.uv = float2(o.uv.x, _Repeat * o.uv.y);
    o.unwrapped_uv = v.uv;
    o.model_normal = v.normal;
    return o;
}

fixed4 frag(FragmentInput i) : SV_Target{
    fixed4 swirlMask = tex2D(_SwirlMask, float2(i.uv.x, M_SPEED * _Time.y + i.uv.y));
    fixed4 swirl = M_COLOR * swirlMask;
    float CylinderCapCull = (abs(i.model_normal.y) < 0.99);
    return M_INTENSITY * swirl * CylinderCapCull * EdgeTransparency(0.1, 0.9, i.unwrapped_uv.y);
}