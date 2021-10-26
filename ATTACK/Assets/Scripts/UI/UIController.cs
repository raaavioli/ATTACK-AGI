using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    [SerializeField]
    private UICardAnimator[] T1CardAnimators;
    [SerializeField]
    private UICardAnimator[] T2CardAnimators;

    [SerializeField]
    private Text setupTimer;
    [SerializeField]
    private Text t1Score;
    [SerializeField]
    private Text t2Score;
    [SerializeField]
    private Text roundCounter;
    [SerializeField]
    private Text roundWinner;

    void Update()
    {
        for (int i = 0; i < CardManager.MAX_CARDS_PER_TEAM; ++i)
        {
            foreach (Team team in System.Enum.GetValues(typeof(Team)))
            {
                UICardAnimator[] Animators = team == Team.One ? T1CardAnimators : T2CardAnimators;
                bool hasCard = CardManager.HasCard(team, i);
                bool isRotated = CardManager.IsRotated(team, i);
                Animators[i]._Rotating = !hasCard;
                Animators[i]._Color = !hasCard ? Color.white : isRotated ? Colors.DefenceColor : Colors.AttackColor;
            }
        }
    }

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

    public void SetStatsVisible(UIVisibility visibility, Team team, int position)
    {
        UICardAnimator[] animators = team == Team.One ? T1CardAnimators : T2CardAnimators;
        animators[position].SetVisible(visibility);
    }

    public void ShowScoreBoard(bool show)
    {
        transform.Find("ScoreHUD").gameObject.SetActive(show);
    }

    public void SetRoundWinnerText(string roundWinnerText)
    {
        roundWinner.text = roundWinnerText;
    }

    public void SetTimer(int secondsLeft)
    {
        setupTimer.text = secondsLeft.ToString();
        float colorShiftTime = 5.0f;
        if (secondsLeft < colorShiftTime)
            setupTimer.color = Color.Lerp(Color.red, Color.white, secondsLeft / (colorShiftTime - 1));
        if (secondsLeft == 0)
            setupTimer.text = "";
    }

    public void SetRound(int round)
    {
        roundCounter.text = round.ToString();
    }

    public void SetScore(int t1Score, int t2Score)
    {
        this.t1Score.text = t1Score.ToString();
        this.t2Score.text = t2Score.ToString();
    }
}
