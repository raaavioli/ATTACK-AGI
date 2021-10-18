using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningAttack : ParticleSystemAttack
{
    protected override int GetMaxTargets()
    {
        return 1;
    }

    protected override void UpdateCharge(ref ParticleSystem Charge)
    {
        float MaxScale = 0.1f;
        float scale = 1 - ((ChargeTime - SimulationTime) / ChargeTime);
        scale *= MaxScale;
        if (scale < 0)
            scale = 0;

        Charge.transform.localScale = new Vector3(scale, scale, scale);
    }

    protected override void StartProjectile()
    {
        if (Mode == CharacterMode.Defensive)
            ProjectileMPB.SetColor("_Color", new Color(0, 0.88f, 1));
        else if (Mode == CharacterMode.Offensive)
            ProjectileMPB.SetColor("_Color", new Color(1, 0.21f, 0));
        Projectile.GetComponent<Renderer>().SetPropertyBlock(ProjectileMPB);
        
        float Distance = (AttackSource.transform.position - TargetPositions[0]).magnitude;
        Projectile.time = 0;
        ParticleSystem.MainModule main = Projectile.main;
        main.startLifetime = Distance * 1.1f / main.startSpeed.constant;
        Projectile.Play();
    }
}
