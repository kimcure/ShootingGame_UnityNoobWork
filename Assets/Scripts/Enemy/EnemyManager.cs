using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적들의 "생성(Object Pool)과 소환 타이밍"을 책임지는 관리자.
///
///  - Awake에서 적 N마리를 미리 만들어 풀에 보관한다 (Object Pooling).
///  - WaveGroup 목록을 순서대로 읽으며 FormationFactory로 대열 좌표를 계산해 소환한다.
///  - 적이 죽으면(EnemyController.Killed) 점수 이벤트를 GameManager에 전달한다.
///  - 모든 그룹을 소환하고 화면의 적이 다 사라지면 AllWavesCleared 이벤트를 발생시킨다.
///
/// EnemyManager는 "언제 누구를 소환할지"만 알고,
/// 적이 어떻게 움직이고 쏘는지는 EnemyController가 안다.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("Prefab / Pool")]
    [SerializeField] private EnemyController enemyPrefab;
    [Tooltip("동시에 화면에 존재할 수 있는 최대 적 수 (풀 크기)")]
    [SerializeField, Range(10, 100)] private int poolSize = 40;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private MissileManager missileManager;
    [SerializeField] private EffectManager effectManager;

    [Header("Enemy Data")]
    [Tooltip("적 종류별 EnemyData. WaveGroup의 enemyKind와 연결된다")]
    [SerializeField] private EnemyData[] enemyDatas;

    [Header("Spawn Area")]
    [Tooltip("화면 위쪽 경계에서 이만큼 더 위에서 등장")]
    [SerializeField, Range(0.5f, 5f)] private float spawnMarginY = 1.5f;
    [Tooltip("화면 좌우 가장자리에서 이만큼 안쪽까지만 대열 중심을 둔다")]
    [SerializeField, Range(0f, 3f)] private float spawnMarginX = 1.2f;

    [Header("Wave Pacing")]
    [Tooltip("true면 화면의 적이 모두 사라진 뒤에야 다음 웨이브를 시작한다 (웨이브가 겹치지 않아 가장 느긋한 진행)")]
    [SerializeField] private bool waitUntilFieldClear = true;
    [Tooltip("모든 delayBefore(대기 시간)에 곱하는 배율. 2면 대기 시간이 두 배")]
    [SerializeField, Range(0.5f, 5f)] private float delayMultiplier = 1.5f;

    [Header("Waves")]
    [SerializeField] private List<WaveGroup> waves = WaveGroup.CreateDefaultWaves();

    /// <summary>플레이어가 적을 처치했을 때 (획득 점수).</summary>
    public event Action<int> EnemyKilled;

    /// <summary>다음 그룹으로 넘어갈 때 (현재 번호, 전체 개수). 1부터 시작.</summary>
    public event Action<int, int> WaveChanged;

    /// <summary>모든 그룹을 소환하고 적도 모두 사라졌을 때.</summary>
    public event Action AllWavesCleared;

    private SimplePool<EnemyController> enemyPool;
    private readonly Dictionary<EnemyKind, EnemyData> dataByKind = new Dictionary<EnemyKind, EnemyData>();
    private Coroutine waveRoutine;

    private void Awake()
    {
        if (!ValidateReferences()) return;

        // 종류(EnemyKind) → 데이터 조회용 사전
        foreach (EnemyData data in enemyDatas)
        {
            if (data == null) continue;
            dataByKind[data.kind] = data;
        }

        // 적 Object Pool 생성. Hierarchy 정리를 위해 이 오브젝트(EnemyManager)의 자식으로 모은다
        enemyPool = new SimplePool<EnemyController>(enemyPrefab, poolSize, transform);

        // 풀이 만든 적들에게 필요한 참조를 한 번씩 나눠주고, 처치 이벤트를 구독한다
        foreach (EnemyController enemy in enemyPool.Items)
        {
            enemy.Initialize(player, missileManager, effectManager);
            enemy.Killed += HandleEnemyKilled;
        }
    }

    private void OnDestroy()
    {
        if (enemyPool == null) return;

        foreach (EnemyController enemy in enemyPool.Items)
        {
            if (enemy != null) enemy.Killed -= HandleEnemyKilled;
        }
    }

    /// <summary>GameManager가 게임 시작 시 호출.</summary>
    public void StartWaves()
    {
        if (enemyPool == null) return;

        StopWaves();
        waveRoutine = StartCoroutine(RunWaves());
    }

    /// <summary>게임오버 등으로 더 이상 소환하지 않을 때.</summary>
    public void StopWaves()
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }
    }

    private IEnumerator RunWaves()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            // 이전 웨이브의 적이 아직 남아 있으면 다 사라질 때까지 기다린다 (처치되거나 화면 아래로 지나가거나)
            if (waitUntilFieldClear)
            {
                while (enemyPool.ActiveCount > 0)
                {
                    yield return null;
                }
            }

            WaveChanged?.Invoke(i + 1, waves.Count);

            yield return new WaitForSeconds(waves[i].delayBefore * delayMultiplier);
            yield return StartCoroutine(SpawnGroup(waves[i]));
        }

        // 마지막 그룹까지 소환 끝 → 화면의 적이 모두 사라질 때까지(처치되거나 지나가거나) 기다린다
        while (enemyPool.ActiveCount > 0)
        {
            yield return null;
        }

        waveRoutine = null;
        AllWavesCleared?.Invoke();
    }

    private IEnumerator SpawnGroup(WaveGroup group)
    {
        if (!dataByKind.TryGetValue(group.enemyKind, out EnemyData data))
        {
            Debug.LogError($"EnemyManager:: EnemyData for {group.enemyKind} is missing  (enemyDatas 배열에 추가하세요)");
            yield break;
        }

        // 1) 대열 모양 계산: 기준점으로부터의 상대 좌표들
        Vector2[] offsets = FormationFactory.GetOffsets(group.formation, group.count, group.spacing, group.columns);

        // 2) 기준점 계산: spawnX(-1~1)를 실제 화면 좌표로 변환하고, y는 화면 위쪽 바깥
        float minX = ScreenWorldBounds.MinX + spawnMarginX;
        float maxX = ScreenWorldBounds.MaxX - spawnMarginX;
        float originX = Mathf.Lerp(minX, maxX, (group.spawnX + 1f) * 0.5f);
        float originY = ScreenWorldBounds.MaxY + spawnMarginY;
        Vector2 origin = new Vector2(originX, originY);

        // 3) 기준점 + 상대 좌표 위치에 한 마리씩 소환
        for (int i = 0; i < offsets.Length; i++)
        {
            EnemyController enemy;

            // 풀이 비어 있으면 (아직 안 죽은 적이 너무 많으면) 자리가 날 때까지 기다린다
            while (!enemyPool.TryGet(out enemy))
            {
                yield return null;
            }

            enemy.Spawn(data, origin + offsets[i]);

            if (group.spawnInterval > 0f)
            {
                yield return new WaitForSeconds(group.spawnInterval);
            }
        }
    }

    private void HandleEnemyKilled(EnemyController enemy)
    {
        EnemyKilled?.Invoke(enemy.Data.scoreValue);
    }

    private bool ValidateReferences()
    {
        bool valid = true;

        if (enemyPrefab == null) { Debug.LogError("EnemyManager:: enemyPrefab is null"); valid = false; }
        if (player == null) { Debug.LogError("EnemyManager:: player is null"); valid = false; }
        if (missileManager == null) { Debug.LogError("EnemyManager:: missileManager is null"); valid = false; }
        if (effectManager == null) { Debug.LogError("EnemyManager:: effectManager is null"); valid = false; }
        if (enemyDatas == null || enemyDatas.Length == 0) { Debug.LogError("EnemyManager:: enemyDatas is empty"); valid = false; }

        return valid;
    }
}