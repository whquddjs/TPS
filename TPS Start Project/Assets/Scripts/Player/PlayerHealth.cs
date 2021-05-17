using UnityEngine;

public class PlayerHealth : LivingEntity
{
    private Animator animator;
    private AudioSource playerAudioPlayer;

    public AudioClip deathClip;
    public AudioClip hitClip;


    private void Awake()
    {
        playerAudioPlayer = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    //LivingEntity의 OnEnable를 override
    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateUI();
    }

    //LivingEntity의 RestoreHealth를 override
    public override void RestoreHealth(float newHealth)
    {
        base.RestoreHealth(newHealth);
        UpdateUI();
    }

    private void UpdateUI()
    {
        //플레이어가 사망 시 체력 UI를 0으로, 아닐경우 현재 체력수치 표시
        UIManager.Instance.UpdateHealthText(dead ? 0f : health);
    }

    //LivingEntity의 ApplyDamage를 override
    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        //LivingEntity의 ApplyDamage가 활성화 안되었을 경우 false를 return
        if (!base.ApplyDamage(damageMessage)) return false;

        //정해진 위치에 이펙트 생성
        EffectManager.Instance.PlayHitEffect(damageMessage.hitPoint,
            damageMessage.hitNormal, transform, EffectManager.EffectType.Flesh);

        playerAudioPlayer.PlayOneShot(hitClip);

        //UI Text변경
        UpdateUI();

        return true;
    }

    //LivingEntity의 Die를 override
    public override void Die()
    {
        base.Die();
        //죽을때의 음성과 애니메이션 실행
        playerAudioPlayer.PlayOneShot(deathClip);
        animator.SetTrigger("Die");

        //UI변경
        UpdateUI();
    }
}