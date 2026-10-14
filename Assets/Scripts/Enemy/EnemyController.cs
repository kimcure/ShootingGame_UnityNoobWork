using System;
using UnityEngine;
using Random = UnityEngine.Random;   // System.Random과 이름이 겹치므로 UnityEngine.Random을 쓰겠다고 명시

/// <summary>
/// 적 1마리. EnemyManager(Object Pool)가 꺼내서 Spawn()으로 종류(EnemyData)를 입혀 준다.
///
/// 책임 분리 (수업 2강 "EnemyController는 움직일 수 있는 방법을 알고, Manager는 언제 누구를 움직일지 결정")
///  - EnemyController : 이동 패턴 실행, 사격, 충돌/사망 처리
///  - EnemyManager    : 생성(풀), 대열 계산, 소환 타이밍
///  - HPController    : 체력 (HPBarView/HitFlash는 이벤트로 연동)
///
/// 이동은 Rigidbody2D(Kinematic) + MovePosition + FixedUpdate + fixedDeltaTime (수업 규칙 2, 3, 5).
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(HPController), typeof(SpriteFlipbook))]
public class EnemyController : PooledObject
{
    [Header("Settings")]
    [Tooltip("Zigzag 패턴이 화면 가장자리에서 방향을 바꾸는 여유 거리")]
    [SerializeField, Range(0f, 3f)] private float screenEdgeMargin = 0.6f;
    [Tooltip("화면 아래로 이만큼 더 벗어나면 사라진다")]
    [SerializeField, Range(0f, 5f)] private float offscreenPadding = 1.5f;

    /// <summary>플레이어에게 처치당했을 때 (점수 계산은 Manager/GameManager가 한다).</summary>
    public event Action<EnemyController> Killed;

    public EnemyData Data => data;

    private Rigidbody2D rb;
    private HPController hp;
    private SpriteFlipbook flipbook;

    // Manager가 주입해 주는 의존성 (Initialize)
    private Transform target;
    private MissileManager missileManager;
    private EffectManager effectManager;

    // 소환될 때마다 새로 정해지는 상태 (Spawn에서 전부 초기화)
    private EnemyData data;
    private float elapsed;
    private float originX;
    private float fireTimer;
    private float zigzagDirection;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = GetComponent<HPController>();
        flipbook = GetComponent<SpriteFlipbook>();

        // 탑다운 적 = 코드가 직접 움직이는 Kinematic Rigidbody (수업 Body Type)
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        hp.Died += HandleDied;
    }

    private void OnDestroy()
    {
        if (hp != null) hp.Died -= HandleDied;
    }

    /// <summary>풀이 만들어진 직후 한 번, EnemyManager가 호출해서 필요한 참조를 넘겨준다.</summary>
    public void Initialize(Transform player, MissileManager missiles, EffectManager effects)
    {
        target = player;
        missileManager = missiles;
        effectManager = effects;
    }

    /// <summary>
    /// 풀에서 꺼낸 직후 호출. 종류를 입히고 모든 상태를 초기화한다.
    /// (재사용되는 오브젝트이므로 "이전 생애의 값"이 남지 않게 여기서 전부 덮어쓴다)
    /// </summary>
    public void Spawn(EnemyData newData, Vector2 position)
    {
        data = newData;

        isDead = false;
        elapsed = 0f;
        originX = position.x;
        zigzagDirection = position.x >= 0f ? -1f : 1f;   // 화면 가운데 쪽으로 먼저 움직이게
        fireTimer = data.fireInterval > 0f ? Random.Range(0.5f, 1f) * data.fireInterval : 0f;   // 첫 발사 타이밍을 흩뜨려 일제사격 방지

        transform.position = position;
        rb.position = position;
        transform.localScale = Vector3.one * data.spriteScale;

        hp.Configure(Team.Enemy, data.maxHP);
        flipbook.SetFrames(data.frames, data.frameRate);
    }

    private void FixedUpdate()
    {
        if (!InUse || isDead || data == null) return;

        float dt = Time.fixedDeltaTime;
        elapsed += dt;

        Vector2 next = CalculateNextPosition(dt);
        rb.MovePosition(next);

        // 화면 아래로 빠져나가면 (놓친 적) 조용히 반납
        if (ScreenWorldBounds.IsOutBottom(next, offscreenPadding))
        {
            ReturnToPool();
        }
    }

    private void Update()
    {
        // 사격은 물리가 아니라 "시간"이므로 Update에서 처리
        if (!InUse || isDead || data == null || data.fireInterval <= 0f) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f) return;

        fireTimer = data.fireInterval;

        // 화면 안에 있을 때만 쏜다 (아직 화면 위쪽 바깥에서 쏘면 억울하다)
        if (ScreenWorldBounds.IsInside(transform.position))
        {
            FireMissile();
        }
    }

    // ───────── 이동 패턴 ─────────

    private Vector2 CalculateNextPosition(float dt)
    {
        Vector2 position = rb.position;   // 수업 규칙 3: Rigidbody로 움직이는 오브젝트의 위치는 rigidbody.position에서 읽는다
        float speed = data.moveSpeed;

        switch (data.movePattern)
        {
            case MovePattern.Straight:
                position.y -= speed * dt;
                break;

            case MovePattern.Sine:
                // 아래로 내려오면서 x는 "스폰 위치 ± 진폭"으로 흔들린다
                position.y -= speed * dt;
                position.x = originX + Mathf.Sin(elapsed * data.sineFrequency) * data.sineAmplitude;
                break;

            case MovePattern.Zigzag:
                // 옆으로 튕기듯 움직이며 천천히 내려온다
                position.y -= speed * 0.5f * dt;
                position.x += zigzagDirection * speed * dt;

                if (position.x > ScreenWorldBounds.MaxX - screenEdgeMargin) zigzagDirection = -1f;
                else if (position.x < ScreenWorldBounds.MinX + screenEdgeMargin) zigzagDirection = 1f;
                break;

            case MovePattern.Chase:
                if (target != null && target.gameObject.activeInHierarchy)
                {
                    // 수업의 추적 코드: 방향 = 목표 - 나, Normalize로 방향만 남기고 속도를 곱한다
                    Vector2 direction = (Vector2)target.position - position;
                    if (direction.sqrMagnitude > 0.0001f) direction.Normalize();
                    position += direction * speed * dt;
                }
                else
                {
                    position.y -= speed * dt;   // 플레이어가 없으면 그냥 직진
                }
                break;
        }

        return position;
    }

    // ───────── 사격 ─────────

    private void FireMissile()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * (0.5f * data.spriteScale);
        Vector2 direction = Vector2.down;

        if (data.aimAtPlayer && target != null && target.gameObject.activeInHierarchy)
        {
            Vector2 toPlayer = (Vector2)target.position - origin;
            if (toPlayer.sqrMagnitude > 0.01f) direction = toPlayer.normalized;
        }

        missileManager.Fire(Team.Enemy, origin, direction, data.missileSpeed, data.missileDamage);
    }

    // ───────── 충돌 / 사망 ─────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!InUse || isDead) return;

        // 플레이어(= 반대 팀의 HP를 가진 대상)와 부딪히면: 플레이어에게 피해를 주고 자폭
        if (!other.TryGetComponent(out HPController otherHp)) return;
        if (otherHp.Team != Team.Player) return;

        otherHp.TakeDamage(data.contactDamage);
        Explode(false);   // 부딪혀서 죽은 경우에는 점수를 주지 않는다
    }

    // HP가 0이 되면 (= 플레이어 미사일에 맞아 죽으면) 점수를 준다
    private void HandleDied()
    {
        Explode(true);
    }

    private void Explode(bool giveScore)
    {
        if (isDead) return;   // 같은 프레임에 두 번 죽는 것을 방지
        isDead = true;

        if (effectManager != null)
        {
            effectManager.PlayExplosion(transform.position, data.explosionScale);
        }

        if (giveScore)
        {
            Killed?.Invoke(this);
        }

        ReturnToPool();
    }
}
