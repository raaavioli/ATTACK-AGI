using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healing : Attack
{
    [SerializeField]
    private GameObject HealingPrefab;

    private GameObject HealingProjectile;

    protected override int GetMaxTargets()
    {
        return 2;
    }

    protected override void InstantiateProjectile()
    {
    }

    protected override void StartProjectile()
    {
        HealingProjectile = Instantiate(HealingPrefab);
        HealingProjectile.transform.position = TargetPositions[0];
        foreach (ParticleSystem ps in HealingProjectile.GetComponentsInChildren<ParticleSystem>())
        {
            ps.time = 0;
            ps.Play();
        }
    }

    protected override void StopProjectile()
    {
        Destroy(HealingProjectile, 2.0f);
    }

    protected override void UpdateProjectile()
    {
        float time = (SimulationTime - FireStartTime) / MaxFireTime;
        if (time < 0)
            time = 0;
        if (time > 1)
            time = 1;


        foreach (Transform t in HealingProjectile.GetComponentsInChildren<Transform>())
        {
            if (t == HealingProjectile.transform || t.gameObject.GetComponent<ParticleSystem>() != null)
                continue;
            // For non particle systems
            Vector3 scale = t.localScale;
            float amplitude = 3.0f;
            scale.y = amplitude * Mathf.Sin(time * Mathf.PI);
            t.localScale = scale;

            Vector3 position = t.localPosition;
            position.y = scale.y;
            t.localPosition = position;
        }
    }

    private void OnDisable()
    {
        if (HealingProjectile != null)
            HealingProjectile.SetActive(false);
    }
}
