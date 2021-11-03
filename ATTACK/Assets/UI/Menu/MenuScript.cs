using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    [SerializeField]
    GameObject mainMenu;
    [SerializeField]
    GameObject instructions;
    [SerializeField]
    GameObject inGameMenu;
    [SerializeField]
    GameObject inGameButtons;
    [SerializeField]
    GameObject continueButton;
    [SerializeField]
    GameObject combatTouchPanels;
    [SerializeField]
    GameManager gameManager;
    Color previousBackgroundColor;
    GameState state;

    private void FadeBackgroundSwitch()
    {
        Color temp = previousBackgroundColor;
        previousBackgroundColor = gameObject.GetComponent<Image>().color;
        gameObject.GetComponent<Image>().color = temp;
    }

    public void Start()
    {
        instructions.SetActive(false);
        inGameMenu.SetActive(false);
        inGameButtons.SetActive(false);
        continueButton.SetActive(false);
        combatTouchPanels.SetActive(false);
        previousBackgroundColor = new Color(0, 0, 0, 0);
        state = gameManager.GetGameState();
    }

    public void Update()
    {
        GameState newState = gameManager.GetGameState();
        if (newState != state)
        {
            switch (newState)
            {
                case GameState.Setup:
                    if (state == GameState.MainMenu)
                        HideMainMenu();
                    else if (state == GameState.GameMenu)
                        HideInGameMenu();
                    ShowInGameButtons();
                    HideContinueButton();
                    break;
                case GameState.Combat:
                    ShowCombatTouchPanels();
                    HideInGameButtons();
                    break;
                case GameState.RoundOver:
                    HideCombatTouchPanels();
                    ShowContinueButton();
                    break;
                case GameState.GameOver:
                    HideCombatTouchPanels();
                    ShowContinueButton();
                    break;
                case GameState.MainMenu:
                    break;
                case GameState.GameMenu:
                    ShowInGameMenu();
                    HideInGameButtons();
                    break;
                default:
                    break;
            }
            state = newState;
        }
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

    public void ShowContinueButton()
    {
        continueButton.SetActive(true);
    }

    public void HideContinueButton()
    {
        continueButton.SetActive(false);
    }

    public void ShowCombatTouchPanels()
    {
        combatTouchPanels.SetActive(true);
        combatTouchPanels.GetComponent<TouchPanelScript>().ResetAmounts();
    }

    public void HideCombatTouchPanels()
    {
        combatTouchPanels.SetActive(false);
    }
}
