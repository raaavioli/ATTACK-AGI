using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AttackUtils
{
    public static float Saturate(float a)
    {
        float ret;
        if (a < 0)
            ret = 0;
        else if (a > 1)
            ret = 1;
        else 
            ret = a;
        return ret;
    }
    public static float InverseLerp(float a, float b, float v)
    {
        return (v - a) / (b - a);
    }

    public static float Remap(float aIn, float bIn, float aOut, float bOut, float Value)
    {
        float t = Saturate(InverseLerp(aIn, bIn, Value));
        return aOut * (1 - t) + bOut * t;
    }
}
