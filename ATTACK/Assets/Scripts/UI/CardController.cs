using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour {
    [SerializeField]
    private CardAnimator[] T1CardAnimators;
    [SerializeField]
    private CardAnimator[] T2CardAnimators;

    public GameObject setupTimer;
    public GameObject roundWinnerText;

    void Start() {
        foreach (CardAnimator updater in T1CardAnimators) {
            updater.transform.Find("HealthBar").gameObject.SetActive(false);
        }

        foreach (CardAnimator updater in T2CardAnimators) {
            updater.transform.Find("HealthBar").gameObject.SetActive(false);
        }
    }

    public void EnableHealthBar(bool enable, Team team, int index)
    {
        if (team == Team.One)
            T1CardAnimators[index].transform.Find("HealthBar").gameObject.SetActive(enable);
        else
            T2CardAnimators[index].transform.Find("HealthBar").gameObject.SetActive(enable);
    }

    void Update()
    {
        for (int i = 0; i < CardManager.MAX_CARDS_PER_TEAM; ++i)
        {
            foreach (Team team in System.Enum.GetValues(typeof(Team)))
            {
                CardAnimator[] Animators = team == Team.One ? T1CardAnimators : T2CardAnimators;
                bool hasCard = (i % 3) != 0 || CardManager.HasCard(team, i);
                bool isRotated = (i % 3) == 1 || CardManager.IsRotated(team, i);
                Animators[i]._Rotating = !hasCard;
                Animators[i]._Color = !hasCard ? Color.white : isRotated ? Colors.BrightBlue : Colors.BrightOrange;
            }
        }
          
    }
}
