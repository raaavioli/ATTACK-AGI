using UnityEngine;

public class ObjectShatter : MonoBehaviour {
    public GameObject ShatteredObject;
    public ParticleSystem ImpactParticles;
    public void Shatter() {
        GameObject ShatteredInstance = Instantiate(ShatteredObject, transform.position, transform.rotation);
        IntensityUpdater iu = ShatteredInstance.GetComponent<IntensityUpdater>();
        float Lifetime = 4.0f;
        if (iu != null)
            Lifetime = iu.Lifetime;

        Vector3 ParticleDirection = transform.forward + Vector3.up;
        ParticleSystem Particles = Instantiate(ImpactParticles, transform.position, Quaternion.LookRotation(ParticleDirection));
        Particles.Play();

        foreach (Rigidbody part in ShatteredInstance.GetComponentsInChildren<Rigidbody>()) {
            part.AddExplosionForce(100.0f, transform.position, 40.0f);
            Vector3 Forward = transform.forward;
            Forward.y = 0;
            part.AddForce(600f * Forward);
        }
        gameObject.SetActive(false);
        Destroy(ShatteredInstance, Lifetime);
    }
}
