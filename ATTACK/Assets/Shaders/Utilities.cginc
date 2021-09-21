/*
* Inverse lerp
* @param a Start value
* @param b End value
* @param v Value
* @returns 0 if v = a, 1 if v = b, the proportional distance from a to v given distance a to b.
*/
float InverseLerp(float a, float b, float v) {
    return (v - a) / (b - a);
}

float Remap(float Value, float aIn, float bIn, float aOut, float bOut) {
    float t = InverseLerp(aIn, bIn, Value);
    return aOut * (1 - t) + bOut * t;
}

float EdgeTransparency(float LowerBound, float UpperBound, float value) {
    float StartEdgeTransparency = saturate(InverseLerp(0, LowerBound, value));
    float EndEdgeTransparency = 1 - saturate(InverseLerp(UpperBound, 1, value));
    return StartEdgeTransparency * EndEdgeTransparency;
}