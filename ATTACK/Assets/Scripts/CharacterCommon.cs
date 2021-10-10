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

    private Animator animator;

    public int health = 100;

    public bool CanAttack() {
        if (attack == null)
            return false;
        return attack.CanAttack;
    }

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        attack = GetComponentInChildren<Attack>();
        Assert.IsNotNull(attack);

        animator = gameObject.GetComponent<Animator>();
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
        Vector3 targetPoint = gameManager.GetRandomTarget(team);
        Vector3 DirToTarget = (targetPoint - transform.position).normalized;

        transform.rotation = Quaternion.LookRotation(DirToTarget);

        return attack.StartSimulation(targetPoint);
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        // Some characters dont have an animator, so this is a hacky solution for now.
        if (animator != null) animator.SetTrigger("StartGetHit");

        if (health <= 0) gameManager.KillCharacter(team, gameObject);
    }
}