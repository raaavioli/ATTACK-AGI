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

    private ParticleSystem Charge;
    private bool Simulating = false;
    private bool Shooting = false;
    protected float SimulationTime = 0;
    Animator Animator;

    protected abstract void InstantiateProjectile();
    protected abstract void StartProjectile();
    protected abstract void UpdateProjectile();
    protected abstract void StopProjectile();



    public void Start()
    {
        Animator = gameObject.GetComponentInParent<Animator>();
        Charge = Instantiate(ChargePrefab, this.transform.position, this.transform.rotation);
        Charge.Stop();
        InstantiateProjectile();
    }

    public void Update()
    {
        // ----------------------------------------------------------------------------------//
        // Just to illustrate how to run an attack, currently all attacks are run when pressing
        // the mouse.
        // StartSimulation will later be called by the game manager
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
            Charge.time = 0;
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
                Charge.transform.position = gameObject.transform.position;
            }
            else if (Charge.isPlaying && SimulationTime >= FireStartTime)
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
                Animator.SetTrigger("StartIdle");
            }
        }
    }
}
