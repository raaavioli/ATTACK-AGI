using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum CharacterMode
{
    Offensive,
    Defensive
}
public class CharacterCommon : MonoBehaviour
{
    public CharacterMode Mode = CharacterMode.Offensive;
    public int health = 100;

    private GameManager gameManager;
    private Attack attack;

    private Animator animator;

    private Team team;
    private HealthScript healthScript;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        attack = GetComponent<Attack>();
        Assert.IsNotNull(attack);
        string parentName = transform.parent.name;
        healthScript = GameObject.Find("T" + parentName[1]).transform.GetChild(((int)parentName[3] - '0') - 1).GetComponent<HealthScript>();
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
    public bool CanAttack()
    {
        if (attack == null)
            return false;
        return attack.CanAttack;
    }

    /**
     * Performs an attack and starts character's Attack animation
     * @returns true if an attack simulation was started, false otherwise
     */
    public bool AttackRandom()
    {
        CharacterCommon target = gameManager.GetRandomTarget(team);
        Vector3 DirToTarget = (target.transform.position - transform.position).normalized;

        transform.rotation = Quaternion.LookRotation(DirToTarget);

        return attack.StartAttack(target);
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        healthScript.SetHealth((float)health / 100f);
        // Some characters dont have an animator, so this is a hacky solution for now.
        if (animator != null) animator.SetTrigger("StartGetHit");

        if (health <= 0)
        {
            gameManager.KillCharacter(team, gameObject);
            healthScript.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}