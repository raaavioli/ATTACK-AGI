using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

public class CharacterCommon : MonoBehaviour
{
    private GameManager gameManager;
    private Team team;
    private Attack attack;

    public bool CanAttack => attack.CanAttack;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        attack = GetComponentInChildren<Attack>();
        Assert.IsNotNull(attack);
    }

    public void SetTeam(Team team)
    {
        this.team = team;
    }

    public Team GetTeam()
    {
        return this.team;
    }

    /**
     * Performs an attack and starts character's Attack animation
     * @returns true if an attack simulation was started, false otherwise
     */
    public bool AttackRandom()
    {
        Transform targetPoint = gameManager.GetRandomTarget(team);
        Vector3 DirToTarget = (targetPoint.position - gameObject.transform.position).normalized;

        transform.rotation = Quaternion.LookRotation(DirToTarget);

        return attack.StartSimulation(targetPoint.position);
    }
}
