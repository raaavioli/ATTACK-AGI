using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject[] spawnPointsT1;
    private GameObject[] spawnPointsT2;

    public GameObject character;

    // Start is called before the first frame update
    void Start()
    {
        spawnPointsT1 = GameObject.FindGameObjectsWithTag("Team1Spawn");
        spawnPointsT2 = GameObject.FindGameObjectsWithTag("Team2Spawn");

        for (int i = 0; i < 3; i++)
        {
            SpawnCharacter(character, spawnPointsT1, i, 0);
        }

        for (int i = 0; i < 3; i++)
        {
            SpawnCharacter(character, spawnPointsT2, i, 1);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnCharacter(GameObject character, GameObject[] teamSpawns, int spawnIndex, int teamNumber)
    {
        Vector3 spawnPoint = teamSpawns[spawnIndex].transform.position;
        
        // Quaternions are required to Instantiate at a Vec3. Alternative will require a transform which makes the characters a child of their spawnpoint
        Quaternion towardsMiddle = new Quaternion(0, teamNumber * 180, 0, 1); 

        GameObject c = Instantiate(character, spawnPoint, towardsMiddle);
        c.GetComponent<CharacterCommon>().SetTeam(teamNumber);
    }
}
