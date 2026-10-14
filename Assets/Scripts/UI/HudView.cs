using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 화면 UI(점수, HP, 웨이브, 중앙 메시지) 표시 전용 View.
/// 게임 규칙은 전혀 모르고, GameManager가 시키는 대로 글자만 바꾼다.
/// </summary>
public class HudView : MonoBehaviour
{
    [Header("Texts (TextMeshPro)")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text waveText;
    [Tooltip("화면 중앙의 큰 메시지 (WAVE 안내 / GAME OVER / STAGE CLEAR)")]
    [SerializeField] private TMP_Text messageText;

    [Header("Message Fade")]
    [Tooltip("FlashMessage가 서서히 나타나고 사라지는 데 걸리는 시간(초)")]
    [SerializeField, Range(0.1f, 2f)] private float fadeTime = 0.6f;

    private Coroutine flashRoutine;

    private void Awake()
    {
        if (scoreText == null || hpText == null || waveText == null || messageText == null)
        {
            Debug.LogError("HudView:: TMP text reference is missing  (4개 텍스트를 모두 연결하세요)");
        }

        HideMessage();
    }

    public void SetScore(int score)
    {
        SetText(scoreText, $"SCORE {score:N0}");
    }

    public void SetHP(float current, float max)
    {
        SetText(hpText, $"HP {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}");
    }

    public void SetWave(int current, int total)
    {
        SetText(waveText, $"WAVE {current} / {total}");
    }

    /// <summary>중앙 메시지를 계속 표시 (GAME OVER 등).</summary>
    public void ShowMessage(string message)
    {
        StopFlash();
        SetText(messageText, message);
        if (messageText != null)
        {
            messageText.alpha = 1f;
            messageText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 중앙 메시지를 서서히 나타냈다가 일정 시간 보여주고 서서히 사라지게 한다 (WAVE 시작 안내 등).
    /// seconds는 나타나는 시간 + 유지 시간 + 사라지는 시간을 모두 합친 전체 표시 시간이다.
    /// </summary>
    public void FlashMessage(string message, float seconds)
    {
        if (messageText == null) return;

        StopFlash();
        SetText(messageText, message);
        messageText.alpha = 0f;
        messageText.gameObject.SetActive(true);
        flashRoutine = StartCoroutine(FadeRoutine(seconds));
    }

    public void HideMessage()
    {
        StopFlash();
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    private IEnumerator FadeRoutine(float totalSeconds)
    {
        // 전체 시간이 너무 짧아도 페이드 두 번이 겹치지 않도록 제한
        float fade = Mathf.Min(fadeTime, totalSeconds * 0.4f);
        float hold = Mathf.Max(0f, totalSeconds - fade * 2f);

        // 서서히 나타남
        for (float t = 0f; t < fade; t += Time.deltaTime)
        {
            messageText.alpha = t / fade;
            yield return null;
        }

        messageText.alpha = 1f;
        yield return new WaitForSeconds(hold);

        // 서서히 사라짐
        for (float t = 0f; t < fade; t += Time.deltaTime)
        {
            messageText.alpha = 1f - t / fade;
            yield return null;
        }

        flashRoutine = null;
        messageText.gameObject.SetActive(false);
    }

    private void StopFlash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null) target.text = value;
    }
}