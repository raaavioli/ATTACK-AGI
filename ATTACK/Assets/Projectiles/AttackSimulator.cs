using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;

public class AttackSimulator : MonoBehaviour
{
    public ParticleSystem ChargePrefab;
    public ParticleSystem ProjectilePrefab;

    [Range(0.1f, 1.0f)]
    public float FireStartTime;
    [Range(0.1f, 1.0f)]
    public float MaxFireTime;

    private ParticleSystem Charge;
    private ParticleSystem Projectile;
    private bool Simulating = false;
    private bool Shooting = false;
    private float SimulationTime = 0;
    Animator Animator;
    public void Start()
    {
        Assert.IsTrue(MaxFireTime > 0);
        Animator = gameObject.GetComponentInParent<Animator>();
        Charge = Instantiate(ChargePrefab, this.transform.position, this.transform.rotation);
        Charge.Stop();
        Projectile = Instantiate(ProjectilePrefab, this.transform.position + new Vector3(0, 0, 1), this.transform.rotation);
        Projectile.Stop();
    }

    public void Update()
    {
        // ----------------------------------------------------------------------------------//
        // Just to illustrate how to emit particles, should otherwise be done from elsewhere,
        // i.e by the system controlling the battle
        if (Input.GetMouseButton(0))
            StartSimulation();
        // ---------------------------------------------------------------------------------//

        RunSimulation();
    }

    public void StartSimulation()
    {
        if (!Simulating)
        {
            // Start charging
            Simulating = true;
            Charge.Play();
            Animator.SetTrigger("StartShoot");
        }
    }

    /**
     * Runs the simulation and repositions particle systems.
     * Should only be run from Update
     */
    private void RunSimulation()
    {
        if (Simulating)
        {
            SimulationTime += Time.deltaTime;

            if (SimulationTime < FireStartTime)
            {
                // Charge
                float scale = 0.1f * SimulationTime;
                Charge.transform.localScale = new Vector3(scale, scale, scale);
                Charge.transform.position = gameObject.transform.position;
            }
            else if (Charge.isPlaying && SimulationTime >= FireStartTime)
            {
                Charge.Stop();
            }

            if (!Shooting && SimulationTime >= FireStartTime)
            {
                // Start firing
                Shooting = true;
                Projectile.Play();
            }
            else if (Shooting && SimulationTime >= FireStartTime && SimulationTime < FireStartTime + MaxFireTime)
            {
                // Fire
                Projectile.transform.position = gameObject.transform.position;
            }
            else if (Shooting && SimulationTime >= FireStartTime + MaxFireTime)
            {
                // Simulation finished
                Simulating = false;
                Shooting = false;
                SimulationTime = 0;
                Projectile.Stop();
                Animator.SetTrigger("StartIdle");
            }
        }
    }
}
