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

    private Card[] T1Cards;
    private Card[] T2Cards;

    private Card[] T1SavedCardInfo;
    private Card[] T2SavedCardInfo;

    private int[] T1Counters = new int[MAX_CARDS_PER_TEAM];
    private int[] T2Counters = new int[MAX_CARDS_PER_TEAM];

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        T1Cards = new Card[MAX_CARDS_PER_TEAM];
        T1SavedCardInfo = new Card[MAX_CARDS_PER_TEAM];
        for (int i = 0; i < T1Cards.Length; i++) {
            T1Cards[i] = Card.INVALID;
            T1SavedCardInfo[i] = Card.INVALID;
        }

        T2Cards = new Card[MAX_CARDS_PER_TEAM];
        T2SavedCardInfo = new Card[MAX_CARDS_PER_TEAM];
        for (int i = 0; i < T2Cards.Length; i++) {
            T2Cards[i] = Card.INVALID;
            T2SavedCardInfo[i] = Card.INVALID;
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
        else if (team == 0)
            return !Equals(Instance.T1Cards[index], Card.INVALID);
        else
            return !Equals(Instance.T2Cards[index], Card.INVALID);
    }

    public static Card GetCard(Team team, int index)
    {
        if (team == Team.One)
            return Instance.T1Cards[index];
        return Instance.T2Cards[index];
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
                        if (team.AsTeam() == Team.One)
                        {
                            if (T1SavedCardInfo[i] == Card.INVALID) {
                                // if saved position info is INVALID, save new info to position and set position counter to MAX, set team card
                                T1Counters[i] = MAX_BUFFERING;
                                T1CardValid = true;
                                T1Cards[i] = card;
                            } else if (T1SavedCardInfo[i] == card) {
                                // if saved position info MATCHES new info, increment position counter, clamped to MAX
                                int T1Counter = T1Counters[i];
                                T1Counters[i] = Mathf.Clamp(T1Counter + 1, 0, MAX_BUFFERING);
                                T1CardValid = true;
                            }
                        }
                        else
                        {
                            if (T2SavedCardInfo[i] == Card.INVALID) {
                                // if saved position info is INVALID, save new info to position and set position counter to MAX, set team card
                                T2Counters[i] = MAX_BUFFERING;
                                T2CardValid = true;
                                T2Cards[i] = card;
                            } else if (T2SavedCardInfo[i] == card) {
                                // if saved position info MATCHES new info, increment position counter, clamped to MAX
                                int T2Counter = T2Counters[i];
                                T2Counters[i] = Mathf.Clamp(T2Counter + 1, 0, MAX_BUFFERING);
                                T2CardValid = true;
                            }
                        }
                    }
                }
                if (!T1CardValid) {
                    // if saved position info DOES NOT MATCH new info, decrement position counter
                    T1Counters[i]--;

                    if (T1Counters[i] == 0) {
                        // if position counter is now 0, set saved position info to INVALID, set team card to INVALID
                        T1Counters[i] = 0;
                        T1Cards[i] = Card.INVALID;
                        T1SavedCardInfo[i] = Card.INVALID;
                    }
                }
                    
                if (!T2CardValid) {
                    // if saved position info DOES NOT MATCH new info, decrement position counter
                    T2Counters[i]--;

                    if (T2Counters[i] == 0) {
                        // if position counter is now 0, set saved position info to INVALID, set team card to INVALID
                        T2Counters[i] = 0;
                        T2Cards[i] = Card.INVALID;
                        T2SavedCardInfo[i] = Card.INVALID;
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
