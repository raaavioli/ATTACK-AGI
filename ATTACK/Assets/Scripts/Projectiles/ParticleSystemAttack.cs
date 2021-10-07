using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;

public class ParticleSystemAttack : Attack
{
    public ParticleSystem ProjectilePrefab;

    protected ParticleSystem Projectile;


    protected override void InstantiateProjectile()
    {
        Projectile = Instantiate(ProjectilePrefab, transform.position + new Vector3(0, 0, 1), transform.rotation);
        Projectile.Stop();
    }

    protected override void StartProjectile()
    {
        Projectile.time = 0;
        Projectile.Play();
    }

    protected override void UpdateProjectile()
    {
        Projectile.transform.position = gameObject.transform.position;
        Projectile.transform.rotation = Quaternion.LookRotation(TargetPosition - transform.position);
    }

    protected override void StopProjectile()
    {
        Projectile.Stop();
    }

    private void OnDisable()
    {
        if (Projectile != null)
            Projectile.Stop();
    }
}
