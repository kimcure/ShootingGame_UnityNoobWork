using System;
using UnityEngine;

/// <summary>
/// 스프라이트 프레임을 순서대로 넘기는 간단한 코드 애니메이션 ("아트 작업 연동"의 핵심 컴포넌트).
///
/// Animator/AnimationClip 없이 Sprite 배열만 연결하면 되므로, 아티스트가 그린 프레임을 그대로 끼워 넣을 수 있다.
///  - 적 날개짓, 플레이어 엔진 불꽃 : loop = true
///  - 폭발 : loop = false (끝나면 Completed 이벤트 → 풀로 반납)
///
/// 활성화(OnEnable)될 때마다 처음 프레임부터 다시 재생하므로 Object Pool에서 재사용돼도 안전하다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipbook : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(1f)] private float framesPerSecond = 8f;
    [SerializeField] private bool loop = true;

    /// <summary>loop = false 일 때 마지막 프레임까지 재생이 끝나면 한 번 호출.</summary>
    public event Action Completed;

    private SpriteRenderer spriteRenderer;
    private int frameIndex;
    private float timer;
    private bool playing;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        Play();
    }

    /// <summary>프레임 목록을 교체하고 처음부터 재생한다 (적 종류가 바뀔 때 사용).</summary>
    public void SetFrames(Sprite[] newFrames, float fps)
    {
        frames = newFrames;
        if (fps > 0f) framesPerSecond = fps;
        Play();
    }

    /// <summary>처음 프레임부터 재생.</summary>
    public void Play()
    {
        frameIndex = 0;
        timer = 0f;
        playing = frames != null && frames.Length > 0;

        if (playing && spriteRenderer != null)
        {
            spriteRenderer.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (!playing) return;

        timer += Time.deltaTime;
        float frameTime = 1f / framesPerSecond;

        // 프레임이 낮을 때도 놓치지 않도록 while로 따라잡는다
        while (timer >= frameTime)
        {
            timer -= frameTime;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                if (loop)
                {
                    frameIndex = 0;
                }
                else
                {
                    frameIndex = frames.Length - 1;
                    playing = false;
                    Completed?.Invoke();
                    return;
                }
            }

            spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
