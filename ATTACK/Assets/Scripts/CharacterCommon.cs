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
    public CharacterMode Mode;
    private Team team;

    private GameManager gameManager;
    private Attack attack;
    public int maxTargets {
        get
        {
            if (attack != null)
                return attack.MaxTargets;
            return 0;
        }
    }

    private Animator animator;

    [SerializeField]
    private float maxHealth = 100;
    private float health;
    private HealthScript healthScript;

    void Awake()
    {
        Mode = CharacterMode.Offensive;
        health = maxHealth;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        attack = GetComponent<Attack>();
        Assert.IsNotNull(attack);

        string parentName = transform.parent.name;
        healthScript = GameObject.Find("T" + parentName[1]).transform.GetChild(((int)parentName[3] - '0') - 1).GetComponent<HealthScript>();
        animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        int position = gameManager.GetCharacterPosition(gameObject);
        if (position != -1 && CardManager.HasCard(team, position))
        {
            Card c = CardManager.GetCard(team, position);
            Mode = c.rotated ? CharacterMode.Defensive : CharacterMode.Offensive;
        }
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
    public bool Attack(List<CharacterCommon> targets)
    {
        Vector3 DirToTarget = Vector3.zero;
        if (targets.Count == 1)
            DirToTarget = targets[0].transform.position;
        else
            foreach (CharacterCommon target in targets)
                DirToTarget += target.transform.position;
        DirToTarget -= transform.position;

        transform.rotation = Quaternion.LookRotation(DirToTarget.normalized);

        return attack.StartAttack(targets);
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health > maxHealth)
            health = maxHealth;

        healthScript.SetHealth(health / maxHealth);
        // Some characters dont have an animator, so this is a hacky solution for now.
        if (animator != null) animator.SetTrigger("StartGetHit");

        if (health <= 0)
        {
            gameManager.KillCharacter(team, gameObject);
            healthScript.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}