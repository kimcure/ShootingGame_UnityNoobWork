using UnityEngine;

/// <summary>
/// 카메라 흔들림 연출. 플레이어가 피격당했을 때 "맞았다!"는 느낌을 준다.
/// 카메라 이동은 모든 Update가 끝난 뒤인 LateUpdate에서 한다 (수업: LateUpdate는 카메라 로직에 사용).
/// </summary>
public class CameraShake : MonoBehaviour
{
    private Vector3 originLocalPosition;
    private float timeLeft;
    private float duration;
    private float magnitude;

    private void Awake()
    {
        originLocalPosition = transform.localPosition;
    }

    /// <param name="shakeDuration">흔들리는 시간(초)</param>
    /// <param name="shakeMagnitude">흔들림 세기(월드 단위)</param>
    public void Shake(float shakeDuration, float shakeMagnitude)
    {
        duration = Mathf.Max(0.01f, shakeDuration);
        timeLeft = duration;
        magnitude = shakeMagnitude;
    }

    private void LateUpdate()
    {
        if (timeLeft <= 0f) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            transform.localPosition = originLocalPosition;   // 정확히 원위치로 복귀
            return;
        }

        // 시간이 지날수록 흔들림이 약해지도록 falloff 적용
        float falloff = timeLeft / duration;
        Vector2 shake = Random.insideUnitCircle * (magnitude * falloff);
        transform.localPosition = originLocalPosition + new Vector3(shake.x, shake.y, 0f);
    }
}
