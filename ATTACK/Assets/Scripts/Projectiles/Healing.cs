using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healing : Attack
{
    [SerializeField]
    private GameObject HealingPrefab;

    private List<GameObject> HealingProjectiles = new List<GameObject>();

    protected override int GetMaxTargets()
    {
        return 2;
    }

    protected override void InstantiateProjectile()
    {
    }

    protected override void StartProjectile()
    {
        for (int i = 0; i < Targets.Count; i++)
        {
            if (Targets[i] == null)
                continue;
            GameObject HealingProjectile = Instantiate(HealingPrefab); 
            HealingProjectiles.Add(HealingProjectile);
            string parentName = Targets[i].transform.parent.name;
            GameObject SpawnObject = GameObject.Find(parentName + "/RockPlatform");
            HealingProjectile.transform.position = SpawnObject.transform.position + new Vector3(0, 1.5f, 0);
            foreach (ParticleSystem ps in HealingProjectile.GetComponentsInChildren<ParticleSystem>())
            {
                ps.time = 0;
                ps.Play();
            }
        }
    }

    protected override void StopProjectile()
    {
        foreach (GameObject projectile in HealingProjectiles)
        {
            Destroy(projectile, 2.0f);
        }
        HealingProjectiles.Clear();
    }

    protected override void UpdateProjectile()
    {
        float time = (SimulationTime - ChargeTime) / MaxFireTime;
        if (time < 0)
            time = 0;
        if (time > 1)
            time = 1;

        foreach (GameObject projectile in HealingProjectiles)
        {
            foreach (Transform t in projectile.GetComponentsInChildren<Transform>())
            {
                if (t == projectile.transform || t.gameObject.GetComponent<ParticleSystem>() != null)
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
    }

    private void OnDisable()
    {
        foreach (GameObject projectile in HealingProjectiles)
        {
            if (projectile != null)
                    projectile.SetActive(false);
        }
    }
}
