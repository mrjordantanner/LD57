using UnityEngine;
using System.Linq;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;


public class NameEntryPanel : MenuPanel
{
    public TMP_InputField nameInputField;
    [ReadOnly] public bool nameAlreadyConfirmed;

    private void Update()
    {
        if (!isShowing) return;

        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            if (nameAlreadyConfirmed || string.IsNullOrEmpty(nameInputField.text)) return;

            nameAlreadyConfirmed = true;
            Menu.Instance.ConfirmNameEntry();
        }
    }


    public override void Show(float fadeDuration = 0.2f, bool setActivePanel = true)
    {
        //nameInputField.onEndEdit.AddListener(OnInputEndEdit);
        nameInputField.characterLimit = 10;

        if (PlayerData.Instance.Data.PlayerName != PlayerData.Instance.Data.defaultPlayerName)
        {
            nameInputField.text = PlayerData.Instance.Data.PlayerName;
        } 
        else
        {
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }

        base.Show(fadeDuration);
    }

    //public void OnInputEndEdit(string playerName)
    //{
    //    //if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
    //    //{
    //    //    Menu.Instance.ConfirmNameEntry();
    //    //}
    //}

    public override void Hide(float fadeDuration = 0.1f)
    {
        base.Hide(fadeDuration);

    }

}
