//외부로 부터 데미지를 받을수 있도록 설정
public interface IDamageable
{
    bool ApplyDamage(DamageMessage damageMessage);
}