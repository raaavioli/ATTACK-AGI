using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack : MonoBehaviour
{
    public GameObject AttackSource;

    public ParticleSystem ChargePrefab;
    private ParticleSystem Charge = null;

    public AudioClip ChargeSound;
    public AudioClip FireSound;
    private AudioSource ChargeSource;
    private AudioSource FireSource;

    [Range(0.1f, 3.0f)]
    public float FireStartTime = 1.0f;
    [Range(0.1f, 3.0f)]
    public float MaxFireTime = 0.5f;
    public float TimeToHit = 1.0f;
    protected float SimulationTime = 0;

    public int Damage;
    private bool Simulating = false;
    private bool Shooting = false;
    private bool TargetHit = false;
    Animator Animator;

    private CharacterCommon Target;
    protected Vector3 TargetPosition;

    public bool CanAttack => !Simulating;

    protected CharacterMode Mode
    {
        get
        {
            return GetComponent<CharacterCommon>().Mode;
        }
    }

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
            Charge = Instantiate(ChargePrefab, AttackSource.transform.position, AttackSource.transform.rotation);
            Charge.Stop();
        }
        InstantiateProjectile();
    }

    public void Update()
    {
        UpdateAttack();
    }

    public bool StartAttack(CharacterCommon Target)
    {
        if (!Simulating)
        {
            this.Target = Target;
            this.TargetPosition = Target.transform.position;
            TargetHit = false;
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
    private void UpdateAttack()
    {
        if (Simulating)
        {
            SimulationTime += Time.deltaTime;

            if (SimulationTime < FireStartTime)
            {
                UpdateCharge(ref Charge);
                if(Charge != null)
                {
                    Charge.transform.position = AttackSource.transform.position;
                    Charge.transform.rotation = Quaternion.LookRotation(TargetPosition - AttackSource.transform.position);
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

                FireSource.time = 0;
                FireSource.Play();
            }
            else if (Shooting && SimulationTime >= FireStartTime && SimulationTime < FireStartTime + MaxFireTime)
            {
                UpdateProjectile();
                if (SimulationTime - FireStartTime >= TimeToHit && !TargetHit)
                {
                    TargetHit = true;
                    if (Target != null)
                        Target.TakeDamage(Damage);
                }
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
