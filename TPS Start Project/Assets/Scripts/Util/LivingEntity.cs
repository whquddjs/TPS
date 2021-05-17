using System;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    //초기 체력
    public float startingHealth = 100f;

    //현재 체력 외부에서는 체력값을 읽을수는 있으나 덮어쓸수는 없음
    public float health { get; protected set; }

    //사망상태 표시 마찬가지로 죽음 여부를 읽을 수는 있으나 덮어 쓰기 불가능
    public bool dead { get; protected set; }
    
    //LivingEntity가 사망하는 순간에 실행되는 콜백을 외부에서 할당할 이벤트
    public event Action OnDeath;
    
    //공격과 공격사이 최소한의 대기시간
    //짧은 시간 사이에 여러번의 공격당하는 것을 막기위해 설정
    private const float minTimeBetDamaged = 0.1f;

    //최근에 공격당한 시간
    private float lastDamagedTime;

    //시간을 재었을 때 공격을 당한 시점에서 최소 공격 대기시간이 지났는 지 판별함
    //최소 대기시간이 지나기 전에 공격을 당할 경우 무적판정
    protected bool IsInvulnerable
    {
        get
        {
            if (Time.time >= lastDamagedTime + minTimeBetDamaged) return false;

            return true;
        }
    }
    
    //LivingEntity의 자식 클래스들은 아래 코드를 이용해 더 확장 할 수 있다.
    protected virtual void OnEnable()
    {
        dead = false;
        health = startingHealth;
    }

    public virtual bool ApplyDamage(DamageMessage damageMessage)
    {
        //무적상태이거나 피해입힌 오브젝트가 플레이어이거나 죽은 상태일 경우
        if (IsInvulnerable || damageMessage.damager == gameObject || dead) return false;

        lastDamagedTime = Time.time;
        health -= damageMessage.amount;//데미지 받은 만큼 현재 체력 감소
        
        if (health <= 0) Die();//체력이 0 이하시 사망

        return true;
    }
    
    //체력을 입력된 양 만큼 회복
    public virtual void RestoreHealth(float newHealth)
    {
        if (dead) return;
        
        health += newHealth;
    }
    

    public virtual void Die()
    {
        //OnDeath에 최소 1개의 Listener가 등록되어있을 경우 OnDeath 이벤트 실행
        if (OnDeath != null) OnDeath();
        
        dead = true;
    }
}