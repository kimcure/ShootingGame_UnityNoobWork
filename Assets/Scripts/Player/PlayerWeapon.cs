using UnityEngine;

/// <summary>
/// 플레이어의 "발사" 책임. (PlayerController는 입력/이동, PlayerWeapon은 발사 — 5강: 책임 분리)
///
///  - 발사 버튼을 누르고 있는 동안 fireInterval 간격으로 연사한다.
///  - 무기 레벨에 따라 발사 패턴이 달라진다 (개성 포인트).
///      Lv1 : 단발
///      Lv2 : 나란히 2발
///      Lv3 : 가운데 + 좌우로 벌어지는 3발
///  - 실제 미사일은 MissileManager(Object Pool)에서 꺼내 쓴다.
/// </summary>
public class PlayerWeapon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MissileManager missileManager;

    [Header("Settings")]
    [Tooltip("연사 간격(초). 작을수록 빠르다")]
    [SerializeField, Range(0.05f, 1f)] private float fireInterval = 0.18f;
    [SerializeField, Range(5f, 30f)] private float missileSpeed = 14f;
    [SerializeField, Min(0.1f)] private float damage = 1f;

    [Header("Pattern")]
    [Tooltip("기체 중심에서 총구까지의 거리 (위쪽)")]
    [SerializeField, Range(0f, 1.5f)] private float muzzleOffset = 0.5f;
    [Tooltip("Lv2/Lv3에서 좌우 미사일의 간격")]
    [SerializeField, Range(0.1f, 1f)] private float sideOffset = 0.25f;
    [Tooltip("Lv3에서 좌우 미사일이 벌어지는 각도")]
    [SerializeField, Range(0f, 30f)] private float spreadAngle = 12f;

    public int Level { get; private set; } = 1;

    private bool isFiring;
    private float nextFireTime;

    private void Awake()
    {
        if (missileManager == null)
        {
            Debug.LogError("PlayerWeapon:: missileManager is null !!!!!!!!!!");
            enabled = false;
        }
    }

    private void Update()
    {
        // 발사 중이 아니거나 아직 쿨다운이면 아무것도 하지 않는다
        if (!isFiring || Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireInterval;
        Fire();
    }

    /// <summary>PlayerController가 버튼 눌림/뗌에 맞춰 호출한다.</summary>
    public void SetFiring(bool firing)
    {
        isFiring = firing;
    }

    /// <summary>무기 레벨 설정 (1~3).</summary>
    public void SetLevel(int level)
    {
        Level = Mathf.Clamp(level, 1, 3);
    }

    private void Fire()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.up * muzzleOffset;
        Vector2 left = Vector2.left * sideOffset;
        Vector2 right = Vector2.right * sideOffset;

        switch (Level)
        {
            case 1:
                Shoot(origin, 0f);
                break;

            case 2:
                Shoot(origin + left, 0f);
                Shoot(origin + right, 0f);
                break;

            default: // 3
                Shoot(origin, 0f);
                Shoot(origin + left, spreadAngle);    // +각도 = 반시계 = 왼쪽으로 기울어짐
                Shoot(origin + right, -spreadAngle);  // -각도 = 시계   = 오른쪽으로 기울어짐
                break;
        }
    }

    private void Shoot(Vector2 position, float angleDegrees)
    {
        // 위쪽(Vector2.up)을 angle만큼 회전시킨 방향
        Vector2 direction = Quaternion.Euler(0f, 0f, angleDegrees) * Vector2.up;
        missileManager.Fire(Team.Player, position, direction, missileSpeed, damage);
    }
}
