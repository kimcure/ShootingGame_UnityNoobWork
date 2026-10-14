/// <summary>
/// 게임 전체에서 공유하는 열거형 모음.
/// 한 파일에 모아두면 "이 게임에 어떤 종류가 있는지"를 한눈에 볼 수 있다.
/// </summary>

/// <summary>누구 편인가? (미사일/HP가 아군을 때리지 않도록 구분하는 데 사용)</summary>
public enum Team
{
    Player,
    Enemy
}

/// <summary>게임 진행 상태.</summary>
public enum GameState
{
    Playing,
    GameOver,
    StageClear
}

/// <summary>적 종류. EnemyData(ScriptableObject)와 WaveGroup이 이 값으로 서로를 연결한다.</summary>
public enum EnemyKind
{
    Scout,   // 직진하는 기본 적
    Weaver,  // 사인파로 흔들리며 내려오는 적
    Gunner,  // 멈추지 않고 내려오며 플레이어를 조준 사격
    Chaser   // 플레이어를 추적하는 적
}

/// <summary>적의 이동 방식.</summary>
public enum MovePattern
{
    Straight,
    Sine,
    Zigzag,
    Chase
}

/// <summary>적이 스폰될 때 만드는 "대열" 모양.</summary>
public enum FormationType
{
    Line,      // 가로 일렬   ■ ■ ■ ■ ■
    Column,    // 세로 일렬   (위로 길게)
    V,         // V자         꼭짓점이 아래(먼저 도착)
    InvertedV, // 역V자(∧)    꼭짓점이 위(나중에 도착)
    Grid,      // 격자
    Circle,    // 원형 링
    Zigzag     // 지그재그
}
