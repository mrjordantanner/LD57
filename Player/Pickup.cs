using UnityEngine;
using DG.Tweening;
using System.Collections;


public class Pickup : MonoBehaviour, ITarget
{
    public enum PickupType { Charge, Powerup }
    public PickupType type;

    public GameObject Entity { get { return gameObject; } }

    public GameObject DestroyVFX;
    public SoundEffect destroySound;

    public int pointValue;

    public void OnTargetHit()
    {
        if (DestroyVFX) Instantiate(DestroyVFX, transform.position, Quaternion.identity, VFX.Instance.VFXContainer.transform);
        if (destroySound) destroySound.Play();

        Destroy(gameObject);
    }

}
