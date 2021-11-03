using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxScript : MonoBehaviour
{
    [SerializeField]
    string[] titles = new string[6];
    [SerializeField]
    string[] contents = new string[6];
    [SerializeField]
    GameObject title;
    [SerializeField]
    GameObject content;

    int idx;
    // Start is called before the first frame update
    void Start()
    {
        idx = 0;
        SetTexts();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextPage() {
        idx++;
        idx = idx % titles.Length;
        SetTexts();
    }

    public void PrevPage() {
        idx--;
        if (idx < 0)
            idx = titles.Length-1;
        SetTexts();
    }

    void SetTexts() {
        title.GetComponent<Text>().text = titles[idx];
        content.GetComponent<Text>().text = contents[idx].Replace("<br>", "\n");
    }
}
