using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject[] spawnPointsT1;
    private GameObject[] spawnPointsT2;
    public int teamSize = 3;

    public GameObject character;

    void Start()
    {
        spawnPointsT1 = GameObject.FindGameObjectsWithTag("Team1Spawn");
        spawnPointsT2 = GameObject.FindGameObjectsWithTag("Team2Spawn");

        for (int i = 0; i < teamSize; i++)
        {
            SpawnCharacter(character, spawnPointsT1[i], Team.Left);
        }

        for (int i = 0; i < teamSize; i++)
        {
            SpawnCharacter(character, spawnPointsT2[i], Team.Right);
        }

    }

    public Transform GetRandomTarget(Team characterTeam)
    {
        int enemy = Random.Range(0, teamSize);

        if (characterTeam == Team.Left)
        {
            return spawnPointsT2[enemy].transform;
        }
        else
        {
            return spawnPointsT1[enemy].transform;
        }
    }

    private void SpawnCharacter(GameObject character, GameObject spawn, Team team)
    {
        Vector3 spawnPoint = spawn.transform.position;
        
        // Quaternions are required to Instantiate at a Vec3. Alternative will require a transform which makes the characters a child of their spawnpoint
        Quaternion towardsMiddle = new Quaternion(0, (int)team * 180, 0, 1); 

        GameObject c = Instantiate(character, spawnPoint, towardsMiddle);
        c.GetComponent<CharacterCommon>().SetTeam(team);
    }
}

public enum Team : ushort 
{
    Left = 0,
    Right = 1
}