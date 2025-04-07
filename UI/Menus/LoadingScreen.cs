using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MenuPanel
{
    public Slider progressSlider;
    public TextMeshProUGUI progressLabel, currentWorkItemLabel;

    public Image loadingImage;
    public float imageRotationSpeed = 1f;

    private void Update()
    {
        if (Loader.Instance.isLoading)
        {
            loadingImage.transform.Rotate(Vector3.up, imageRotationSpeed * Time.deltaTime);
        }
    }

    public void UpdateProgress(float progressValue, string currentWorkItem)
    {
        currentWorkItemLabel.text = currentWorkItem;
        progressSlider.value = progressValue;
        progressLabel.text = $"{(int)(progressValue * 100)}%";
    }


}
