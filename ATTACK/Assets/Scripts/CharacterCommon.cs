using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCommon : MonoBehaviour
{
    private GameManager gameManager;
    
    private Team team;

    public GameObject attack;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        Attack();
    }

    void Update()
    {
        
    }

    public void SetTeam(Team team)
    {
        this.team = team;
    }

    public Team GetTeam()
    {
        return this.team;
    }

    private void Attack()
    {
        Transform targetPoint = gameManager.GetRandomTarget(team);

        GameObject projectile = Instantiate(attack, transform);
        projectile.GetComponent<SimpleAttack>().SetTeam(team);

        projectile.transform.LookAt(targetPoint);
    }
}
