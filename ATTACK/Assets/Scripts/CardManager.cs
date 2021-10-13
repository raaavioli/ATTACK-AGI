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

    public override string ToString()
    {
        return $"position: {position}, rank: {rank}, rotated: {rotated}";
    }
}
public class CardManager : MonoBehaviour
{
    private static CardManager Instance;

    [HideInInspector]
    public const int MAX_CARDS_PER_TEAM = 5;

    private Card[] T1Cards;
    private Card[] T2Cards;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        T1Cards = new Card[MAX_CARDS_PER_TEAM];
        for (int i = 0; i < T1Cards.Length; i++) 
            T1Cards[i] = Card.INVALID;
        T2Cards = new Card[MAX_CARDS_PER_TEAM];
        for (int i = 0; i < T2Cards.Length; i++) 
            T2Cards[i] = Card.INVALID;
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
        for (int i = 0; i < T1Cards.Length; i++)
        {
            bool cardFound = false;
            string cardInfo = ServerHandler.mostRecentCardInfo;
            if (cardInfo != null)
            {
                string[] cardStrings = cardInfo.Split(',');
                for (int j = 0; j < cardStrings.Length; j++)
                {
                    string cardString = cardStrings[j];
                    string[] parts = cardString.Split(':');
                    int position = int.Parse(parts[1]);
                    if (position == i)
                    {
                        cardFound = true;
                        Card card = new Card(position, int.Parse(parts[2]), bool.Parse(parts[3]));
                        if (int.Parse(parts[0]).AsTeam() == Team.One)
                            T1Cards[i] = card;
                        else
                            T2Cards[i] = card;
                    }
                }
            }
            if (!cardFound)
                T1Cards[i] = Card.INVALID;
        }
    }

}
