using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack : MonoBehaviour
{
    public ParticleSystem ChargePrefab;

    [Range(0.1f, 3.0f)]
    public float FireStartTime = 1.0f;
    [Range(0.1f, 3.0f)]
    public float MaxFireTime = 0.5f;

    private ParticleSystem Charge = null;
    private bool Simulating = false;
    private bool Shooting = false;
    protected float SimulationTime = 0;
    protected Vector3 TargetPosition;
    Animator Animator;

    public bool CanAttack => !Simulating;

    protected abstract void InstantiateProjectile();
    protected abstract void StartProjectile();
    protected abstract void UpdateProjectile();
    protected abstract void StopProjectile();

    public void Awake()
    {
        Animator = gameObject.GetComponentInParent<Animator>();
        if (ChargePrefab != null)
        {
            Charge = Instantiate(ChargePrefab, this.transform.position, this.transform.rotation);
            Charge.Stop();
        }
        InstantiateProjectile();
    }

    public void Update()
    {
        RunSimulation();
    }

    public bool StartSimulation(Vector3 TargetPosition)
    {
        if (!Simulating)
        {
            this.TargetPosition = TargetPosition;
            // Start charging
            Simulating = true;
            if (Charge != null)
            {
                Charge.time = 0;
                Charge.Play();
            }
            if (Animator != null)
                Animator.SetTrigger("StartShoot");
            return true;
        }
        return false;
    }

    protected virtual void UpdateCharge(ref ParticleSystem Charge)
    {
        // Overridden in children, no default behaviour
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
                UpdateCharge(ref Charge);
                if(Charge != null)
                {
                    Charge.transform.position = transform.position;
                    Charge.transform.rotation = Quaternion.LookRotation(TargetPosition - transform.position);
                }
            } 
            else if (SimulationTime >= FireStartTime  && Charge != null && Charge.isPlaying)
            {
                Charge.Stop();
            }

            if (!Shooting && SimulationTime >= FireStartTime)
            {
                Shooting = true;
                StartProjectile();
            }
            else if (Shooting && SimulationTime >= FireStartTime && SimulationTime < FireStartTime + MaxFireTime)
            {
                UpdateProjectile();
            }
            else if (Shooting && SimulationTime >= FireStartTime + MaxFireTime)
            {
                // Simulation finished
                Simulating = false;
                Shooting = false;
                SimulationTime = 0;
                StopProjectile();
                if (Animator != null)
                    Animator.SetTrigger("StartIdle");
            }
        }
    }
}
