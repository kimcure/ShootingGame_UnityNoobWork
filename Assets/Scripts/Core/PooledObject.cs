using UnityEngine;

/// <summary>
/// 풀(Pool)이 "돌려받을 때" 호출하는 창구.
/// PooledObject는 자기가 어느 풀 소속인지 몰라도 이 인터페이스만 알면 된다.
/// </summary>
public interface IPoolOwner
{
    void Return(PooledObject item);
}

/// <summary>
/// Object Pooling 대상(적, 미사일, 이펙트)의 공통 부모.
///
/// 수업에서 배운 교훈을 코드로 고정해 둔 클래스:
///  1) 상태(InUse)는 "우리가 설계한 게임 상태"이며 active/enabled와는 별개다.
///  2) 비활성화(SetActive(false))는 값을 초기화하지 않으므로, 재사용될 때 Spawn/Launch/Play 쪽에서 상태를 직접 초기화한다.
///  3) 풀 오브젝트는 Start에서 SetActive(false)를 하지 않는다 (풀이 생성 시점에 비활성화를 담당).
///  4) Destroy 대신 ReturnToPool()을 쓴다.
/// </summary>
public abstract class PooledObject : MonoBehaviour
{
    private IPoolOwner owner;

    /// <summary>지금 풀 밖에서 사용 중인가? (= 화면에서 살아 있는가)</summary>
    public bool InUse { get; private set; }

    /// <summary>풀이 오브젝트를 만든 직후 소속을 알려준다.</summary>
    internal void Bind(IPoolOwner poolOwner)
    {
        owner = poolOwner;
    }

    /// <summary>풀이 꺼내 줄 때 호출한다.</summary>
    internal void MarkInUse()
    {
        InUse = true;
    }

    /// <summary>
    /// 사용이 끝나면 호출. 숨기고(SetActive false) 풀에 반납한다.
    /// 이미 반납된 상태에서 또 호출돼도 안전하다 (충돌 콜백이 한 프레임에 여러 번 와도 중복 반납 방지).
    /// </summary>
    public void ReturnToPool()
    {
        if (!InUse) return;

        InUse = false;
        gameObject.SetActive(false);
        owner?.Return(this);
    }
}
