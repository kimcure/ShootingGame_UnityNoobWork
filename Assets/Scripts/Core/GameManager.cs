using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체의 진행(상태, 점수, 시작/종료/재시작)을 책임지는 중앙 관리자.
///
/// GameManager는 "누가 무엇을 하는지"는 모르고, 각 시스템이 보내는 이벤트에만 반응해 연결해 주는 역할이다.
///
///   EnemyManager.EnemyKilled   → 점수 증가, 무기 레벨 갱신
///   EnemyManager.WaveChanged   → 웨이브 표시
///   EnemyManager.AllWavesCleared → STAGE CLEAR
///   Player HPController.HPChanged → HP 표시
///   Player HPController.Damaged   → 카메라 흔들림
///   Player HPController.Died      → GAME OVER
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private EffectManager effectManager;
    [SerializeField] private HudView hud;
    [SerializeField] private CameraShake cameraShake;

    [Header("Messages")]
    [Tooltip("WAVE n 안내 문구가 화면에 머무는 전체 시간(초). 나타남 + 유지 + 사라짐을 모두 포함")]
    [SerializeField, Range(1f, 6f)] private float waveMessageSeconds = 3f;

    [Header("Weapon Upgrade")]
    [Tooltip("이 점수에 도달할 때마다 무기 레벨이 1씩 오른다. (인덱스 0 = Lv1)")]
    [SerializeField] private int[] weaponLevelScores = { 0, 1500, 4000 };

    private GameState state = GameState.Playing;
    private int score;
    private int currentWeaponLevel = 1;
    private HPController playerHp;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        // 화면 경계를 가장 먼저 계산해 둔다 (플레이어 이동 제한, 적 소환 위치, 미사일 화면 이탈 판정에 모두 사용)
        ScreenWorldBounds.Initialize(mainCamera);
    }

    private void Start()
    {
        playerHp = player.Hp;

        // 이벤트 연결
        enemyManager.EnemyKilled += HandleEnemyKilled;
        enemyManager.WaveChanged += HandleWaveChanged;
        enemyManager.AllWavesCleared += HandleAllWavesCleared;
        playerHp.HPChanged += HandlePlayerHPChanged;
        playerHp.Damaged += HandlePlayerDamaged;
        playerHp.Died += HandlePlayerDied;

        // 초기 HUD
        hud.SetScore(score);
        hud.SetHP(playerHp.HP, playerHp.MaxHP);
        player.Weapon.SetLevel(currentWeaponLevel);

        enemyManager.StartWaves();
    }

    private void OnDestroy()
    {
        if (enemyManager != null)
        {
            enemyManager.EnemyKilled -= HandleEnemyKilled;
            enemyManager.WaveChanged -= HandleWaveChanged;
            enemyManager.AllWavesCleared -= HandleAllWavesCleared;
        }

        if (playerHp != null)
        {
            playerHp.HPChanged -= HandlePlayerHPChanged;
            playerHp.Damaged -= HandlePlayerDamaged;
            playerHp.Died -= HandlePlayerDied;
        }
    }

    private void Update()
    {
        // 게임이 끝난 뒤 R 또는 Enter로 재시작
        if (state != GameState.Playing && IsRestartPressed())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // ───────── 이벤트 핸들러 ─────────

    private void HandleEnemyKilled(int points)
    {
        if (state != GameState.Playing) return;

        score += points;
        hud.SetScore(score);
        UpdateWeaponLevel();
    }

    private void HandleWaveChanged(int current, int total)
    {
        if (state != GameState.Playing) return;

        hud.SetWave(current, total);
        hud.FlashMessage($"WAVE {current}", waveMessageSeconds);
    }

    private void HandleAllWavesCleared()
    {
        if (state != GameState.Playing) return;

        state = GameState.StageClear;
        player.SetControllable(false);
        hud.ShowMessage($"STAGE CLEAR!\nSCORE {score:N0}\n\nPress R to Restart");
    }

    private void HandlePlayerHPChanged(float current, float max)
    {
        hud.SetHP(current, max);
    }

    private void HandlePlayerDamaged(float damage)
    {
        cameraShake.Shake(0.25f, 0.25f);
    }

    private void HandlePlayerDied()
    {
        if (state != GameState.Playing) return;

        state = GameState.GameOver;
        enemyManager.StopWaves();
        player.SetControllable(false);

        // 플레이어 폭발 연출 후 기체를 숨긴다
        effectManager.PlayExplosion(player.transform.position, 2f);
        player.gameObject.SetActive(false);

        hud.ShowMessage($"GAME OVER\nSCORE {score:N0}\n\nPress R to Restart");
    }

    // ───────── 내부 로직 ─────────

    /// <summary>점수에 맞춰 무기 레벨 갱신. 레벨이 오르면 안내 메시지를 띄운다.</summary>
    private void UpdateWeaponLevel()
    {
        int level = 1;
        for (int i = 0; i < weaponLevelScores.Length; i++)
        {
            if (score >= weaponLevelScores[i]) level = i + 1;
        }

        if (level == currentWeaponLevel) return;

        currentWeaponLevel = level;
        player.Weapon.SetLevel(level);
        hud.FlashMessage("WEAPON UP!", 1.0f);
    }

    private static bool IsRestartPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.rKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame);
    }

    private bool ValidateReferences()
    {
        bool valid = true;

        if (mainCamera == null) { Debug.LogError("GameManager:: mainCamera is null"); valid = false; }
        if (player == null) { Debug.LogError("GameManager:: player is null"); valid = false; }
        if (enemyManager == null) { Debug.LogError("GameManager:: enemyManager is null"); valid = false; }
        if (effectManager == null) { Debug.LogError("GameManager:: effectManager is null"); valid = false; }
        if (hud == null) { Debug.LogError("GameManager:: hud is null"); valid = false; }
        if (cameraShake == null) { Debug.LogError("GameManager:: cameraShake is null"); valid = false; }

        return valid;
    }
}