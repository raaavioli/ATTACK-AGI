using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Card
{
    public int position;
    public int rank;
    public bool rotated;

    public static Card INVALID = new Card(-1, -1, false);

    public Card(int position, int rank, bool rotated)
    {
        this.position = position;
        this.rank = rank;
        this.rotated = rotated;
    }

    public override bool Equals(object obj) => obj is Card other && this.Equals(other);

    public bool Equals(Card c) => position == c.position && rank == c.rank;

    public static bool operator ==(Card lhs, Card rhs) => lhs.Equals(rhs);

    public static bool operator !=(Card lhs, Card rhs) => !(lhs == rhs);

    public override string ToString()
    {
        return $"position: {position}, rank: {rank}, rotated: {rotated}";
    }
}
public class CardManager : MonoBehaviour
{
    private static CardManager Instance;

    [SerializeField]
    private const int MAX_BUFFERING = 5;

    [HideInInspector]
    public const int MAX_CARDS_PER_TEAM = 5;

    private Card[][] teamCards;
    private Card[][] teamSavedCardInfo;

    private int[][] teamCounters = new int[][] {
        new int[MAX_CARDS_PER_TEAM],
        new int[MAX_CARDS_PER_TEAM]
    };

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        for (int team = 0; team < 2; ++team) {
            teamCards[team] = new Card[MAX_CARDS_PER_TEAM];
            teamSavedCardInfo[team] = new Card[MAX_CARDS_PER_TEAM];
            for (int i = 0; i < MAX_CARDS_PER_TEAM; ++i) {
                teamCards[team][i] = Card.INVALID;
                teamSavedCardInfo[team][i] = Card.INVALID;
			}
        }
    }

    void Update()
    {
        UpdateCards();
    }

    public static bool HasCard(Team team, int index)
    {
        if (index < 0 || index >= MAX_CARDS_PER_TEAM)
            return false;
        else
            return !Equals(Instance.teamCards[(int) team][index], Card.INVALID);
    }

    public static Card GetCard(Team team, int index)
    {
        return Instance.teamCards[(int) team][index];
    }

    public static bool IsRotated(Team team, int index)
    {
        if (team == Team.One)
            return Instance.T1Cards[index].rotated;
        return Instance.T2Cards[index].rotated;
    }

    private void UpdateCards()
    {
        string cardInfo = ServerHandler.mostRecentCardInfo;
        if (cardInfo != null)
        {
            string[] cardStrings = cardInfo.Split(',');
            for (int i = 0; i < MAX_CARDS_PER_TEAM; i++)
            {
                bool T1CardValid = false;
                bool T2CardValid = false;

                bool[] teamCardValid = new bool[2];

                for (int j = 0; j < cardStrings.Length; j++)
                {
                    string cardString = cardStrings[j];
                    string[] parts = cardString.Split(':');
                    if (parts.Length < 4)
                        continue;
                    int position = int.Parse(parts[1]);
                    if (position == i)
                    {
                        Card card = new Card(position, int.Parse(parts[2]), int.Parse(parts[3]) > 0);
                        int team = int.Parse(parts[0]) - 1;

                        if (teamSavedCardInfo[team][i] == Card.INVALID) {
                            // if saved position info is INVALID, save new info to position and set position counter to MAX, set team card
                            teamCounters[team][i] = MAX_BUFFERING;
                            teamCardValid[team] = true;
                            teamCards[team][i] = card;
                        } else if (teamSavedCardInfo[team][i] == card) {
                            // if saved position info MATCHES new info, increment position counter, clamped to MAX
                            int teamCounter = teamCounters[team][i];
                            teamCounters[team][i] = Mathf.Clamp(teamCounter + 1, 0, MAX_BUFFERING);
                            teamCardValid[team] = true;
                        }
                    }
                }

                for (int team = 0; team < 2; ++team) {
                    if (!teamCardValid[team]) {
                        // if saved position info DOES NOT MATCH new info, decrement position counter
                        teamCounters[team][i]--;

                        if (teamCounters[team][i] == 0) {
                            // if position counter is now 0, set saved position info to INVALID, set team card to INVALID
                            teamCounters[team][i] = 0;
                            teamCards[team][i] = Card.INVALID;
                            teamSavedCardInfo[team][i] = Card.INVALID;
                        }
                    }
                }
            }
        }
    }

    /* Scheme for jitter protection:
     * 
     * for a card in position
     * 
     * if saved position info is INVALID, save new info to position and set position counter to MAX, set team card
     * if saved position info MATCHES new info, increment position counter, clamped to MAX
     * if saved position info DOES NOT MATCH new info, decrement position counter
     * 
     * if position counter is now 0, set saved position info to INVALID, set team card to INVALID
     */
}
