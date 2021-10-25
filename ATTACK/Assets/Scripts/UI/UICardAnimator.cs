using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICardAnimator : MonoBehaviour
{
    [Range(0.1f, 10)]
    [SerializeField]
    private float Period = 1.0f;

    [Range(0f, 1f)]
    [SerializeField]
    private float StartRotation = 0.0f;

    private Color DefaultMaterialColor;
    public Color _Color;
    public bool _Rotating;

    private UIVisibility visibility;

    Material material;
    Material attackRankMaterial;
    Material defenceRankMaterial;
    Material speedRankMaterial;

    void Start()
    {
        material = Instantiate(GetComponent<Image>().material);
        GetComponent<Image>().material = material;
        DefaultMaterialColor = material.GetColor("_Color");
        _Color = DefaultMaterialColor;

        Transform statsUI = transform.Find("Stats");
        attackRankMaterial = Instantiate(statsUI.Find("Attack").GetComponentInChildren<Image>().material);
        attackRankMaterial.SetColor("_Color", Colors.AttackColor);
        statsUI.Find("Attack").GetComponentInChildren<Image>().material = attackRankMaterial;
        defenceRankMaterial = Instantiate(statsUI.Find("Defence").GetComponentInChildren<Image>().material);
        defenceRankMaterial.SetColor("_Color", Colors.DefenceColor);
        statsUI.Find("Defence").GetComponentInChildren<Image>().material = defenceRankMaterial;
        speedRankMaterial = Instantiate(statsUI.Find("Speed").GetComponentInChildren<Image>().material);
        speedRankMaterial.SetColor("_Color", Colors.SpeedColor);
        statsUI.Find("Speed").GetComponentInChildren<Image>().material = speedRankMaterial;

        // Default hide all stats, from All to None, at start.
        visibility = UIVisibility.All;
        SetVisible(UIVisibility.None);
    }

    void Update()
    {
        float Ang01 = StartRotation + (Time.time / Period);
        Ang01 -= (int)Ang01;
        material.SetFloat("_Angle", Ang01);
        material.SetColor("_Color", _Color);
        material.SetFloat("_Rotating", _Rotating ? 1.0f : 0.0f);
    }

    public void SetVisible(UIVisibility visibility)
    {
        if (this.visibility == visibility)
            return;

        this.visibility = visibility;
        switch (visibility)
        {
            case UIVisibility.All:
            {
                SetVisible(true, true, 210f, 180f);
                break;
            };
            case UIVisibility.Reduced:
            {
                SetVisible(true, false, 160f, 75f);
                break;
            };
            case UIVisibility.None:
            {
                SetVisible(false, false, 0, 0);
                break;
            };
            default:
            {
                Debug.LogError("Unknown UIVisibility type: " + visibility);
                break;
            }
        }
    }

    private void SetVisible(bool statsUI, bool substatsUI, float x, float width)
    {
        GameObject stats = transform.Find("Stats").gameObject;
        stats.SetActive(statsUI);
        if (!statsUI) // Early return if parent object is not active
            return;
        stats.transform.Find("Attack").gameObject.SetActive(substatsUI);
        stats.transform.Find("Defence").gameObject.SetActive(substatsUI);
        stats.transform.Find("Speed").gameObject.SetActive(substatsUI);
        RectTransform rectTransform = stats.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector3(Mathf.Sign(rectTransform.anchoredPosition.x) * x, 0f, 0f);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, width);
    }

    public void SetHealth(float Health)
    {
        Transform statsUI = transform.Find("Stats");
        statsUI.GetComponentInChildren<UIHealth>().SetHealth(Health);
    }

    public void SetStats(CharacterStats stats)
    {
        Transform statsUI = transform.Find("Stats");
        statsUI.Find("Name").GetComponent<Text>().text = stats.Name;
        attackRankMaterial.SetInt("_Rank", stats.Attack);
        defenceRankMaterial.SetInt("_Rank", stats.Defence);
        speedRankMaterial.SetInt("_Rank", stats.Speed);
    }

}

public enum UIVisibility
{
    All,
    Reduced,
    None
}
