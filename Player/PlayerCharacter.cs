using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;


public class PlayerCharacter : Character
{
    public GameObject PlayerGraphics;
    public MeshRenderer meshRenderer;
    public SpriteFlicker spriteFlicker;

    [HideInInspector] public Rigidbody rb;


    //public SpriteRenderer spriteRenderer;
    [HideInInspector] public SpriteTrails trails;

    public LayerMask obstacleLayer;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        //if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        //if (!trails) trails = GetComponentInChildren<SpriteTrails>();
        //if (!spriteFlicker) spriteFlicker = GetComponentInChildren<SpriteFlicker>();

        if (PlayerManager.Instance) PlayerManager.Instance.UpdatePlayerRef(this);

        if (!meshRenderer) meshRenderer = GetComponentInChildren<MeshRenderer>();
        startingMaterial = meshRenderer.material;
    }

    void Update()
    {
        if (!GameManager.Instance.gameRunning || GameManager.Instance.gamePaused) return;

        HandleAnimation();
    }

    void HandleAnimation()
    {
        if (!anim || !rb) return;

        anim.SetFloat("velocityX", rb.velocity.x);
        anim.SetFloat("velocityY", rb.velocity.y);

        anim.SetBool("isMoving", PlayerManager.Instance.isMoving);
        anim.SetBool("isHurt", PlayerManager.Instance.State == PlayerState.Hurt);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Pickup"))
        {
            Debug.Log("Collided with pickup");

            PlayerManager.Instance.CollectPickup(collision.GetComponent<Pickup>());
        }
    }
}
