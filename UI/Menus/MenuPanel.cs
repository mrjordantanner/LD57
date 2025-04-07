using UnityEngine;
using System.Linq;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;


[RequireComponent(typeof(CanvasGroup))]
public class MenuPanel : MonoBehaviour
{
    [HideInInspector]
    public CanvasGroup canvasGroup;
    public MenuPanel[] childPanelsToShow;
    public MenuPanel[] childPanelsToHide;
    [ReadOnly]
    public bool isShowing;

    void Start()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
    }


    public virtual void Hide(float fadeDuration = 0f)
    {
        StartCoroutine(HidePanel(fadeDuration));

        if (childPanelsToHide.Length > 0)
        {
            foreach (var panel in childPanelsToHide)
            {
                StartCoroutine(panel.HidePanel(fadeDuration));
            }
        }
    }

    public virtual void Show(float fadeDuration = 0f, bool setActivePanel = true)
    {
        StartCoroutine(ShowPanel(fadeDuration, setActivePanel));

        if (childPanelsToShow.Length > 0)
        {
            foreach (var panel in childPanelsToShow)
            {
                StartCoroutine(panel.ShowPanel(fadeDuration, setActivePanel));
            }
        }
    }

    IEnumerator ShowPanel(float fadeDuration, bool setActivePanel = true)
    {
        isShowing = true;
        if (setActivePanel) Menu.Instance.ActiveMenuPanel = this;
        Tween fadeIn = canvasGroup.DOFade(1f, fadeDuration).SetUpdate(UpdateType.Normal, true);
        yield return new WaitForSecondsRealtime(fadeDuration);

        fadeIn.Kill();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    IEnumerator HidePanel(float fadeDuration)
    {
        isShowing = false;
        Tween fadeOut = canvasGroup.DOFade(0f, fadeDuration).SetUpdate(UpdateType.Normal, true);
        yield return new WaitForSecondsRealtime(fadeDuration);

        fadeOut.Kill();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

}
