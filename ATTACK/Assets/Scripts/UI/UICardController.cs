using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICardController : MonoBehaviour {
    [SerializeField]
    private UICardAnimator[] T1CardAnimators;
    [SerializeField]
    private UICardAnimator[] T2CardAnimators;

    public GameObject setupTimer;
    public GameObject roundWinnerText;

    public void SetStats(CharacterStats stats, Team team, int index)
    {
        if (index < 0 || index  >= CardManager.MAX_CARDS_PER_TEAM)
        {
            Debug.LogError("Index out of bounds when setting stats for team: " + team);
            return;
        }

        UICardAnimator[] animators = team == Team.One ? T1CardAnimators : T2CardAnimators;
        animators[index].SetStats(stats);
    }

    public void SetHealth(float health, Team team, int index)
    {
        if (index < 0 || index >= CardManager.MAX_CARDS_PER_TEAM)
        {
            Debug.LogError("Index out of bounds when setting stats for team: " + team);
            return;
        }
        UICardAnimator[] animators = team == Team.One ? T1CardAnimators : T2CardAnimators;
        animators[index].SetHealth(health);
    }

    void Update()
    {
        for (int i = 0; i < CardManager.MAX_CARDS_PER_TEAM; ++i)
        {
            foreach (Team team in System.Enum.GetValues(typeof(Team)))
            {
                UICardAnimator[] Animators = team == Team.One ? T1CardAnimators : T2CardAnimators;
                bool hasCard = (i % 3) != 0 || CardManager.HasCard(team, i);
                bool isRotated = (i % 3) == 1 || CardManager.IsRotated(team, i);
                Animators[i]._Rotating = !hasCard;
                Animators[i]._Color = !hasCard ? Color.white : isRotated ? Colors.DefenceColor : Colors.AttackColor;
            }
        }
          
    }
}
