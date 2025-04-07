using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TitleScreen : MenuPanel
{
    //public TextMeshProUGUI pressKeyToStartLabel;

    private void Update()
    {
        if (!isShowing) return;

        //if (Utils.ClickOrTap())
        //{

        //    // TODO play start sound

        //    //pressKeyToStartLabel.gameObject.SetActive(false);
        //    StartCoroutine(GameManager.Instance.InitializeNewRun());
            
        //}
    }

    public void StartTitleSequence()
    {
        Show();
        AudioManager.Instance.FadeMusicIn(AudioManager.Instance.gameplayMusic);
    }


}
