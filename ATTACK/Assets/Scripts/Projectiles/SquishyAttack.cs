using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishyAttack : Attack
{
    public GameObject BubblePrefab;
    List<GameObject> Bubbles = new List<GameObject>();
    MaterialPropertyBlock Mpb;

    protected override int GetMaxTargets()
    {
        return 3;
    }
    protected override void UpdateCharge(ref ParticleSystem Charge)
    {
        float RadiusStart = 0.16f;
        float RadiusPeak = 0.5f;
        float SmoothStart = 0.2f;
        float SmoothPeak = 0.5f;
        float t = Mathf.Sin(Mathf.PI * SimulationTime / ChargeTime);
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
        for (int i = 0; i < TargetPositions.Count; i++)
        {
            GameObject bubble = Instantiate(BubblePrefab);
            bubble.transform.position = AttackSource.transform.position;
            bubble.SetActive(true);
            Bubbles.Add(bubble);
        }
    }

    protected override void StopProjectile()
    {
        foreach (GameObject bubble in Bubbles)
        {
            bubble.GetComponent<MeshRenderer>().enabled = false;
            Destroy(bubble, 1.0f);
        }
        Bubbles.Clear();
    }

    protected override void UpdateProjectile()
    {
        for (int i = 0; i < Bubbles.Count; i++)
        {
            GameObject bubble = Bubbles[i];
            if (bubble != null && bubble.activeSelf)
            {
                float t = (SimulationTime - ChargeTime) / MaxFireTime;
                if (t < 0)
                    t = 0;
                Vector3 Position = (1 - t) * AttackSource.transform.position + t * TargetPositions[i];
                float s = Mathf.Sin(t * 60 * Mathf.PI);
                float c = Mathf.Cos(t * 60 * Mathf.PI);
                Vector3 Swirl = 0.1f * new Vector3(s, c, s);
                bubble.transform.position = Position + Swirl;
            }
        }
    }

    private void OnDisable()
    {
        foreach (GameObject bubble in Bubbles)
        {
            if (bubble != null)
                bubble.SetActive(false);
        }
    }
}
