using UnityEngine;

/// <summary>
/// 미사일 1발. 플레이어 미사일과 적 미사일이 이 클래스를 같이 쓰고, 프리팹(스프라이트)만 다르다.
///
/// 책임
///  - 발사(Launch): 위치/방향/속도/피해량/팀을 받아 날아가기 시작한다.
///  - 화면 밖으로 나가면 스스로 풀로 돌아간다 (ReturnToPool).
///  - 적(반대 팀의 HPController)에 닿으면 피해를 주고 풀로 돌아간다.
///
/// 수업 5강 교훈 반영
///  - "미사일이 적을 직접 SetActive(false) 하지 않는다": 미사일은 HPController.TakeDamage만 호출한다.
///    적이 죽을지 말지는 적 자신(HPController/EnemyController)이 판단한다.
///  - 재사용 시 상태(속도/팀/피해량)는 Launch에서 매번 새로 덮어쓴다.
///  - 충돌 대상은 "제외 목록"이 아니라 "HPController를 가진 반대 팀인가?"로 확인한다.
///
/// 이동은 Rigidbody2D.linearVelocity (수업: 탄환 = 일정 속도 이동).
/// Dynamic + Gravity 0 + Trigger Collider 구성이다 (Trigger 이벤트가 확실히 발생하도록).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MissileController : PooledObject
{
    [Header("Settings")]
    [Tooltip("화면 경계에서 이 거리만큼 더 벗어나야 반납한다")]
    [SerializeField, Range(0f, 3f)] private float offscreenPadding = 1f;

    private Rigidbody2D rb;
    private Team team;
    private float damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 프리팹 설정 실수를 막는 안전장치
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.freezeRotation = true;
    }

    /// <summary>풀에서 꺼낸 직후 MissileManager가 호출한다.</summary>
    public void Launch(Team ownerTeam, Vector2 position, Vector2 direction, float speed, float damageAmount)
    {
        team = ownerTeam;
        damage = damageAmount;

        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;

        // 스폰 순간의 순간이동은 transform과 rigidbody 둘 다 맞춘다 (한 프레임이라도 옛 위치에 보이는 것을 방지)
        transform.position = position;
        rb.position = position;

        // 스프라이트는 위쪽(+Y)을 향해 그려져 있으므로 진행 방향에 맞춰 회전 (-90도 보정)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        rb.rotation = angle;

        rb.linearVelocity = dir * speed;
    }

    private void FixedUpdate()
    {
        if (!InUse) return;

        if (ScreenWorldBounds.IsOutside(rb.position, offscreenPadding))
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!InUse) return;

        // 1. 상대가 HP를 가진 대상인가? (아니면 무시: 다른 미사일, 배경 등)
        if (!other.TryGetComponent(out HPController target)) return;

        // 2. 아군이면 무시 (플레이어 미사일은 플레이어를, 적 미사일은 적을 통과)
        if (target.Team == team) return;

        // 3. 이미 죽은 대상은 무시
        if (target.IsDead) return;

        target.TakeDamage(damage);
        ReturnToPool();
    }
}
