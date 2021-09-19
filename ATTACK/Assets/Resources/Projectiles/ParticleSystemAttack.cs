using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;

public class ParticleSystemAttack : Attack
{
    public ParticleSystem ProjectilePrefab;

    private ParticleSystem Projectile;


    protected override void InstantiateProjectile()
    {
        Projectile = Instantiate(ProjectilePrefab, this.transform.position + new Vector3(0, 0, 1), this.transform.rotation);
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
    }

    protected override void StopProjectile()
    {
        Projectile.Stop();
    }
}
