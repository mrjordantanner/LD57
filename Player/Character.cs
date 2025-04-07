using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Character : MonoBehaviour
{
    public GameObject Entity { get { return gameObject; } }

    public int currentHealth;
    [ReadOnly] public int maxHealth;

    public float hitFlashDuration = 0.10f;

    public Animator anim;
    public SoundEffect hitSound, dieSound;
    public GameObject HitVFX, DeathVFX;
    [HideInInspector] public Material startingMaterial;

    public SpriteRenderer characterSpriteRenderer;

    private void Awake()
    {
        if (!characterSpriteRenderer) characterSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!anim) anim = GetComponentInChildren<Animator>();

        startingMaterial = characterSpriteRenderer.material;
        //Init();
    }

    public virtual void Init()
    {
       currentHealth = maxHealth;
       // State = PlayerState.Idle;
    }


    //public void HitByProjectile(int damage)
    //{
    //    HitFlash();
    //    if (hitSound) hitSound.Play();
    //    if (HitVFX) Instantiate(HitVFX, transform.position, Quaternion.identity, VFX.Instance.VFXContainer.transform);

    //    if (damage <= 0) damage = 1;
    //    TakeDamage(damage);

    //}

    public virtual void TakeDamage(float damageAmount)
    {
        print($"{name} takes {damageAmount} damage");
        HitFlash();

        currentHealth -= (int)damageAmount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public virtual void Die()
    {
        //if (dieSound) dieSound.Play();
        if (DeathVFX) Instantiate(DeathVFX, transform.position, Quaternion.identity, VFX.Instance.VFXContainer.transform);
        print($"{name} dies");
        Destroy(gameObject);
    }

    public virtual void HitFlash()
    {
        StartCoroutine(SpriteFlash(VFX.Instance.hitFlashMaterial));
    }

    public virtual IEnumerator SpriteFlash(Material material)
    {
        characterSpriteRenderer.material = startingMaterial;
        //State = PlayerState.Hurt;

        if (characterSpriteRenderer != null)
        {
            characterSpriteRenderer.material = material;
            yield return new WaitForSeconds(hitFlashDuration);
        }
        if (characterSpriteRenderer != null)
        {
            characterSpriteRenderer.material = startingMaterial;
        }

       //State = PlayerState.Idle;
    }

}
