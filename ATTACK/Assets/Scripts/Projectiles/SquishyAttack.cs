using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishyAttack : Attack
{
    GameObject Bubble;
    MaterialPropertyBlock Mpb;

    protected override void UpdateCharge(ref ParticleSystem Charge)
    {
        float RadiusStart = 0.16f;
        float RadiusPeak = 0.5f;
        float SmoothStart = 0.2f;
        float SmoothPeak = 0.5f;
        float t = Mathf.Sin(Mathf.PI * SimulationTime / FireStartTime);
        if (t < 0) 
            t = 0;
        float Radius = RadiusStart * (1 - t) + RadiusPeak * t;
        float Smoothness = SmoothStart * (1 - t) + SmoothPeak * t;
        Mpb.SetFloat("_SphereRadius1", Radius);
        Mpb.SetFloat("_SmoothRadius", Smoothness);
        GetComponent<Renderer>().SetPropertyBlock(Mpb);
    }
    protected override void InstantiateProjectile()
    {
        Mpb = new MaterialPropertyBlock();
    }

    protected override void StartProjectile()
    {
        GameObject BubblePrefab = Resources.Load<GameObject>("Projectiles/Bubble/BubblePrefab");
        Bubble = Instantiate(BubblePrefab);
        Bubble.transform.position = AttackSource.transform.position;
        Bubble.SetActive(true);
    }

    protected override void StopProjectile()
    {
        Bubble.GetComponent<MeshRenderer>().enabled = false;
        Destroy(Bubble, 1.0f);
    }

    protected override void UpdateProjectile()
    {
        if (Bubble.activeSelf)
        {
            float t = (SimulationTime - FireStartTime) / MaxFireTime;
            if (t < 0)
                t = 0;
            Vector3 Position = (1 - t) * AttackSource.transform.position + t * TargetPosition;
            float s = Mathf.Sin(t * 60 * Mathf.PI);
            float c = Mathf.Cos(t * 60 * Mathf.PI);
            Vector3 Swirl = 0.1f * new Vector3(s, c, s);
            Bubble.transform.position = Position + Swirl;
        }
    }

    private void OnDisable()
    {
        if (Bubble != null)
            Bubble.SetActive(false);
    }
}
