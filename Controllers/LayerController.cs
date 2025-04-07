using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Linq;
using DG.Tweening;

public class LayerController : MonoBehaviour, IInitializable
{
    #region Singleton
    public static LayerController Instance;
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
        #endregion

        //StartCoroutine(Init());
    }

    public string Name { get { return "Layer Controller"; } }

    public Layer[] LayerBank;
    [HideInInspector] public Layer[] Layers;
    public Material defaultLayerMaterial;
    [HideInInspector] public Layer activeLayer;

    public int maxLayers = 5;
    public float cameraZDistance = -40;
    public float layerZDistance = -200;
    public Vector3 cameraStartingPosition = new Vector3(0, 0, 0);
    public Transform activeLayerAnchor;
    public GameObject LayersContainer;

    [ReadOnly]
    public bool isShifting;
    public float shiftDuration = 2f;
    public bool generateOnLoad = false;
    public int layersVisited;

    [Header("Color")]
    public Color baseColor = Color.white;


    public IEnumerator Init()
    {
        DestroyAllLayers();
        layersVisited = 1;

         LayerBank = Resources.LoadAll<Layer>("Layers");

        if (LayerBank.Length == 0)
        {
            Debug.LogError("No Layers were loaded.");
            yield break;
        }
        Debug.Log($"Loaded {LayerBank.Length} Layers.");

        if (generateOnLoad)
        {
            GenerateRandomLayers(maxLayers);
        }

        yield return new WaitForSecondsRealtime(0f);
    }

    public void DestroyAllLayers()
    {
        //Utils.DestroyListOfItems(Layers.Select(layer => layer.gameObject).ToList());

        var allLayers = LayersContainer.GetComponentsInChildren<Layer>();
        foreach (var l in allLayers)
        {
            Destroy(l.gameObject);
        }
        Layers = new Layer[0];
        
    }

    /// <summary>
    /// Instantiates a random layer from the LayerBank, positions it with a Z offset,
    /// and adjusts its material to use a runtime instance with modified opacity.
    /// </summary>
    private Layer CreateLayerInstance(int layerIndex, int totalLayers, Vector3 anchorPosition)
    {
        // Select a random prefab from the LayerBank.
        GameObject prefab = LayerBank[Random.Range(0, LayerBank.Length)].gameObject;

        // Calculate the final position of this layer (with proper Z offset)
        Vector3 layerPosition = anchorPosition;
        layerPosition.z -= layerIndex * layerZDistance;

        // Instantiate the layer at the final position.
        GameObject NewLayer = Instantiate(prefab, layerPosition, Quaternion.identity, LayersContainer.transform);

        // Set up the SpriteRenderer and its material.
        SpriteRenderer spriteRenderer = NewLayer.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.material = Instantiate(defaultLayerMaterial);

            float alpha = 0.90f;
            float h, s, baseV;
            Color.RGBToHSV(baseColor, out h, out s, out baseV);
            float targetV = Mathf.Lerp(1f, 0.5f, (float)layerIndex / totalLayers);

            Color initialColor = Color.HSVToRGB(h, s, targetV);
            initialColor.a = alpha;
            spriteRenderer.material.color = initialColor;
            spriteRenderer.sortingOrder = (layerIndex * -10) - 10;
        }
        else
        {
            Debug.LogError($"SpriteRenderer on layer prefab {prefab.name} was null");
        }

        // Spawn pickups using the calculated position.

        Layer layer = NewLayer.GetComponent<Layer>();
        LevelController.Instance.SpawnCollectiblesOnLayer(layer);

        return layer;
    }



    /// <summary>
    /// Generates a set of random layers using the CreateLayerInstance helper.
    /// </summary>
    public void GenerateRandomLayers(int numberOfLayers = 5)
    {
        DestroyAllLayers();

        for (int i = 0; i < numberOfLayers; i++)
        {
            CreateLayerInstance(i, numberOfLayers, activeLayerAnchor.position);
        }

        RefreshLayersArray();

        // Set the active layer as the one with the lowest Z (closest to the camera).
        if (Layers.Length > 0)
        {
            activeLayer = Layers[0];
        }
    }

    /// <summary>
    /// Generates a new random layer intended to be added to the bottom of the layer stack.
    /// The new layer is created with an initial opacity of 0.
    /// </summary>
    public Layer GenerateRandomBottomLayer()
    {
        // Use the activeLayerAnchor as the base.
        Vector3 newAnchorPosition = activeLayerAnchor.position;
        // Set the new anchor z such that when passed to CreateLayerInstance (with index = maxLayers - 1),
        // the resulting layer's final z will be activeLayerAnchor.position.z + maxLayers * Abs(layerZDistance).
        newAnchorPosition.z = activeLayerAnchor.position.z - layerZDistance; // layerZDistance is negative

        // Create the new bottom layer at the new anchor position.
        var layer = CreateLayerInstance(maxLayers - 1, maxLayers, newAnchorPosition);

        // Set its opacity to 0 so it can fade in later.
        SpriteRenderer sr = layer.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = sr.material.color;
            sr.material.color = new Color(color.r, color.g, color.b, 0f);
        }

        RefreshLayersArray();
        return layer;
    }

    /// <summary>
    /// Shifts the layers by moving the camera forward to the next layer,
    /// smoothly tweening opacity for all layers (including the new bottom layer, which fades in),
    /// and then removing the old active layer.
    /// </summary>
    // Refactored ShiftLayers() to tween brightness while keeping each layer's alpha constant,
    // except for the new bottom layer which fades in from 0 to 0.95.
    public IEnumerator ShiftLayers()
    {
        if (isShifting) yield break;
        RefreshLayersArray();
        isShifting = true;

        // Create the new bottom layer before shifting.
        GenerateRandomBottomLayer();
        RefreshLayersArray();

        // Determine the new active layer: the second layer in our sorted array.
        Layer nextLayer = Layers.Length > 1 ? Layers[1] : null;
        if (nextLayer == null)
        {
            Debug.LogError("No next layer available for shifting.");
            isShifting = false;
            yield break;
        }

        // Smoothly move the camera toward the new active layer.
        Vector3 offset = CameraController.Instance.cam.transform.TransformDirection(new Vector3(0, 0, cameraZDistance));
        Vector3 targetPosition = nextLayer.transform.position + offset;

        CameraController.Instance.SmoothFollowToPosition(targetPosition, shiftDuration);

        // Tween all layers' materials
        for (int i = 0; i < Layers.Length; i++)
        {
            Layer layer = Layers[i];
            SpriteRenderer sr = layer.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color currentColor = sr.material.color;
                float currentAlpha = currentColor.a;
                float targetAlpha = currentAlpha;

                if (i == Layers.Length - 1)
                {
                    targetAlpha = 0.95f;
                }

                float targetBrightness = Mathf.Lerp(1f, 0.5f, (float)i / (maxLayers - 1));

                Color.RGBToHSV(baseColor, out float h, out float s, out _);
                Color targetRGB = Color.HSVToRGB(h, s, targetBrightness);
                Color targetColorRGB = new Color(targetRGB.r, targetRGB.g, targetRGB.b, currentAlpha);

                sr.material.DOColor(targetColorRGB, shiftDuration).SetEase(Ease.InOutCubic);

                if (i == Layers.Length - 1)
                {
                    DOTween.To(() => sr.material.color.a, x =>
                    {
                        Color c = sr.material.color;
                        c.a = x;
                        sr.material.color = c;
                    }, targetAlpha, shiftDuration).SetEase(Ease.InOutCubic);
                }

                sr.sortingOrder = (i * -10);
            }
        }

        // Move the player straight downward, keeping X/Y, updating Z to the next layer's Z
        Vector3 playerPos = PlayerManager.Instance.transform.position;
        float newZ = nextLayer.transform.position.z;
        Vector3 newPlayerPos = new Vector3(playerPos.x, playerPos.y, newZ);

        // Optional: add a tween if you want the descent to feel smooth
        PlayerManager.Instance.transform.position = newPlayerPos;

        // Set new active layer
        activeLayer = nextLayer;
        yield return new WaitForSeconds(shiftDuration);

        // Keep anchor aligned with new active layer (we keep using its Z for new layers)
        activeLayerAnchor.position = activeLayer.transform.position;

        // Destroy the old active layer (the one with the lowest Z)
        Layer oldActiveLayer = Layers.Length > 0 ? Layers[0] : null;
        if (oldActiveLayer != null)
        {
            Destroy(oldActiveLayer.gameObject);
        }

        yield return new WaitForEndOfFrame();
        RefreshLayersArray();
        isShifting = false;
        layersVisited++;
    }





    //public IEnumerator ShiftLayers()
    //{
    //    if (isShifting) yield break;
    //    RefreshLayersArray();
    //    isShifting = true;

    //    // Create the new bottom layer before shifting.
    //    GenerateRandomBottomLayer();
    //    RefreshLayersArray();

    //    // Determine the new active layer: the second layer in our sorted array.
    //    Layer nextLayer = Layers.Length > 1 ? Layers[1] : null;
    //    if (nextLayer == null)
    //    {
    //        Debug.LogError("No next layer available for shifting.");
    //        isShifting = false;
    //        yield break;
    //    }

    //    // Compute the offset based on the camera's current rotation and add that offset to the next layer's position.
    //    Vector3 offset = CameraController.Instance.cam.transform.TransformDirection(new Vector3(0, 0, cameraZDistance));
    //    Vector3 targetPosition = nextLayer.transform.position + offset;

    //    CameraController.Instance.SmoothFollowToPosition(targetPosition, shiftDuration);

    //    // tween the material’s color(RGB channels) to this target color and, for the new bottom layer, tween its alpha separately.
    //    for (int i = 0; i < Layers.Length; i++)
    //    {
    //        Layer layer = Layers[i];
    //        SpriteRenderer sr = layer.GetComponent<SpriteRenderer>();
    //        if (sr != null)
    //        {
    //            // Get current material color.
    //            Color currentColor = sr.material.color;
    //            float currentAlpha = currentColor.a;
    //            float targetAlpha = currentAlpha;
    //            if (i == Layers.Length - 1)
    //            {
    //                // For the new bottom layer, tween alpha from 0 up to 0.95.
    //                targetAlpha = 0.95f;
    //            }

    //            // Calculate target brightness based on the layer's index.
    //            float targetBrightness = Mathf.Lerp(1f, 0.5f, (float)i / (maxLayers - 1));

    //            // Use baseColor's hue and saturation for consistency.
    //            float h, s, _;
    //            Color.RGBToHSV(baseColor, out h, out s, out _);
    //            Color targetRGB = Color.HSVToRGB(h, s, targetBrightness);
    //            // Build the target color using the new RGB values and keeping current alpha.
    //            Color targetColorRGB = new Color(targetRGB.r, targetRGB.g, targetRGB.b, currentAlpha);

    //            // Tween the material's RGB color over shiftDuration.
    //            sr.material.DOColor(targetColorRGB, shiftDuration).SetEase(Ease.InOutCubic);

    //            // For the new bottom layer, tween the material's alpha separately.
    //            if (i == Layers.Length - 1)
    //            {
    //                DOTween.To(() => sr.material.color.a, x =>
    //                {
    //                    Color c = sr.material.color;
    //                    c.a = x;
    //                    sr.material.color = c;
    //                }, targetAlpha, shiftDuration).SetEase(Ease.InOutCubic);
    //            }

    //            sr.sortingOrder = (i * -10);
    //        }
    //    }


    //    activeLayer = nextLayer;
    //    yield return new WaitForSeconds(shiftDuration);

    //    // Update the activeLayerAnchor to the new active layer's position.
    //    activeLayerAnchor.position = activeLayer.transform.position;

    //    // Destroy the old active layer (the one with the lowest Z).
    //    Layer oldActiveLayer = Layers.Length > 0 ? Layers[0] : null;
    //    if (oldActiveLayer != null)
    //    {
    //        Destroy(oldActiveLayer);
    //    }

    //    // Wait a frame to ensure the destroyed object is removed.
    //    yield return new WaitForEndOfFrame();
    //    RefreshLayersArray();
    //    isShifting = false;
    //}






    /// <summary>
    /// Refreshes the Layers array by fetching all Layer components from the LayersContainer,
    /// then sorting them by their Z position.
    /// </summary>
    void RefreshLayersArray()
    {
        Layers = LayersContainer
            .GetComponentsInChildren<Layer>()
            .OrderBy(layer => layer.transform.position.z)
            .ToArray();
    }
}
