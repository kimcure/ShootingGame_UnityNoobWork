using System.Collections;
using UnityEngine;

/// <summary>
/// 피격 연출. 맞는 순간 스프라이트가 잠깐 붉게 번쩍이고,
/// 무적 시간이 있는 대상(플레이어)은 그동안 깜빡인다.
///
/// HPController의 Damaged 이벤트만 구독하므로 Player/Enemy 어느 쪽에 붙여도 동작한다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(HPController))]
public class HitFlash : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField, Range(0.02f, 0.3f)] private float flashDuration = 0.08f;

    [Header("Invincible Blink")]
    [Tooltip("무적 시간 동안 깜빡이게 할지 (플레이어용)")]
    [SerializeField] private bool blinkWhileInvincible = false;
    [SerializeField, Range(0.03f, 0.2f)] private float blinkInterval = 0.08f;

    private SpriteRenderer spriteRenderer;
    private HPController hp;
    private Color baseColor;
    private Coroutine routine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        hp = GetComponent<HPController>();
        baseColor = spriteRenderer.color;

        hp.Damaged += HandleDamaged;
    }

    private void OnDestroy()
    {
        if (hp != null) hp.Damaged -= HandleDamaged;
    }

    // 풀로 돌아가 비활성화될 때 색을 원래대로 (색이 빨갛게 남은 채 재사용되는 버그 방지)
    private void OnDisable()
    {
        routine = null;
        if (spriteRenderer != null) spriteRenderer.color = baseColor;
    }

    private void HandleDamaged(float damage)
    {
        // 죽는 순간에는 오브젝트가 곧 꺼지므로 코루틴을 시작하지 않는다
        if (!isActiveAndEnabled || hp.IsDead) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = baseColor;

        if (blinkWhileInvincible)
        {
            float endTime = Time.time + Mathf.Max(0f, hp.InvincibleDuration - flashDuration);
            bool dim = true;

            while (Time.time < endTime)
            {
                Color c = baseColor;
                c.a = dim ? 0.3f : 1f;
                spriteRenderer.color = c;
                dim = !dim;
                yield return new WaitForSeconds(blinkInterval);
            }

            spriteRenderer.color = baseColor;
        }

        routine = null;
    }
}
