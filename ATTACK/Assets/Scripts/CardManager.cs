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

	public override int GetHashCode() {
		return base.GetHashCode();
	}

	public override string ToString()
    {
        return $"position: {position}, rank: {rank}, rotated: {rotated}";
    }
}
public class CardManager : MonoBehaviour
{
    private static CardManager Instance;

    [SerializeField]
    private const int MAX_BUFFERING = 10;

    [HideInInspector]
    public const int MAX_CARDS_PER_TEAM = 5;

    private Card[][] teamCards = new Card[2][];
    private Card[][] teamSavedCardInfo = new Card[2][];

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
            return Instance.teamCards[(int) team][index] != Card.INVALID;
    }

    public static Card GetCard(Team team, int index)
    {
        return Instance.teamCards[(int) team][index];
    }

    public static bool IsRotated(Team team, int index)
    {
        return Instance.teamCards[(int) team][index].rotated;
    }

    private void UpdateCards() {
        // If there is no new info, do not update cards.
        if (!ServerHandler.updatedCardInfo) {
            return;
		}

        string cardInfo = ServerHandler.mostRecentCardInfo;
        Card[][] cardsFromCardInfo = new Card[][] {
            new Card[MAX_CARDS_PER_TEAM],
            new Card[MAX_CARDS_PER_TEAM]
        };

        // Set all found cards to INVALID.
        for (int team = 0; team < 2; ++team) {
            for (int position = 0; position < MAX_CARDS_PER_TEAM; ++position) {
                cardsFromCardInfo[team][position] = Card.INVALID;
            }
        }

        // Parse the cards from the card info.
        if (cardInfo != null) {
            string[] cardStrings = cardInfo.Split(',');

            foreach (string cardString in cardStrings) {
                string[] parts = cardString.Split(':');
                if (parts.Length != 4) {
                    continue;
                }

                int team = int.Parse(parts[0]);
                int position = int.Parse(parts[1]);
                cardsFromCardInfo
                    [team - 1]
                    [position] = 
                    new Card(position, int.Parse(parts[2]), int.Parse(parts[3]) > 0);
            }
        }

        // Compare the parsed server card info to the saved info, and maybe update cards.
        for (int team = 0; team < 2; ++team) {
            for (int position = 0; position < MAX_CARDS_PER_TEAM; ++position) {
                Card currentCard = cardsFromCardInfo[team][position];

                // Otherwise, compare the found card to the saved card.
                if (teamSavedCardInfo[team][position] == Card.INVALID) {
                    // if saved position info is INVALID, save new info to position and set position counter to MAX, set team card
                    teamSavedCardInfo[team][position] = currentCard;
                    teamCounters[team][position] = MAX_BUFFERING;
                    teamCards[team][position] = currentCard;
                } else if (teamSavedCardInfo[team][position] == currentCard) {
                    // if saved position info MATCHES new info, increment position counter, clamped to MAX
                    int teamCounter = teamCounters[team][position];
                    teamCounters[team][position] = Mathf.Clamp(teamCounter + 1, 0, MAX_BUFFERING);
                    teamSavedCardInfo[team][position] = currentCard;
                    teamCards[team][position] = currentCard;
                } else {
                    // if saved position info DOES NOT MATCH new info, decrement position counter
                    teamCounters[team][position]--;
                }

                if (teamCounters[team][position] <= 0) {
                    // if position counter is now 0, set saved position info to INVALID, set team card to INVALID
                    teamCounters[team][position] = 0;
                    teamCards[team][position] = Card.INVALID;
                    teamSavedCardInfo[team][position] = Card.INVALID;
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
