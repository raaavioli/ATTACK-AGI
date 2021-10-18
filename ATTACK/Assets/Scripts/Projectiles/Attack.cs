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

    [Range(0.1f, 5.0f)]
    [SerializeField]
    protected float SecondsPerAttack = 1.5f;
    [Range(0.1f, 3.0f)]
    [SerializeField]
    protected float ChargeTime = 1.0f;
    [Range(0.1f, 3.0f)]
    [SerializeField]
    protected float MaxFireTime = 0.5f;
    [SerializeField]
    protected float TimeToHit = 0.0f;
    protected float SimulationTime = 0;

    public int Damage;
    private bool Simulating = false;
    private bool Shooting = false;
    Animator Animator;

    public int MaxTargets { get; private set; }
    private List<CharacterCommon> Targets = new List<CharacterCommon>();
    private List<bool> TargetsHit = new List<bool>();
    // Only give position info to subclasses, no need to expose whole CharacterCommon to subclass (as of now at least)
    protected List<Vector3> TargetPositions = new List<Vector3>();

    public bool CanAttack => !Simulating;

    protected CharacterMode Mode
    {
        get
        {
            return GetComponent<CharacterCommon>().Mode;
        }
    }

    protected abstract int GetMaxTargets();
    protected abstract void InstantiateProjectile();
    protected abstract void StartProjectile();
    protected abstract void UpdateProjectile();
    protected abstract void StopProjectile();

    public virtual void Awake()
    {
        Debug.Assert(SecondsPerAttack >= MaxFireTime + ChargeTime);
        // Ensure MaxTargets is set by subclass
        MaxTargets = GetMaxTargets();

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

    public bool StartAttack(List<CharacterCommon> Targets)
    {
        if (Targets.Count > MaxTargets)
        {
            Debug.LogError("Attempting to attack " + Targets.Count + " targets while MaxTargets is " + MaxTargets);
            return false;
        }

        if (!Simulating)
        {
            this.Targets.Clear();
            TargetPositions.Clear();
            TargetsHit.Clear();
            foreach (CharacterCommon c in Targets)
            {
                this.Targets.Add(c);
                TargetPositions.Add(c.transform.position);
                TargetsHit.Add(false);
            }
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

            bool DuringCharge = SimulationTime < ChargeTime;
            bool DuringFire = SimulationTime >= ChargeTime && SimulationTime < ChargeTime + MaxFireTime;
            bool AfterFire = SimulationTime >= ChargeTime + MaxFireTime;
            if (DuringCharge)
            {
                UpdateCharge(ref Charge);
                if(Charge != null)
                {
                    Charge.transform.position = AttackSource.transform.position;
                    Vector3 TargetPosition = Vector3.zero;
                    foreach (Vector3 v in TargetPositions)
                        TargetPosition += v;
                    TargetPosition /= TargetPositions.Count;
                    Charge.transform.rotation = Quaternion.LookRotation(TargetPosition - AttackSource.transform.position);
                }
            } 
            else if (!DuringCharge && Charge != null && Charge.isPlaying)
            {
                Charge.Stop();
            }

            if (!Shooting && DuringFire)
            {
                Shooting = true;
                StartProjectile();

                FireSource.time = 0;
                FireSource.Play();
            }
            else if (Shooting && DuringFire)
            {
                UpdateProjectile();
                for (int i = 0; i < TargetsHit.Count; i++)
                {
                    if (((SimulationTime - ChargeTime) >= TimeToHit) && !TargetsHit[i])
                    {
                        TargetsHit[i] = true;
                        if (Targets[i] != null)
                            Targets[i].TakeDamage(Damage);
                    }
                }
            }
            else if (Shooting && AfterFire)
            {
                // Deal damage to targets not yet hit
                for (int i = 0; i < TargetsHit.Count; i++)
                    if (Targets[i] != null && !TargetsHit[i])
                        Targets[i].TakeDamage(Damage);
                // Simulation finished
                Shooting = false;
                StopProjectile();
                if (Animator != null)
                    Animator.SetTrigger("StartIdle");
            } else if (SimulationTime >= SecondsPerAttack && AfterFire)
            {
                SimulationTime = 0;
                Simulating = false;
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
