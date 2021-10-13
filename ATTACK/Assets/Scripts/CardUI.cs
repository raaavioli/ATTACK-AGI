using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour {
    [SerializeField]
    private Animator[] T1CardAnimators;
    [SerializeField]
    private Animator[] T2CardAnimators;

    public GameObject setupTimer;
    public GameObject roundWinnerText;

    void Start() {
        foreach (Animator animator in T1CardAnimators) {
            animator.transform.Find("HealthBar").gameObject.SetActive(false);
        }

        foreach (Animator animator in T2CardAnimators) {
            animator.transform.Find("HealthBar").gameObject.SetActive(false);
        }
    }

    public void EnableHealthBar(bool enable, Team team, int index)
    {
        if (team == Team.One)
            T1CardAnimators[index].transform.Find("HealthBar").gameObject.SetActive(enable);
        else
            T1CardAnimators[index].transform.Find("HealthBar").gameObject.SetActive(enable);
    }

    void Update()
    {
        for (int i = 0; i < CardManager.MAX_CARDS_PER_TEAM; ++i)
        {
            if (CardManager.HasCard(Team.One, i))
                T1CardAnimators[i].SetTrigger("FadeIn");
            else 
                T1CardAnimators[i].SetTrigger("FadeOut");
            if (CardManager.HasCard(Team.Two, i))
                T2CardAnimators[i].SetTrigger("FadeIn");
            else
                T2CardAnimators[i].SetTrigger("FadeOut");
        }
          
    }
}
