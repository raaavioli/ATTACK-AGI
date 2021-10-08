using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardUI : MonoBehaviour {
    [SerializeField]
    private Animator[] T1CardAnimators;
    [SerializeField]
    private Animator[] T2CardAnimators;

    private bool[] T1PresentCards = new bool[6];
    private bool[] T2PresentCards = new bool[6];

    void Start() {
        ServerHandler.onCardDataReceived += UpdateAnimations;
    }

    private void UpdateAnimations() {
        ServerHandler.CardPosition[] cardPositions = ServerHandler.cardInformation;

        bool[] T1Found = new bool[6];
        bool[] T2Found = new bool[6];

        for (int i = 0; i < 6; ++i) {
            foreach (ServerHandler.CardPosition cardPosition in cardPositions) {
                Team team = cardPosition.team;
                if (i == cardPosition.position - 1) {
                    if (team == Team.Left) {
                        T1Found[i] = true;
                        if (!T1PresentCards[i]) {
                            T1CardAnimators[i].SetTrigger("FadeIn");
                            T1PresentCards[i] = true;
                        }
                    } else {
                        T2Found[i] = true;
                        if (!T2PresentCards[i]) {
                            T2CardAnimators[i].SetTrigger("FadeIn");
                            T2PresentCards[i] = true;
                        }
                    }
                }
            }

            if (!T1Found[i] && T1PresentCards[i]) {
                T1CardAnimators[i].SetTrigger("FadeOut");
                T1PresentCards[i] = false;
            }
            if (!T2Found[i] && T2PresentCards[i]) {
                T2CardAnimators[i].SetTrigger("FadeOut");
                T2PresentCards[i] = false;
            }
        }
    }
}