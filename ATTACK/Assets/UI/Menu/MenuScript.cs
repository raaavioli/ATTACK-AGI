using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    GameObject mainMenu;
    GameObject instructions;
    GameObject inGameMenu;
    GameObject inGameButtons;
    Color previousBackgroundColor;

    private void FadeBackgroundSwitch()
    {
        Color temp = previousBackgroundColor;
        previousBackgroundColor = gameObject.GetComponent<Image>().color;
        gameObject.GetComponent<Image>().color = temp;
    }

    public void Start()
    {
        mainMenu = transform.GetChild(0).gameObject;
        instructions = transform.GetChild(1).gameObject;
        inGameMenu = transform.GetChild(2).gameObject;
        inGameButtons = transform.GetChild(3).gameObject;
        instructions.SetActive(false);
        inGameMenu.SetActive(false);
        inGameButtons.SetActive(false);
        previousBackgroundColor = new Color(0, 0, 0, 0);
    }

    public void HideMainMenu()
    {
        FadeBackgroundSwitch();
        mainMenu.SetActive(false);
    }

    public void ShowMainMenu()
    {
        FadeBackgroundSwitch();
        mainMenu.SetActive(true);
    }

    public void ShowInstructions()
    {
        instructions.SetActive(true);
    }

    public void HideInstructions()
    {
        instructions.SetActive(false);
    }

    public void ShowInGameMenu()
    {
        FadeBackgroundSwitch();
        inGameMenu.SetActive(true);
    }

    public void HideInGameMenu()
    {
        FadeBackgroundSwitch();
        inGameMenu.SetActive(false);
    }

    public void ShowInGameButtons()
    {
        inGameButtons.SetActive(true);
    }

    public void HideInGameButtons()
    {
        inGameButtons.SetActive(false);
    }
}
