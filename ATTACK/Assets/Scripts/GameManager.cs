using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] spawnPointsT1;
    [SerializeField]
    private GameObject[] spawnPointsT2;
    
    [SerializeField]
    [Range(1, 30)]
    private int setupTime = 30;
    private const int TEAM_SIZE = CardManager.MAX_CARDS_PER_TEAM;

    private List<GameObject> T1;
    private List<GameObject> T2;

    private int spawnedCharacters = 0;

    private bool inCombatPhase = false;

    GameObject Canvas;

    public void Start()
    {
        T1 = new List<GameObject>();
        T2 = new List<GameObject>();

        Canvas = GameObject.Find("Canvas");
        Canvas.GetComponent<CardUI>().roundWinnerText.SetActive(false);

        StartCoroutine(SetupPhaseTimer(setupTime));
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
            int position = (int)(spawnedCharacters / 2f);
            Team team = (spawnedCharacters % 2).AsTeam();
            GameObject spawnPoint = team == Team.One ? spawnPointsT1[position] : spawnPointsT2[position];
            CharacterMode mode = team == Team.One ? CharacterMode.Defensive : CharacterMode.Offensive;
            SpawnCharacter(characters[position % characters.Count], mode, spawnPoint, team);
            Canvas.GetComponent<CardUI>().EnableHealthBar(true, team, position);

            spawnedCharacters++;
        }
    }

    private void CombatPhaseUpdate()
    {
        CardUI cardUI = Canvas.GetComponent<CardUI>();
        if (T1.Count == 0 && T2.Count == 0) {
            cardUI.roundWinnerText.SetActive(true);
            cardUI.roundWinnerText.GetComponentsInChildren<Text>()[0].text = "Round ends in a tie!";
        }
        else if(T1.Count == 0)
        {
            cardUI.roundWinnerText.SetActive(true);
            cardUI.roundWinnerText.GetComponentsInChildren<Text>()[0].text = "Right Player won this round!";
        }
        else if(T2.Count == 0)
        {
            cardUI.roundWinnerText.SetActive(true);
            cardUI.roundWinnerText.GetComponentsInChildren<Text>()[0].text = "Left Player won this round!";
        }

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
        
        CardUI cardUI = Canvas.GetComponent<CardUI>();
        cardUI.setupTimer.SetActive(true);
        for (int i = 0; i < seconds; i++)
        {
            if (seconds - i == startSoundTime)
                GetComponent<AudioSource>().Play();
            updateUITimer(seconds - i);
            yield return new WaitForSeconds(1f);
        }
        cardUI.setupTimer.SetActive(false);
        
        inCombatPhase = true;
        SpawnFromCards();
        CameraHandler.instance.StartCombatCamera();
    }

    private void updateUITimer(int secondsLeft)
    {
        CardUI cardUI = Canvas.GetComponent<CardUI>();
        Text setupTimerText = cardUI.setupTimer.GetComponent<Text>();
        setupTimerText.text = ""+secondsLeft;
        if(secondsLeft < 4)
        {
            if(secondsLeft == 1)
                setupTimerText.color = Color.red;
            else
                setupTimerText.color = Color.yellow;
            setupTimerText.fontSize += 16;
        } 
        
    }

    public CharacterCommon GetRandomTarget(Team characterTeam)
    {
        if (characterTeam == Team.One && T2.Count > 0)
            return T2[(int)UnityEngine.Random.Range(0, T2.Count)].GetComponent<CharacterCommon>();
        else if (characterTeam == Team.Two && T1.Count > 0)
            return T1[(int)UnityEngine.Random.Range(0, T1.Count)].GetComponent<CharacterCommon>();

        Debug.LogError("Could not get random target, there are no targets in the opposing team");
        Assert.IsTrue(false);
        return null;
    }

    /**
     * @param character The character to find position of
     * @returns the position of character in its Team, -1 if not found.
     **/
    public int GetCharacterPosition(GameObject character)
    {
        if (T1.Contains(character))
            return T1.IndexOf(character);
        else if (T2.Contains(character))
            return T2.IndexOf(character);
        return -1;
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
            T1.Remove(character);
        else 
            T2.Remove(character);
        Destroy(character);
    }



    private void SpawnCharacter(Character character, CharacterMode mode, GameObject spawn, Team team)
    {     
        GameObject c = spawn.GetComponent<Spawner>().Spawn(character, mode);
        CharacterCommon cc = c.GetComponent<CharacterCommon>();

        if (team == Team.One)
            T1.Add(c);
        else
            T2.Add(c);

        // THIS IS NECESSARY ONLY FOR THE INITIAL WAY OF SPAWNING FROM CARD INFORMATION, SINCE IT RELIES ON CHILD COUNT CHECKING
        c.transform.parent = spawn.transform;
    }

    private void SpawnFromCards() 
    {
        foreach (Team team in Enum.GetValues(typeof(Team)))
        {
            for(int i = 0; i < TEAM_SIZE; i++) {
                if (CardManager.HasCard(team, i)) {
                    Card card = CardManager.GetCard(team, i);
                    GameObject spawn = team == Team.One ? spawnPointsT1[card.position] : spawnPointsT2[card.position];
                    Character character = Character.Values()[card.rank % Character.Values().Count];
                    CharacterMode mode = card.rotated ?
                        CharacterMode.Defensive :
                        CharacterMode.Offensive;

                    SpawnCharacter(character, mode, spawn, team);

                    Canvas.GetComponent<CardUI>().EnableHealthBar(true, team, i);
                }
            }
        }
    }
}

public enum Team : int
{
    One = 0,
    Two = 1,
}

public static class TeamExtension
{
    public static Team AsTeam(this int team)
    {
        if (team == 0)
            return Team.One;
        return Team.Two;
    }
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