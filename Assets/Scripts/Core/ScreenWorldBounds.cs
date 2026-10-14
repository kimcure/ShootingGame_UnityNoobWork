using UnityEngine;

/// <summary>
/// 카메라 화면의 월드 좌표 경계를 저장/제공하는 정적 유틸리티 (수업 자료의 ScreenWorldBounds를 이 프로젝트에 맞게 줄인 버전).
///
/// 사용처
///  - 플레이어 화면 밖 이탈 방지 : ClampPosition
///  - 미사일/적이 화면을 벗어났는가 : IsOutside / IsOutBottom
///  - 적 스폰 위치 계산 : MinX / MaxX / MaxY
/// </summary>
public static class ScreenWorldBounds
{
    private static Camera targetCamera;
    private static bool initialized;

    private static float minX;
    private static float maxX;
    private static float minY;
    private static float maxY;

    public static float MinX { get { EnsureInitialized(); return minX; } }
    public static float MaxX { get { EnsureInitialized(); return maxX; } }
    public static float MinY { get { EnsureInitialized(); return minY; } }
    public static float MaxY { get { EnsureInitialized(); return maxY; } }
    public static float Width => MaxX - MinX;
    public static float Height => MaxY - MinY;

    /// <summary>
    /// "Enter Play Mode Options"로 도메인 리로드를 끈 경우에도 이전 플레이의 static 값이 남지 않도록 초기화.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        targetCamera = null;
        initialized = false;
    }

    /// <summary>GameManager.Awake에서 한 번 호출한다.</summary>
    public static void Initialize(Camera camera)
    {
        if (camera == null)
        {
            Debug.LogError("ScreenWorldBounds:: Initialize failed - camera is null");
            return;
        }

        targetCamera = camera;
        Refresh();
        initialized = true;
    }

    /// <summary>카메라 위치/크기가 바뀌었을 때 다시 계산.</summary>
    public static void Refresh()
    {
        if (targetCamera == null) return;

        // 2D 게임: 모든 오브젝트가 z = 0 평면에 있다고 가정
        float distance = -targetCamera.transform.position.z;

        Vector3 bottomLeft = targetCamera.ViewportToWorldPoint(new Vector3(0f, 0f, distance));
        Vector3 topRight = targetCamera.ViewportToWorldPoint(new Vector3(1f, 1f, distance));

        minX = bottomLeft.x;
        minY = bottomLeft.y;
        maxX = topRight.x;
        maxY = topRight.y;
    }

    /// <summary>padding &gt; 0 이면 화면보다 조금 바깥까지 "안"으로 취급한다.</summary>
    public static bool IsInside(Vector2 position, float padding = 0f)
    {
        EnsureInitialized();
        return position.x >= minX - padding && position.x <= maxX + padding &&
               position.y >= minY - padding && position.y <= maxY + padding;
    }

    public static bool IsOutside(Vector2 position, float padding = 0f)
    {
        return !IsInside(position, padding);
    }

    public static bool IsOutBottom(Vector2 position, float padding = 0f)
    {
        EnsureInitialized();
        return position.y < minY - padding;
    }

    /// <summary>위치를 화면 안쪽으로 제한한다. padding만큼 화면 안쪽으로 여유를 둔다.</summary>
    public static Vector2 ClampPosition(Vector2 position, float padding = 0f)
    {
        EnsureInitialized();
        position.x = Mathf.Clamp(position.x, minX + padding, maxX - padding);
        position.y = Mathf.Clamp(position.y, minY + padding, maxY - padding);
        return position;
    }

    // 초기화가 안 되어 있으면 Camera.main으로 자동 초기화 (실행 순서에 상관없이 안전하게 쓰기 위함)
    private static void EnsureInitialized()
    {
        if (initialized) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ScreenWorldBounds:: not initialized and Camera.main is null");
            return;
        }

        Initialize(mainCamera);
    }
}
