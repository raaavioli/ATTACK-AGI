using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack : MonoBehaviour
{
    public ParticleSystem ChargePrefab;

    public AudioClip ChargeSound;
    public AudioClip FireSound;
    private AudioSource ChargeSource;
    private AudioSource FireSource;

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

    private GameObject hiddenDamageCollider;
    public float timeToHit = 1.0f;
    public int damage;

    public bool CanAttack => !Simulating;

    protected abstract void InstantiateProjectile();
    protected abstract void StartProjectile();
    protected abstract void UpdateProjectile();
    protected abstract void StopProjectile();

    public void Awake()
    {
        Animator = gameObject.GetComponentInParent<Animator>();
        if (ChargeSource == null)
        {
            ChargeSource = gameObject.AddComponent<AudioSource>();
            ChargeSource.playOnAwake = false;
            ChargeSource.clip = ChargeSound;
            ChargeSource.spatialBlend = 0.9f;
            ChargeSource.Stop();
        }
        if (FireSource == null)
        {
            FireSource = gameObject.AddComponent<AudioSource>();
            FireSource.playOnAwake = false;
            FireSource.clip = FireSound;
            FireSource.spatialBlend = 0.95f;
            FireSource.Stop();
        }

        if (ChargePrefab != null)
        {
            Charge = Instantiate(ChargePrefab, this.transform.position, this.transform.rotation);
            Charge.Stop();
        }
        InstantiateProjectile();
    }

    private void Start() {
        hiddenDamageCollider = Resources.Load<GameObject>("Projectiles/HiddenDamageCollider");    
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
                ChargeSource.time = 0f;
                ChargeSource.Play();
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

                fireDamageProjectile();

                FireSource.time = 0;
                FireSource.Play();
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

    private void fireDamageProjectile()
    {
        GameObject hiddenAttack = Instantiate(hiddenDamageCollider, gameObject.transform.position, Quaternion.identity);
        hiddenAttack.transform.LookAt(TargetPosition);

        HiddenDamageScript stats = hiddenAttack.GetComponent<HiddenDamageScript>();
        stats.SetTeam(gameObject.GetComponentInParent<CharacterCommon>().GetTeam());
        stats.timeToHit = timeToHit;
        stats.distance = (TargetPosition - gameObject.transform.position).magnitude;
        stats.damage = damage;
    }

    private void OnDisable()
    {
        if (Charge != null)
            Charge.Stop();
    }

    private void OnDestroy()
    {
        if (Charge != null)
            Charge.Stop();
    }
}
