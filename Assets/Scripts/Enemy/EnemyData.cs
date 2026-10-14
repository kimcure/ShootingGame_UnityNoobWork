using UnityEngine;

/// <summary>
/// 적 한 종류의 "설계도" (ScriptableObject).
///
/// 적 프리팹은 EnemyPrefab 하나만 쓰고, 소환될 때 이 데이터를 덮어씌워 종류를 만든다.
///  → 스프라이트/체력/속도/이동 패턴/사격 여부만 바꾸면 새로운 적이 생기므로 아트와 밸런스를 쉽게 연동할 수 있다.
///
/// 만드는 법: Project 창 우클릭 → Create → Shooter → Enemy Data
/// </summary>
[CreateAssetMenu(menuName = "Shooter/Enemy Data", fileName = "EnemyData_New")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public EnemyKind kind = EnemyKind.Scout;

    [Header("Art")]
    [Tooltip("애니메이션 프레임. 2장 이상이면 자동으로 반복 재생된다")]
    public Sprite[] frames;
    [Min(1f)] public float frameRate = 6f;
    [Min(0.3f)] public float spriteScale = 1f;
    [Tooltip("죽을 때 폭발 이펙트 크기 배율")]
    [Min(0.3f)] public float explosionScale = 1f;

    [Header("Stats")]
    [Min(1f)] public float maxHP = 2f;
    [Min(0.1f)] public float moveSpeed = 3f;
    [Tooltip("플레이어와 부딪혔을 때 플레이어가 입는 피해")]
    [Min(0f)] public float contactDamage = 1f;
    [Min(0)] public int scoreValue = 100;

    [Header("Movement")]
    public MovePattern movePattern = MovePattern.Straight;
    [Tooltip("Sine 패턴: 좌우 흔들림 폭")]
    [Min(0f)] public float sineAmplitude = 1.5f;
    [Tooltip("Sine 패턴: 흔들림 속도")]
    [Min(0.1f)] public float sineFrequency = 2.5f;

    [Header("Attack")]
    [Tooltip("미사일 발사 간격(초). 0이면 발사하지 않는다")]
    [Min(0f)] public float fireInterval = 0f;
    [Min(1f)] public float missileSpeed = 5f;
    [Min(0.1f)] public float missileDamage = 1f;
    [Tooltip("true면 플레이어를 향해 쏘고, false면 정면(아래)으로 쏜다")]
    public bool aimAtPlayer = false;
}
