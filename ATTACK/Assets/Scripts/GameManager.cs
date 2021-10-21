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

    private GameObject[] T1;
    private GameObject[] T2;

    private int spawnedCharacters = 0;

    private bool inCombatPhase = false;

    GameObject Canvas;

    public void Start()
    {
        T1 = new GameObject[TEAM_SIZE];
        T2 = new GameObject[TEAM_SIZE];

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

            // Spawn 2, 3, 0, 1, 2
            int character = (2 + position) % characters.Count;
            CharacterMode mode = character == 1 || character == 3 ? CharacterMode.Defensive : CharacterMode.Offensive;
            SpawnCharacter(position, characters[character], mode, team);
            Canvas.GetComponent<CardUI>().EnableHealthBar(true, team, position);

            spawnedCharacters++;
        }
    }

    private static int CountAlive(GameObject[] team)
    {
        int count = 0;
        foreach (GameObject o in team)
            if (o != null)
                count++;
        return count;
    }

    private void CombatPhaseUpdate()
    {
        CardUI cardUI = Canvas.GetComponent<CardUI>();

        int T1Alive = CountAlive(T1);
        int T2Alive = CountAlive(T2);
        if (T1Alive == 0 && T2Alive == 0) {
            cardUI.roundWinnerText.SetActive(true);
            cardUI.roundWinnerText.GetComponentsInChildren<Text>()[0].text = "Round ends in a tie!";
            return;
        }
        else if(T1Alive == 0)
        {
            cardUI.roundWinnerText.SetActive(true);
            cardUI.roundWinnerText.GetComponentsInChildren<Text>()[0].text = "Right Player won this round!";
            return;
        }
        else if(T2Alive == 0)
        {
            cardUI.roundWinnerText.SetActive(true);
            cardUI.roundWinnerText.GetComponentsInChildren<Text>()[0].text = "Left Player won this round!";
            return;
        }

        PerformAttacks(T1, T2);
        PerformAttacks(T2, T1);
    }

    private void PerformAttacks(GameObject[] attackers, GameObject[] opponents)
    {
        foreach (GameObject character in attackers)
        {
            if (character != null && character.activeSelf)
            {
                CharacterCommon cc = character.GetComponent<CharacterCommon>();
                bool isHealer = cc.GetComponent<Healing>() != null;
                if (cc != null && cc.CanAttack())
                    if (isHealer)
                    {
                        // Currently, the only healer there is heals its immediate neighbors
                        // If other healing spells are created, this will have to be changed. 
                        // Maybe have some target priority Enum for the Attack to base decisions on. 
                        int pos = GetCharacterPosition(cc.gameObject);
                        List<CharacterCommon> Neighbors = new List<CharacterCommon>();
                        if (pos > 0 && attackers[pos - 1] != null)
                            Neighbors.Add(attackers[pos - 1].GetComponent<CharacterCommon>());
                        if (pos < attackers.Length - 1 && attackers[pos + 1] != null)
                            Neighbors.Add(attackers[pos + 1].GetComponent<CharacterCommon>());
                        cc.Attack(Neighbors);
                    }
                    else
                    {
                        cc.Attack(GetTargets(opponents, cc.maxTargets));
                    }
            }
        }
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

    /**
     * Returns a target among the team "opponents".
     * Select any defensive target randomly, and offensive targets 
     * if no defensive targets are found.
     */
    private List<CharacterCommon> GetTargets(GameObject[] targets, int count)
    {
        int opponentCount = CountAlive(targets);
        List<CharacterCommon> Targets = new List<CharacterCommon>();
        if (opponentCount > 0 && count > 0)
        {
            List<CharacterCommon> defensiveOpponents = GetCharacters(targets, CharacterMode.Defensive);
            List<CharacterCommon> offensiveOpponents = GetCharacters(targets, CharacterMode.Offensive);
            if (count < defensiveOpponents.Count)
            {
                // Get some of the defensive opponents
                while (count > 0)
                {
                    CharacterCommon defensive = defensiveOpponents[UnityEngine.Random.Range(0, defensiveOpponents.Count)];
                    Targets.Add(defensive);
                    defensiveOpponents.Remove(defensive);
                    count--;
                }
            } else
            {
                // Get all defensive opponents and some offensive if possible
                foreach (CharacterCommon def in defensiveOpponents)
                    Targets.Add(def);
                count -= Targets.Count;
                while (count > 0 && offensiveOpponents.Count > 0)
                {
                    CharacterCommon offensive = offensiveOpponents[UnityEngine.Random.Range(0, offensiveOpponents.Count)];
                    Targets.Add(offensive);
                    offensiveOpponents.Remove(offensive);
                    count--;
                }
            }
        }

        return Targets;
    }

    private List<CharacterCommon> GetCharacters(GameObject[] characters, CharacterMode mode)
    {
        List<CharacterCommon> ccList = new List<CharacterCommon>();
        foreach (GameObject c in characters)
        {
            if (c == null)
                continue;
            CharacterCommon cc = c.GetComponent<CharacterCommon>();
            if (cc.Mode == mode)
                ccList.Add(cc);
        }
        return ccList;
    }

    /**
     * @param character The character to find position of
     * @returns the position of character in its Team, -1 if not found.
     **/
    public int GetCharacterPosition(GameObject character)
    {
        for (int i = 0; i < T1.Length; i++)
            if (T1[i] == character)
                return i;
        for (int i = 0; i < T2.Length; i++)
            if (T2[i] == character)
                return i;
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
        {
            for (int i = 0; i < T1.Length; i++)
                if (T1[i] == character)
                    T1[i] = null;
        } 
        else if (team == Team.Two)
        {
            for (int i = 0; i < T1.Length; i++)
                if (T1[i] == character)
                    T1[i] = null;
        }
        Destroy(character);
    }



    private void SpawnCharacter(int position, Character character, CharacterMode mode, Team team)
    {
        if (team == Team.One && T1[position] == null)
        {
            GameObject spawnPoint = spawnPointsT1[position];
            T1[position] = spawnPoint.GetComponent<Spawner>().Spawn(character, mode);
        }
        else if (team == Team.Two && T2[position] == null)
        {
            GameObject spawnPoint = spawnPointsT2[position];
            T2[position] = spawnPoint.GetComponent<Spawner>().Spawn(character, mode);
        }
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

                    SpawnCharacter(i, character, mode, team);

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