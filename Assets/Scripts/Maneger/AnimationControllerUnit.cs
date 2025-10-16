using UnityEngine;
using System.Collections;

public class AnimationControllerUnit : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [Header("SFX (аудио)")]
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip impactClip; // звук попадания/удара по цели
    [SerializeField] private AudioClip missClip;   // звук промаха (whoosh)

    [Header("VFX (визуальные эффекты)")]
    [Tooltip("Эффект крови, проигрывается при получении урона")]
    [SerializeField] private ParticleSystem hitBloodVFX;

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    // ========= Управление анимациями =========
    public void PlayMove()
    {
        if (!animator) return;
        animator.CrossFade("Walk", 0.10f);
        PlaySound(moveClip);
    }

    public IEnumerator PlayAttack()
    {
        if (!animator) yield break;
        animator.CrossFade("Attack", 0.10f);
        PlaySound(attackClip);

        // важная задержка на один кадр — даём Animator начать переход
        yield return null;

        // ждём текущую анимацию атаки
        var state = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(state.length);
    }

    public IEnumerator PlayDeath()
    {
        if (!animator) yield break;
        animator.CrossFade("Death", 0.10f);
        PlaySound(deathClip);
        yield return null;

        var state = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(state.length);
    }

    public void PlayIdle()
    {
        if (!animator) return;
        animator.CrossFade("Idle", 0.20f);
        PlaySound(idleClip);
    }

    // ========= Аудио-методы для логических событий боя =========
    public void PlayImpact()  // звук попадания (на стороне атакующего)
    {
        PlaySound(impactClip);
    }

    public void PlayMiss()    // звук промаха (на стороне атакующего)
    {
        PlaySound(missClip);
    }

    // ========= Визуальный эффект попадания (на стороне защитника) =========
    public void PlayHitEffect()
    {
        if (hitBloodVFX != null)
        {
            // Если VFX выключен — включим, затем проиграем
            if (!hitBloodVFX.gameObject.activeSelf) hitBloodVFX.gameObject.SetActive(true);
            hitBloodVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitBloodVFX.Play(true);
        }
    }

    // ========= Вспомогательное =========
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // (опционально) метод для Animation Event из клипа атаки,
    // если захочешь вызывать звук/эффект строго в ключевой кадр:
    // public void AnimationEvent_Impact()
    // {
    //     PlayImpact();
    //     PlayHitEffect();
    // }
}
