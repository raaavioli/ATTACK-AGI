using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageScript : MonoBehaviour
{
    private Team belongsTo;
    public float timeToHit;
    public float distance;
    public int damage;

    void FixedUpdate()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * (distance/timeToHit));
    }

    public void SetTeam(Team team)
    {
        belongsTo = team;
    }

    void OnTriggerEnter(Collider other) {
        CharacterCommon enemy = other.transform.GetComponent<CharacterCommon>();

        if (enemy != null){
            if (enemy.GetTeam() != belongsTo)
            {
                enemy.TakeDamage(damage);
            }
            else
            {
                Debug.Log("Hit Something else!");
            }
        }
    }
}
