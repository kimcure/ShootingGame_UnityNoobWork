using UnityEngine;

/// <summary>
/// 미사일 Object Pool 관리자. 플레이어용/적용 풀을 각각 하나씩 가진다.
///
/// 수업의 MissileManager(미리 N개 만들어 SetActive(false) → Fire 때 꺼내 쓰기)와 같은 구조이고,
/// "어떤 풀에서 꺼낼지"는 Team으로 결정한다. 풀 로직 자체는 SimplePool이 담당한다.
/// </summary>
public class MissileManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private MissileController playerMissilePrefab;
    [SerializeField] private MissileController enemyMissilePrefab;

    [Header("Pool Size")]
    [SerializeField, Range(10, 100)] private int playerMissileCount = 40;
    [SerializeField, Range(10, 100)] private int enemyMissileCount = 40;

    private SimplePool<MissileController> playerPool;
    private SimplePool<MissileController> enemyPool;

    private void Awake()
    {
        if (playerMissilePrefab == null || enemyMissilePrefab == null)
        {
            Debug.LogError("MissileManager:: missile prefab is null");
            return;
        }

        // Hierarchy가 지저분해지지 않도록 풀마다 부모 오브젝트를 만들어 모은다 (수업의 parentCleaner)
        playerPool = new SimplePool<MissileController>(playerMissilePrefab, playerMissileCount, CreateRoot("PlayerMissiles"));
        enemyPool = new SimplePool<MissileController>(enemyMissilePrefab, enemyMissileCount, CreateRoot("EnemyMissiles"));
    }

    /// <summary>
    /// 미사일 한 발 발사. 풀에 남은 미사일이 없으면 false (발사 안 됨).
    /// </summary>
    public bool Fire(Team team, Vector2 position, Vector2 direction, float speed, float damage)
    {
        SimplePool<MissileController> pool = team == Team.Player ? playerPool : enemyPool;
        if (pool == null) return false;

        if (!pool.TryGet(out MissileController missile)) return false;

        missile.Launch(team, position, direction, speed, damage);
        return true;
    }

    private Transform CreateRoot(string rootName)
    {
        Transform root = new GameObject(rootName).transform;
        root.SetParent(transform, false);
        return root;
    }
}
