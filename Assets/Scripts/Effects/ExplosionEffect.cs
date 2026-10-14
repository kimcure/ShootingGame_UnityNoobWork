using UnityEngine;

/// <summary>
/// 폭발 이펙트 1개. SpriteFlipbook이 마지막 프레임까지 재생하면 스스로 풀로 돌아간다.
/// (재생 시작은 SpriteFlipbook.OnEnable이 알아서 하므로, 여기서는 위치/크기만 정한다)
/// </summary>
[RequireComponent(typeof(SpriteFlipbook))]
public class ExplosionEffect : PooledObject
{
    private SpriteFlipbook flipbook;

    private void Awake()
    {
        flipbook = GetComponent<SpriteFlipbook>();
        flipbook.Completed += ReturnToPool;   // 애니메이션이 끝나면 풀로 반납
    }

    private void OnDestroy()
    {
        if (flipbook != null) flipbook.Completed -= ReturnToPool;
    }

    public void Play(Vector2 position, float scale)
    {
        transform.position = position;
        transform.localScale = Vector3.one * scale;
    }
}
