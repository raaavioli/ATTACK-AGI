
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TouchScript.Gestures;

public class TapToHealTouchScript : MonoBehaviour {
    TouchPanelScript Parent;
    Team side;

    TapGesture tap;
    bool tappedFirst;
    Healing healing;

    bool DebugMode;
    Camera camera;
    GameObject platform;
    RectTransform rect;

    public void Setup(TouchPanelScript parent, GameObject ts, Camera cam, bool debug, int healAmount, Team team) {
        Parent = parent;
        side = team;

        platform = ts;
        camera = cam;
        DebugMode = debug;

        tappedFirst = false;
        healing = GetComponent<Healing>();
        healing.Damage = -healAmount;

        rect = GetComponent<RectTransform>();
        Vector3 targPos = platform.transform.position;
        Vector3 camForward = camera.transform.forward;
        Vector3 camPos = camera.transform.position + camForward;
        rect.position = RectTransformUtility.WorldToScreenPoint(camera, targPos);

        //Sets the size of the rects to be a good size relative to the screen resolution. Originals were 0.12 and 0.21.
        rect.sizeDelta = new Vector2(0.06f*Screen.width, 0.105f*Screen.height);
        //Shows the hitboxes if desired, for debug purposes.
        if (DebugMode)
            GetComponent<Image>().color = new Vector4(1, 1, 1, 0.4f);
    }

    // Update is called once per frame
    void Update() 
    {
        
    }

    private void OnEnable() {
        GetComponent<TapGesture>().Tapped += TapHandler;
    }

    private void OnDisable() {
        GetComponent<TapGesture>().Tapped -= TapHandler;
    }

    private void TapHandler(object sender, EventArgs eventArgs) {
        if (tappedFirst)
        {
            //*Incredibly* ugly, I know, but it works and there's not much time left. GetChild(5) *should* give the CharacterCommon of the spawn point.
            if(platform.transform.GetChild(5) != null && Parent.AuthorizeHeal(side))
                healing.StartAttack(new List<CharacterCommon> { platform.transform.GetChild(5).gameObject.GetComponent<CharacterCommon>() });
        }
        else
        {
            tappedFirst = true;
            StartCoroutine(WaitForSecondTap());
        }
    }

    private IEnumerator WaitForSecondTap()
    {   
        yield return new WaitForSeconds(0.4f);
        tappedFirst = false;
    }
}
