using UnityEngine;

/// <summary>
/// 이펙트(폭발) Object Pool 관리자.
/// 적이 죽을 때마다 Instantiate/Destroy 하지 않고, 미리 만들어 둔 폭발 이펙트를 꺼내 쓴다.
/// </summary>
public class EffectManager : MonoBehaviour
{
    [Header("Prefab / Pool")]
    [SerializeField] private ExplosionEffect explosionPrefab;
    [SerializeField, Range(5, 60)] private int explosionPoolSize = 20;

    private SimplePool<ExplosionEffect> explosionPool;

    private void Awake()
    {
        if (explosionPrefab == null)
        {
            Debug.LogError("EffectManager:: explosionPrefab is null");
            return;
        }

        explosionPool = new SimplePool<ExplosionEffect>(explosionPrefab, explosionPoolSize, transform);
    }

    /// <summary>해당 위치에서 폭발 이펙트 재생. 풀이 비어 있으면 (이펙트는 필수가 아니므로) 조용히 건너뛴다.</summary>
    public void PlayExplosion(Vector2 position, float scale = 1f)
    {
        if (explosionPool == null) return;
        if (!explosionPool.TryGet(out ExplosionEffect effect)) return;

        effect.Play(position, scale);
    }
}
