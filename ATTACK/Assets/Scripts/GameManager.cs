using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ServerHandler;

public class GameManager : MonoBehaviour
{
    private GameObject[] spawnPointsT1;
    private GameObject[] spawnPointsT2;
    public int teamSize = 3;

    private List<GameObject> T1;
    private List<GameObject> T2;
    public void Start()
    {
        spawnPointsT1 = GameObject.FindGameObjectsWithTag("Team1Spawn");
        spawnPointsT2 = GameObject.FindGameObjectsWithTag("Team2Spawn");
        T1 = new List<GameObject>();
        T2 = new List<GameObject>();
        List<Character> characters = Character.Values();

        //for (int i = 0; i < teamSize; i++)
        //{
        //    SpawnCharacter(characters[i], spawnPointsT1[i], Team.Left);
        //}

        //for (int i = 0; i < teamSize; i++)
        //{
        //    SpawnCharacter(characters[i], spawnPointsT2[i], Team.Right);
        //}

        ServerHandler.onCardDataReceived += SpawnFromCards;

    }

    public void Update()
    {
        foreach (GameObject character in T1)
        {
            CharacterCommon cc = character.GetComponent<CharacterCommon>();
            if (cc.CanAttack)
            {
                cc.AttackRandom();
            }
        }

        foreach (GameObject character in T2)
        {
            CharacterCommon cc = character.GetComponent<CharacterCommon>();
            if (cc.CanAttack)
            {
                cc.AttackRandom();
            }
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
        if (team == 0)
        {
            T1.Add(c);
        } 
        else
        {
            T2.Add(c);
        }

        c.transform.parent = spawn.transform;
    }

    private void SpawnFromCards() {
        CardPosition[] cardPositions = cardInformation;

        foreach (CardPosition cardPosition in cardPositions) {
            // Decide team, and skip if the team is already full.
            Team team = cardPosition.team;
            if ((team == 0 && T1.Count >= 3) || (team == (Team) 1 && T2.Count >= 3)) {
                continue;
            }

            // Decide the spawn point.
            int position = cardPosition.position;
            GameObject spawn;
            if (team == 0) {
                spawn = spawnPointsT1[(int)Mathf.Clamp(position-1, 0, 2)];
            } else {
                spawn = spawnPointsT2[(int)Mathf.Clamp(position-1, 0, 2)];
            }

            // Skip spawn if spawn point is not empty.
            if (spawn.transform.childCount != 0) {
                continue;
            }

            // Decide the character.
            Character wantedCharacter;
            switch (cardPosition.rank) {
                case 1:
                    wantedCharacter = Character.WITCH;
                    break;
                case 2:
                    wantedCharacter = Character.ENIGMA;
                    break;
                case 3:
                    wantedCharacter = Character.COLONEL;
                    break;
                default:
                    wantedCharacter = Character.WITCH;
                    break;
            }

            SpawnCharacter(wantedCharacter, spawn, team);
        }
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