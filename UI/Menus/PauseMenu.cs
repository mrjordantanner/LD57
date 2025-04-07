using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PauseMenu : MenuPanel
{
    #region Singleton
    public static PauseMenu Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion


    public override void Show(float fadeDuration = 0.1f, bool setActivePanel = true)
    {
        RefreshAll();
        base.Show();
    }

    public void RefreshAll()
    {

    }

}
