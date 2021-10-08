using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    private GameObject[] spawnPointsT1;
    private GameObject[] spawnPointsT2;
    private const int TEAM_SIZE = 6;

    private List<GameObject> T1;
    private List<GameObject> T2;

    private int spawnedCharacters = 0;

    private bool inCombatPhase = false;

    public void Start()
    {
        spawnPointsT1 = GameObject.FindGameObjectsWithTag("Team1Spawn");
        spawnPointsT2 = GameObject.FindGameObjectsWithTag("Team2Spawn");
        T1 = new List<GameObject>();
        T2 = new List<GameObject>();

        /*for (int i = 0; i < teamSize; i++)
        {
            SpawnCharacter(characters[i], spawnPointsT1[i], Team.Left);
        }

        for (int i = 0; i < teamSize; i++)
        {
            SpawnCharacter(characters[i], spawnPointsT2[i], Team.Right);
        }*/

        StartCoroutine(SetupPhaseTimer(30));
    }

    public void Update()
    {
        if (inCombatPhase) {
            CombatPhaseUpdate();
        } else {
            SetupPhaseUpdate();
        }
        // Restarts scene on r press.
        if (Input.GetKeyDown("r")) SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    private void SetupPhaseUpdate()
    {
        // Just to test spawning, will soon be replaced by some event from the 
        // SUR40 input server
        if (Input.GetMouseButtonDown(0) && spawnedCharacters < TEAM_SIZE * 2)
        {
            List<Character> characters = Character.Values();
            int team = spawnedCharacters % 2;
            int character = (int)(spawnedCharacters / 2f);
            if (team == 0)
                SpawnCharacter(characters[character % characters.Count], spawnPointsT1[character], Team.One);
            else
                SpawnCharacter(characters[character % characters.Count], spawnPointsT2[TEAM_SIZE - 1 - character], Team.Two);
            spawnedCharacters++;
        }
    }

    private void CombatPhaseUpdate()
    {
        foreach (GameObject character in T1) // Start attacks
        {
            if (character.activeSelf)
            {
                CharacterCommon cc = character.GetComponent<CharacterCommon>();
                if (cc != null && cc.CanAttack() && T2.Count > 0)
                {
                    cc.AttackRandom();
                }
            }
        }

        foreach (GameObject character in T2) // Start attacks
        {
            if (character.activeSelf)
            {
                CharacterCommon cc = character.GetComponent<CharacterCommon>();
                if (cc != null && cc.CanAttack() && T1.Count > 0)
                {
                    cc.AttackRandom();
                }
            }
        }

        // If one team is dead, end combat phase.
    }

    private IEnumerator SetupPhaseTimer(int seconds) // Timer for when setup ends.
    {
        const float startSoundTime = 3.0f;
        Assert.IsTrue(seconds > startSoundTime);

        for (int i = 0; i < seconds; i++)
        {
            if (seconds - i == startSoundTime)
                GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(1f);
        }
        inCombatPhase = true;
        SpawnFromCards();
        CameraHandler.instance.StartCombatCamera();
    }

    public Vector3 GetRandomTarget(Team characterTeam)
    {
        if (characterTeam == Team.One && T2.Count > 0)
            return T2[(int)Random.Range(0, T2.Count)].transform.position;
        else if (characterTeam == Team.Two && T1.Count > 0)
            return T1[(int)Random.Range(0, T1.Count)].transform.position;

        Debug.LogError("Could not get random target, there are no targets in the opposing team");
        Assert.IsTrue(false);
        return new Vector3(0, 0, 0);
    }

    public static Team GetTeamFromTag(string tag)
    {
        switch (tag)
        {
            case "Team1Spawn":
                return Team.One;
            case "Team2Spawn":
                return Team.Two;
            default:
                Debug.LogError("There is no team with tag: " + tag);
                Assert.IsTrue(false);
                break;
        }
        return Team.One;
    }
    
    public void KillCharacter(Team team, GameObject character)
    {
        if (team == Team.One)
        {
            T1.Remove(character);
        } 
        else 
        {
            T2.Remove(character);
        }
        Destroy(character);
    }

    private void SpawnCharacter(Character character, GameObject spawn, Team team)
    {     
        GameObject c = spawn.GetComponent<Spawner>().Spawn(character);

        if (team == 0)
            T1.Add(c);
        else
            T2.Add(c);

        // THIS IS NECESSARY ONLY FOR THE INITIAL WAY OF SPAWNING FROM CARD INFORMATION, SINCE IT RELIES ON CHILD COUNT CHECKING
        c.transform.parent = spawn.transform;
    }

    private void SpawnFromCards() 
    {
        ServerHandler.CardPosition[] cardPositions = ServerHandler.cardInformation;

        foreach (ServerHandler.CardPosition cardPosition in cardPositions) {
            // Decide team, and skip if the team is already full.
            Team team = cardPosition.team;
            if ((team == 0 && T1.Count >= TEAM_SIZE) || (team == (Team)1 && T2.Count >= TEAM_SIZE))
            {
                continue;
            }

            // Decide the spawn point.
            int position = cardPosition.position;
            GameObject spawn;
            if (team == 0)
                spawn = spawnPointsT1[position - 1];
            else
                spawn = spawnPointsT2[position - 1];

            // Decide the character.
            Character wantedCharacter = Character.Values()[cardPosition.rank % Character.Values().Count];

            SpawnCharacter(wantedCharacter, spawn, team);
        }
    }
}

public enum Team : ushort
{
    One = 0,
    Two = 1,
}

public class Character
{
    public static readonly Character WITCH = new Character("Models/witch", "WitchPrefab");
    public static readonly Character ENIGMA = new Character("Models/enigma", "EnigmaPrefab");
    public static readonly Character COLONEL = new Character("Models/colonel", "ColonelPrefab");
    public static readonly Character SQUISHY = new Character("Models/squishy", "SquishyPrefab");


    public static List<Character> Values()
    {
        return new List<Character>() { WITCH, ENIGMA, COLONEL, SQUISHY };
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

// Used for spawn at combat phase start.
public struct Triple
{
    public Character character;
    public GameObject spawn;
    public Team team;

    public Triple(Character character_, GameObject spawn_, Team team_) {
        character = character_;
        spawn = spawn_;
        team = team_;
    }
}