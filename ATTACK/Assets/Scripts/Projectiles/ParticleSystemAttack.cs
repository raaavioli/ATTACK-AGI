using UnityEngine;

public class ParticleSystemAttack : Attack
{
    public ParticleSystem ProjectilePrefab;

    protected ParticleSystem Projectile;
    protected MaterialPropertyBlock ProjectileMPB;

    protected override void InstantiateProjectile()
    {
        ProjectileMPB = new MaterialPropertyBlock();
        Projectile = Instantiate(ProjectilePrefab, AttackSource.transform.position + new Vector3(0, 0, 1), AttackSource.transform.rotation);
        Projectile.Stop();
    }

    protected override void StartProjectile()
    {
        Projectile.time = 0;
        Projectile.Play();
    }

    protected override void UpdateProjectile()
    {
        Projectile.transform.position = AttackSource.transform.position;
        Projectile.transform.rotation = Quaternion.LookRotation(TargetPosition - AttackSource.transform.position);
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
