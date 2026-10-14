using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 "입력과 이동" 책임.
///
///  - 입력: PlayerInput 컴포넌트(Invoke Unity Events)가 OnMove / OnFire를 호출한다. (수업의 Input System 방식 그대로)
///  - 이동: Rigidbody2D.MovePosition + FixedUpdate + fixedDeltaTime  (수업 규칙 2, 3)
///          현재 위치는 transform이 아니라 rigidbody.position에서 읽는다.
///  - 화면 밖으로 못 나가도록 ScreenWorldBounds.ClampPosition으로 제한한다.
///  - 발사는 PlayerWeapon, 체력은 HPController가 맡는다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(HPController), typeof(PlayerWeapon))]
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Range(1f, 15f)] private float moveSpeed = 7f;
    [SerializeField, Min(1f)] private float maxHP = 5f;
    [Tooltip("화면 가장자리에서 기체 크기만큼 안쪽으로 여유")]
    [SerializeField, Range(0f, 2f)] private float screenPadding = 0.5f;
    [Tooltip("시작 위치: 화면 아래에서 이만큼 위")]
    [SerializeField, Range(0f, 5f)] private float startHeightFromBottom = 1.5f;

    public HPController Hp { get; private set; }
    public PlayerWeapon Weapon { get; private set; }

    private Rigidbody2D rb;
    private Vector2 moveInput = Vector2.zero;
    private bool canControl = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Hp = GetComponent<HPController>();
        Weapon = GetComponent<PlayerWeapon>();

        // 프리팹 설정 실수를 막는 안전장치: 중력 없음, 회전 고정
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        Hp.Configure(Team.Player, maxHP);
    }

    private void Start()
    {
        // 화면 아래 중앙에서 시작
        Vector2 startPosition = new Vector2(0f, ScreenWorldBounds.MinY + startHeightFromBottom);
        transform.position = startPosition;
        rb.position = startPosition;
    }

    private void FixedUpdate()
    {
        if (!canControl) return;

        Vector2 next = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(ScreenWorldBounds.ClampPosition(next, screenPadding));
    }

    // ───────── PlayerInput(Unity Events)에서 연결하는 콜백 ─────────

    /// <summary>Move 액션 (WASD / 방향키 / 게임패드 스틱).</summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        // started는 "입력이 시작됐다"는 알림일 뿐 값이 아직 없으므로 건너뛴다 (수업 코드와 동일).
        // 키를 떼면 canceled가 오고 ReadValue는 (0,0)이므로 자연스럽게 멈춘다.
        if (context.started) return;

        moveInput = canControl ? context.ReadValue<Vector2>() : Vector2.zero;
    }

    /// <summary>Attack(Fire) 액션. 누르는 동안 연사, 떼면 중지.</summary>
    public void OnFire(InputAction.CallbackContext context)
    {
        if (!canControl) return;

        if (context.performed)
        {
            Weapon.SetFiring(true);
        }
        else if (context.canceled)
        {
            Weapon.SetFiring(false);
        }
    }

    /// <summary>게임오버/클리어 시 조작을 막는다.</summary>
    public void SetControllable(bool controllable)
    {
        canControl = controllable;

        if (!controllable)
        {
            moveInput = Vector2.zero;
            Weapon.SetFiring(false);
        }
    }
}
