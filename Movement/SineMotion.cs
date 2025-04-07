using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SineMotion : MonoBehaviour
{
    public bool isActive;

    [Header("Randomization")]
    public bool randomStartDelay = true;
    public float maxStartDelay = 1f;
    public bool randomDirection = true;

    [Header("Sine Motion")]
    public bool sineMotion;
    public float amplitude = 1f;
    public float frequency = 1f;
    public float speed = 1f;
    public float motionX = 0f;
    public float motionY = 0f;
    public float motionZ = 0f;

    [Header("Sine Scale")]
    public bool sineScale;
    public float scaleFrequency = 1f;
    public float scaleAmount = 0.2f;

    [Header("Sine Fade")]
    public bool sineFade;
    public float fadeFrequency = 1f;
    public float fadeAmount = 0.5f;

    [Header("Alpha Pulse")]
    public bool pulse;
    public float pulseFrequency = 1f;
    public float pulseAmount = 0.5f;

    private float startDelayTimer;
    private Vector3 initialLocalPosition;
    private MeshRenderer meshRenderer;

    private float scaleTimeCounter;
    private float fadeTimeCounter;
    private float pulseTimeCounter;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Init();
    }

    public void Init()
    {
        initialLocalPosition = transform.localPosition;

        if (randomDirection)
        {
            if (Random.value < 0.5f) motionX = -motionX;
            if (Random.value < 0.5f) motionY = -motionY;
            if (Random.value < 0.5f) motionZ = -motionZ;
        }

        if (randomStartDelay)
        {
            startDelayTimer = Random.Range(0, maxStartDelay);
            isActive = false;
        }
        else
        {
            isActive = true;
        }
    }

    private void Update()
    {
        if (!isActive)
        {
            startDelayTimer -= Time.deltaTime;
            if (startDelayTimer <= 0)
                isActive = true;
            else
                return;
        }

        float time = Time.time;

        // Sine Motion
        if (sineMotion)
        {
            Vector3 offset = new Vector3(
                Mathf.Sin(time * frequency) * motionX,
                Mathf.Sin(time * frequency) * motionY,
                Mathf.Sin(time * frequency) * motionZ
            );

            Vector3 targetPosition = initialLocalPosition + offset;
            Vector3 translation = targetPosition - transform.localPosition;
            transform.Translate(speed * Time.deltaTime * translation, Space.Self);
        }

        // Sine Scale
        if (sineScale)
        {
            scaleTimeCounter += Time.deltaTime * scaleFrequency;
            float scaleValue = 1 + Mathf.Sin(scaleTimeCounter) * scaleAmount;
            transform.localScale = Vector3.one * scaleValue;
        }

        // Fade
        if (sineFade && meshRenderer != null)
        {
            fadeTimeCounter += Time.deltaTime * fadeFrequency;
            float alpha = Mathf.Abs(Mathf.Sin(fadeTimeCounter)) * fadeAmount;

            Color color = meshRenderer.material.color;
            color.a = alpha;
            meshRenderer.material.color = color;
        }

        // Pulse
        if (pulse && meshRenderer != null)
        {
            pulseTimeCounter += Time.deltaTime * pulseFrequency;
            float alpha = Mathf.Abs(Mathf.Sin(pulseTimeCounter)) * pulseAmount;

            Color color = meshRenderer.material.color;
            color.a = alpha;
            meshRenderer.material.color = color;
        }
    }
}
