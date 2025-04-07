using System.Collections.Generic;
using UnityEngine;

public class Layer : MonoBehaviour
{
    public Material material;
    public Sprite sprite;
    public SpriteRenderer SpriteRenderer => GetComponent<SpriteRenderer>();

    public List<GameObject> PickupClusters = new();

    private void Awake()
    {
        if (SpriteRenderer != null)
        {
            material = GetComponent<SpriteRenderer>().material;
        }
    }

   public void CleanUp()
   {
        if (PickupClusters.Count > 0)
        {
            foreach (var cluster in PickupClusters)
            {
                Destroy(cluster);
            }
        }

        Destroy(gameObject);
   }
}
