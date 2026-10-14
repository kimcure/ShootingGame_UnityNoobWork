using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 고정 크기 Object Pool. 적/미사일/이펙트가 모두 이 클래스 하나를 재사용한다.
///
/// 동작 흐름 (수업의 Ready → Fire → Flying → ReturnToPool 과 같다)
///   생성자 : 미리 N개 Instantiate → SetActive(false) → free 큐에 보관
///   TryGet : free 큐에서 하나 꺼내 InUse = true, SetActive(true)
///   Return : 사용이 끝난 오브젝트를 다시 free 큐에 넣는다 (PooledObject.ReturnToPool이 호출)
///
/// 풀이 비면 새로 만들지 않고 false를 돌려준다 (필요하면 호출한 쪽이 기다리거나 포기).
/// </summary>
public class SimplePool<T> : IPoolOwner where T : PooledObject
{
    private readonly List<T> all;
    private readonly Queue<T> free;

    /// <summary>풀이 만든 전체 개수.</summary>
    public int TotalCount => all.Count;

    /// <summary>현재 사용 중(화면에 살아 있는) 개수.</summary>
    public int ActiveCount => all.Count - free.Count;

    /// <summary>풀이 만든 모든 오브젝트 (초기 설정이나 이벤트 구독에 사용).</summary>
    public IReadOnlyList<T> Items => all;

    public SimplePool(T prefab, int size, Transform parent)
    {
        all = new List<T>(size);
        free = new Queue<T>(size);

        for (int i = 0; i < size; i++)
        {
            // MonoBehaviour가 아닌 클래스에서는 UnityEngine.Object를 명시해야 한다.
            T item = UnityEngine.Object.Instantiate(prefab, parent);
            item.name = $"{prefab.name}_{i:00}";
            item.Bind(this);
            item.gameObject.SetActive(false);

            all.Add(item);
            free.Enqueue(item);
        }
    }

    /// <summary>사용 가능한 오브젝트를 꺼낸다. 이미 SetActive(true) 상태로 나온다.</summary>
    public bool TryGet(out T item)
    {
        if (free.Count == 0)
        {
            item = null;
            return false;
        }

        item = free.Dequeue();
        item.MarkInUse();
        item.gameObject.SetActive(true);
        return true;
    }

    void IPoolOwner.Return(PooledObject item)
    {
        free.Enqueue((T)item);
    }
}
