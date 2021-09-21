using UnityEngine;

public class ObjectShatter : MonoBehaviour
{
    public GameObject ShatteredObject;
    public ParticleSystem ImpactParticles;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Equals("Colliderplane"))
        {
            GameObject ShatteredPlaneInstance = Instantiate(ShatteredObject, transform.position, transform.rotation);
            IntensityUpdater iu = ShatteredPlaneInstance.GetComponent<IntensityUpdater>();
            float Lifetime = 4.0f;
            if (iu != null)
                Lifetime = iu.Lifetime;

            Vector3 ParticleDirection = transform.forward + Vector3.up;
            ParticleSystem Particles = Instantiate(ImpactParticles, transform.position, Quaternion.LookRotation(ParticleDirection));
            Particles.Play();

            foreach (Rigidbody part in ShatteredPlaneInstance.GetComponentsInChildren<Rigidbody>())
            {
                part.AddExplosionForce(120.0f, transform.position, 20.0f);
                Vector3 Forward = transform.forward;
                Forward.y = 0;
                part.AddForce(250f * Forward);
            }
            gameObject.SetActive(false);
            Destroy(ShatteredPlaneInstance, Lifetime);
        }
    }
}
