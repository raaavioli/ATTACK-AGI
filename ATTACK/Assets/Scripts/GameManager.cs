using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject[] spawnPointsT1;
    private GameObject[] spawnPointsT2;
    public int teamSize = 3;

    void Start()
    {
        spawnPointsT1 = GameObject.FindGameObjectsWithTag("Team1Spawn");
        spawnPointsT2 = GameObject.FindGameObjectsWithTag("Team2Spawn");

        List<Character> characters = Character.Values();

        for (int i = 0; i < teamSize; i++)
        {
            SpawnCharacter(characters[i], spawnPointsT1[i], Team.Left);
        }

        for (int i = 0; i < teamSize; i++)
        {
            SpawnCharacter(characters[i], spawnPointsT2[i], Team.Right);
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

    private void SpawnCharacter(Character character, GameObject spawn, Team team)
    {
        Vector3 spawnPoint = spawn.transform.position;
        
        // Quaternions are required to Instantiate at a Vec3. Alternative will require a transform which makes the characters a child of their spawnpoint
        Quaternion towardsMiddle = new Quaternion(0, (int)team * 180, 0, 1); 

        GameObject c = Instantiate(character.GetModelPrefab(), spawnPoint, towardsMiddle);
        c.transform.localRotation = towardsMiddle;
        c.GetComponent<CharacterCommon>().SetTeam(team);
    }
}

public enum Team : ushort 
{
    Left = 0,
    Right = 1
}

public class Character
{
    public static readonly Character WITCH = new Character("Models/witch", "WitchPrefab");
    public static readonly Character ENIGMA = new Character("Models/enigma", "EnigmaPrefab");
    public static readonly Character COLONEL = new Character("Models/colonel", "ColonelPrefab");


    public static List<Character> Values()
    {
        return new List<Character>() { WITCH, ENIGMA, COLONEL };
    }

    private string ResourcePath;
    private string PrefabName;
    Character(string resourcePath, string prefabName) => (ResourcePath, PrefabName) = (resourcePath, prefabName);

    public GameObject GetModelPrefab ()
    {
        string path = ResourcePath + "/" + PrefabName;
        return Resources.Load<GameObject>(path);
    }
}