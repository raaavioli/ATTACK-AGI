using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TouchScript.Gestures;

public class TouchPanelScript : MonoBehaviour
{
    [SerializeField]
    bool DebugMode;
    [SerializeField]
    Camera camera;
    [SerializeField]
    GameObject[] platforms = new GameObject[10];
    [SerializeField]
    GameObject[] touchPanels = new GameObject[10];
    [SerializeField]
    int healAmount;
    [SerializeField]
    int amountOfHeals;

    int T1Amount;
    int T2Amount;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < touchPanels.Length/2; i++)
        {
            touchPanels[i].GetComponent<TapToHealTouchScript>().Setup(this, platforms[i], camera, DebugMode, healAmount, Team.One);
        }

        for (int i = 5; i < touchPanels.Length; i++)
        {
            touchPanels[i].GetComponent<TapToHealTouchScript>().Setup(this, platforms[i], camera, DebugMode, healAmount, Team.Two);
        }
        
        T1Amount = 0;
        T2Amount = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool AuthorizeHeal(Team team)
    {
        if(team == Team.One && T1Amount < amountOfHeals)
        {
            T1Amount++;
            return true;
        }
        if (team == Team.Two && T2Amount < amountOfHeals)
        {
            T2Amount++;
            return true;
        }
        return false;
    }

    public void ResetAmounts()
    {
        T1Amount = 0;
        T2Amount = 0;
    }
}
