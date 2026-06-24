using System;
using UnityEngine;

public class EnemyAnimEventHandler : MonoBehaviour
{
    public event Action OnHitStart;
    public event Action OnHitEnd;
    public event Action OnAttackEnd;

    // 히트 판정 시작 프레임에 호출
    public void AnimEvent_HitStart()
    {
        OnHitStart?.Invoke();
    }

    // 히트 판정 끝 프레임에 호출
    public void AnimEvent_HitEnd()
    {
        OnHitEnd?.Invoke();
    }

    // 공격 애니메이션 끝 프레임에 호출
    public void AnimEvent_AttackEnd()
    {
        OnAttackEnd?.Invoke();
    }
}
