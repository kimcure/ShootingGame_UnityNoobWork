using System;
using UnityEngine;

/// <summary>
/// HP "데이터와 규칙"만 책임지는 컴포넌트. Player와 Enemy가 똑같이 재사용한다. (5강: 책임 분리)
///
///  - HP를 깎고(TakeDamage) 죽었는지(IsDead) 판단한다.
///  - 무엇이 일어났는지는 이벤트(HPChanged / Damaged / Died)로 알린다.
///    → 체력바(HPBarView), 피격 연출(HitFlash), HUD, 사망 처리(Enemy/Player)가 각자 구독해서 반응한다.
///  - 화면에 어떻게 보일지는 모른다 (UI는 HPBarView의 책임).
///
/// 충돌 처리(미사일, 접촉)는 "상대가 HPController를 가졌는가?"만 확인하면 되므로
/// Tag 목록을 늘려 가는 대신 Component 기반 구분(수업 5강의 "원하는 대상 확인")을 쓴다.
/// </summary>
public class HPController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("피격 후 무적 시간(초). 플레이어처럼 연속 피해를 막고 싶을 때 사용. 0이면 무적 없음.")]
    [SerializeField, Min(0f)] private float invincibleDuration = 0f;

    public Team Team { get; private set; } = Team.Enemy;
    public float HP { get; private set; }
    public float MaxHP { get; private set; }

    public bool IsDead => HP <= 0f;
    public bool IsAlive => HP > 0f;
    public float Ratio => MaxHP <= 0f ? 0f : HP / MaxHP;
    public float InvincibleDuration => invincibleDuration;
    public bool IsInvincible => Time.time < invincibleUntil;

    /// <summary>HP 수치가 바뀔 때마다 (현재 HP, 최대 HP).</summary>
    public event Action<float, float> HPChanged;

    /// <summary>실제로 피해를 입었을 때 (피해량).</summary>
    public event Action<float> Damaged;

    /// <summary>HP가 0이 되는 순간 한 번.</summary>
    public event Action Died;

    private float invincibleUntil;

    /// <summary>
    /// 팀과 최대 HP를 정하고 HP를 가득 채운다.
    /// 풀에서 적이 재사용될 때마다 호출되므로 "되살리기(리셋)" 역할도 한다.
    /// (SetActive(false)는 HP 같은 필드를 초기화해 주지 않는다는 5강의 버그 교훈)
    /// </summary>
    public void Configure(Team team, float maxHP)
    {
        Team = team;
        MaxHP = Mathf.Max(1f, maxHP);
        HP = MaxHP;
        invincibleUntil = 0f;

        HPChanged?.Invoke(HP, MaxHP);
    }

    /// <summary>
    /// 피해를 준다. 실제로 피해가 들어갔으면 true.
    /// 이미 죽었거나 무적 중이면 무시한다.
    /// </summary>
    public bool TakeDamage(float damage)
    {
        if (IsDead || IsInvincible || damage <= 0f) return false;

        HP = Mathf.Max(0f, HP - damage);

        if (invincibleDuration > 0f)
        {
            invincibleUntil = Time.time + invincibleDuration;
        }

        // 순서가 중요: 수치 갱신 → 피격 연출 → 사망 (사망 처리에서 오브젝트가 꺼질 수 있으므로 마지막)
        HPChanged?.Invoke(HP, MaxHP);
        Damaged?.Invoke(damage);

        if (IsDead)
        {
            Died?.Invoke();
        }

        return true;
    }
}
