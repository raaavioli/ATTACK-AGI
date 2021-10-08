#if !defined(FLOW_INCLUDED)
#define FLOW_INCLUDED

float3 FlowUVW(float2 uv, float2 flowVector, float2 jump, float flowOffset, float tiling, float time, bool flowB) {
	float phaseOffset = flowB ? 0.5 : 0;
	float progress = frac(time + phaseOffset);
	float3 uvw;
	uvw.xy = uv - flowVector * (progress + flowOffset);
	uvw.xy *= tiling;
	uvw.xy += phaseOffset;
	uvw.xy += (time - progress) * jump;
	uvw.z = 1 - abs(1 - 2 * progress);
	return uvw;
}

float2 DirectionalFlowUVW(float2 uv, float2 flowVector, float tiling, float time) {
	float2 dir = normalize(flowVector.xy);
	uv = mul(float2x2(dir.y, -dir.x, dir.x, dir.y), uv);
	uv.y -= time;
	return uv * tiling;
}

#endif