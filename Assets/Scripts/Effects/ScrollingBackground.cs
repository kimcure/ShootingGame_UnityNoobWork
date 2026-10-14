using UnityEngine;

/// <summary>
/// 세로로 무한 스크롤되는 배경 레이어.
/// 같은 타일 2장을 위아래로 이어 붙여 내려보내고, 화면 아래로 완전히 빠진 타일을 맨 위로 되돌려 이어 붙인다.
///
/// 레이어를 2개(먼 별: 느리게 / 가까운 별: 빠르게) 만들면 시차(Parallax)로 속도감과 깊이감이 생긴다.
/// 타일 스프라이트는 위아래로 이어 붙여도 티가 나지 않는(seamless) 이미지여야 한다.
/// </summary>
public class ScrollingBackground : MonoBehaviour
{
    [Header("Art")]
    [SerializeField] private Sprite tileSprite;
    [SerializeField] private int sortingOrder = -20;

    [Header("Scroll")]
    [Tooltip("아래로 내려가는 속도 (월드 단위/초)")]
    [SerializeField, Range(0f, 10f)] private float scrollSpeed = 1f;

    private readonly Transform[] tiles = new Transform[2];
    private float tileHeight;

    private void Awake()
    {
        if (tileSprite == null)
        {
            Debug.LogError("ScrollingBackground:: tileSprite is null");
            enabled = false;
            return;
        }

        tileHeight = tileSprite.bounds.size.y;

        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = new GameObject($"Tile_{i}");
            tile.transform.SetParent(transform, false);
            tile.transform.localPosition = new Vector3(0f, i * tileHeight, 0f);

            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = tileSprite;
            sr.sortingOrder = sortingOrder;

            tiles[i] = tile.transform;
        }
    }

    private void Update()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            Vector3 position = tiles[i].localPosition;
            position.y -= scrollSpeed * Time.deltaTime;

            // 타일이 화면 아래로 완전히 빠졌으면 (타일 2장 높이만큼) 위로 올려 이어 붙인다
            if (position.y <= -tileHeight)
            {
                position.y += tileHeight * tiles.Length;
            }

            tiles[i].localPosition = position;
        }
    }
}
