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

    private int round = 0;
    private const int MAX_ROUNDS = 5;
    private int t1Score = 0;
    private int t2Score = 0;
    private GameState state = GameState.RoundOver;

    UIController UIController;

    public void Start()
    {
        T1 = new GameObject[TEAM_SIZE];
        T2 = new GameObject[TEAM_SIZE];

        UIController = GameObject.Find("Canvas").GetComponent<UIController>();
        UIController.SetTotalRounds(MAX_ROUNDS);
    }


    public void Update()
    {
        if (state == GameState.Combat) {
            CombatPhaseUpdate();
        } 
        else if (state == GameState.Setup) {
            SetupPhaseUpdate();
        } 
        else if (state == GameState.RoundOver)
        {
            if (Input.GetKeyDown("s")) StartCoroutine(SetupPhaseTimer(setupTime));
        }

        // Restarts scene on r press.
        if (Input.GetKeyDown("r")) SceneManager.LoadScene(SceneManager.GetActiveScene().name);


        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    private void SetupPhaseUpdate()
    {
        // For debug purpose to be able to spawn by clicking when not running on SUR40
        if (Input.GetMouseButtonDown(0) && spawnedCharacters < TEAM_SIZE * 2)
        {
            List<Character> characters = Character.Values();
            int position = (int)(spawnedCharacters / 2f);
            Team team = (spawnedCharacters % 2).AsTeam();

            // Spawn 2, 3, 0, 1, 2
            int character = (2 + position) % characters.Count;
            CharacterMode mode = CharacterMode.Offensive;
            SpawnCharacter(position, characters[character], mode, team);

            spawnedCharacters++;
        }

        // Spawn from cards during the setup phase.
        SpawnFromCards();

        // Toggle visibility for all living characters
        for (int i = 0; i < TEAM_SIZE; i++)
        {
            UIVisibility T1visible = T1[i] == null ? UIVisibility.None : UIVisibility.All;
            UIController.SetStatsVisible(T1visible, Team.One, i);

            UIVisibility T2visible = T2[i] == null ? UIVisibility.None : UIVisibility.All;
            UIController.SetStatsVisible(T2visible, Team.Two, i);
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

    private static int CountHealers(GameObject[] team) {
        int count = 0;
        foreach(GameObject o in team) {
            if (o != null) {
                if (o.name.Contains("Doctor")) {
                    count++;
				}
			}
		}
        return count;
	}

    private void CombatPhaseUpdate()
    {
        int T1Alive = CountAlive(T1);
        int T2Alive = CountAlive(T2);
        if (T1Alive == 0 || T2Alive == 0) {
            state = GameState.RoundOver;
            string text = T1Alive == 0 && T2Alive == 0 || OnlyHealersAlive(T1Alive, T2Alive) ? "Round ends in a tie!" :
                          T1Alive == 0 ? "Red Team won this round!" : "Blue Team won this round!";

            if (T1Alive > T2Alive)
                t1Score++;
            else if (T2Alive > T1Alive)
                t2Score++;

            if (round == MAX_ROUNDS)
            {
                state = GameState.GameOver;
                text = t1Score > t2Score ? "Blue Team won the game!" :
                    t2Score > t1Score ? "Red Team won the game!" : "Game ends in a tie!";
            }

            UIController.SetScore(t1Score, t2Score);
            UIController.ShowRoundWinnerText(true, text);
            UIController.ShowScoreBoard(true);

            return;
        }

        PerformAttacks(Team.One);
        PerformAttacks(Team.Two);

        CheckHealth(Team.One);
        CheckHealth(Team.Two);
    }

    private bool OnlyHealersAlive(int T1Alive, int T2Alive) {
        int T1Healers = CountHealers(T1);
        int T2Healers = CountHealers(T2);

        return T1Alive == T1Healers && T2Alive == T2Healers;
	}

    private void PerformAttacks(Team attacking)
    {
        GameObject[] attackers = attacking == Team.One ? T1 : T2;
        GameObject[] opponents = attacking == Team.One ? T2 : T1;
        foreach (GameObject character in attackers)
        {
            if (character != null && character.activeSelf)
            {
                CharacterCommon cc = character.GetComponent<CharacterCommon>();
                bool isHealer = cc.GetComponent<Healing>() != null;
                if (cc != null && cc.CanAttack()) {
                    if (isHealer) {
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
                    } else {
                        cc.Attack(GetTargets(opponents, cc.maxTargets));
                    }

                    if (cc.CanExecuteSpecial()) {
                        cc.PerformSpecial(attackers);
					}
                }

            }
        }
    }

    private void CheckHealth(Team team)
    {
        // Check and update health status and remove dead characters
        GameObject[] characters = team == Team.One ? T1 : T2;
        for (int i = 0; i < TEAM_SIZE; i++)
        {

            if (characters[i] == null)
                continue;
            float health = characters[i].GetComponent<CharacterCommon>().GetHealth();
            if (health <= 0f)
                KillCharacter(team, characters[i]);
            UIController.SetHealth(health, team, i);

        }
    }

    private IEnumerator SetupPhaseTimer(int seconds) // Timer for when setup ends.
    {
        CameraHandler.instance.StartSetupCameras();
        const float startSoundTime = 3.0f;
        Assert.IsTrue(seconds > startSoundTime);
        // Set state and setup UI
        state = GameState.Setup;
        UIController.SetRound(++this.round);
        UIController.ShowRoundWinnerText(false, "");
        UIController.ShowScoreBoard(true);

        // Clear old board state
        foreach (GameObject character in T1)
            if (character != null)
                KillCharacter(Team.One, character);
        foreach (GameObject character in T2)
            if (character != null)
                KillCharacter(Team.Two, character);
        spawnedCharacters = 0;

        // Start count down
        for (int i = 0; i < seconds; i++)
        {
            if (seconds - i == startSoundTime)
                GetComponent<AudioSource>().Play();
            UIController.SetTimer(seconds - i);
            yield return new WaitForSeconds(1f);
        }
        UIController.SetTimer(setupTime);
        UIController.ShowScoreBoard(false);

        // Prepare for combat
        state = GameState.Combat;
        for (int i = 0; i < TEAM_SIZE; i++)
        {
            // Toggle combat visibility for all living character cards
            if (T1[i] != null)
                UIController.SetStatsVisible(UIVisibility.Reduced, Team.One, i);
            if (T2[i] != null)
                UIController.SetStatsVisible(UIVisibility.Reduced, Team.Two, i);
        }
        CameraHandler.instance.StartCombatCamera();
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
    
    private void KillCharacter(Team team, GameObject character)
    {
        if (team == Team.One)
        {
            for (int i = 0; i < T1.Length; i++)
                if (T1[i] == character)
                    T1[i] = null;
        } 
        else if (team == Team.Two)
        {
            for (int i = 0; i < T2.Length; i++)
                if (T2[i] == character)
                    T2[i] = null;
        }

        spawnedCharacters--;
        Destroy(character);
    }



    private void SpawnCharacter(int position, Character character, CharacterMode mode, Team team)
    {
        GameObject[] characters = team == Team.One ? T1 : T2;
        GameObject[] spawnPoints = team == Team.One ? spawnPointsT1 : spawnPointsT2;
        if (characters[position] == null)
        {
            GameObject spawnPoint = spawnPoints[position];
            characters[position] = spawnPoint.GetComponent<Spawner>().Spawn(character, mode);
            UIController.SetStats(character.Stats, team, position);
            UIController.SetHealth(1f, team, position);
            UIController.SetStatsVisible(UIVisibility.All, team, position);
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

					// Clear position if it is already taken.
					GameObject potentialCharacter = team == Team.One ? T1[i] : T2[i];
					if (potentialCharacter != null && potentialCharacter.GetComponent<CharacterCommon>().character != character) {
						KillCharacter(team, potentialCharacter);
					}

					CharacterMode mode = card.rotated ?
                        CharacterMode.Defensive :
                        CharacterMode.Offensive;

                    SpawnCharacter(i, character, mode, team);
                }
            }
        }
    }
}

public enum GameState {
    Setup,
    Combat,
    RoundOver,
    GameOver,
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

    public static string ShortString(this Team team)
    {
        if (team == Team.One)
            return "T1";
        return "T2";
    }
}

public class Character
{
    public static readonly Character WITCH = new Character("Models/witch", "WitchPrefab", 
        new CharacterStats("Witch", 3, 2, 5));
    public static readonly Character ENIGMA = new Character("Models/enigma", "EnigmaPrefab",
        new CharacterStats("Enigma", 5, 1, 3));
    public static readonly Character COLONEL = new Character("Models/colonel", "ColonelPrefab",
        new CharacterStats("Colonel", 4, 4, 1));
    public static readonly Character SQUISHY = new Character("Models/squishy", "SquishyPrefab",
        new CharacterStats("Squishy", 4, 4, 3));
    public static readonly Character DOCTOR = new Character("Models/doctor", "DoctorPrefab",
        new CharacterStats("Doctor", 1, 2, 4));
    public static readonly Character ARMOR = new Character("Models/armor", "ArmorPrefab",
        new CharacterStats("Armor", 1, 5, 2));


    public static List<Character> Values()
    {
        return new List<Character>() { WITCH, ENIGMA, COLONEL, SQUISHY, DOCTOR, ARMOR };
    }

    private string ResourcePath;
    private string PrefabName;
    public CharacterStats Stats { get; private set; }
    Character(string resourcePath, string prefabName, CharacterStats stats) => (ResourcePath, PrefabName, Stats) = (resourcePath, prefabName, stats);

    public GameObject GetModelPrefab ()
    {
        string path = ResourcePath + "/" + PrefabName;
        return Resources.Load<GameObject>(path);
    }
}

public class CharacterStats
{
    public CharacterStats(string name, int attack, int defence, int speed)
    {
        Name = name;
        Attack = Mathf.Clamp(attack, 1, 5);
        Defence = Mathf.Clamp(defence, 1, 5);
        Speed = Mathf.Clamp(speed, 1, 5);
    }

    /**
     * A character's displayed name
     */
    public string Name { get; private set; }

    /**
     * Value between 1 and 5 giving the relative strength of the character
     */
    public int Attack { get; private set; }

    /**
     * Value between 1 and 5 giving the relative tankiness of the character
     */
    public int Defence { get; private set; }

    /**
     * Value between 1 and 5 giving the relative attack speed of the character
     */
    public int Speed { get; private set; }
}