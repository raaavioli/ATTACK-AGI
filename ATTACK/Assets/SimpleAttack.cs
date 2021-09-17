using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAttack : MonoBehaviour
{
    private Team belogsTo;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * 3);
    }

    public void SetTeam(Team team)
    {
        belogsTo = team;
    }

    void OnTriggerEnter(Collider other) {
        CharacterCommon enemy = other.transform.GetComponent<CharacterCommon>();

        if (enemy.GetTeam() != belogsTo)
        {
            Debug.Log("Hit Enemy!");
        }
        else
        {
            Debug.Log("Hit Something else!");
        }
    }
}
