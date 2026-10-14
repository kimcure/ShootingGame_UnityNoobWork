using UnityEngine;

/// <summary>
/// HPController의 수치를 "머리 위 체력바"로 보여주는 View. (HP는 숫자, HPBar는 그 숫자를 보여주는 UI)
///
/// Canvas/Slider 대신 SpriteRenderer 2장(배경 + 채움)으로 만든다.
///  - World Space Canvas의 좌표계/SetParent/Scale 문제(5강의 "HPBar 난리" 원인)를 아예 피할 수 있다.
///  - 하얀 1x1 스프라이트(hp_pixel) 하나만 연결하면 된다. 배경/채움 오브젝트는 Awake에서 코드로 만든다.
///  - 부모(적)의 자식으로 들어가므로 적을 따라 움직이고, 풀에서 꺼질 때 함께 꺼진다.
/// </summary>
[RequireComponent(typeof(HPController))]
public class HPBarView : MonoBehaviour
{
    [Header("Art")]
    [Tooltip("하얀색 네모 스프라이트 (Art/hp_pixel). 색은 코드에서 입힌다.")]
    [SerializeField] private Sprite pixelSprite;

    [Header("Layout")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 0.7f);
    [SerializeField, Min(0.1f)] private float width = 0.9f;
    [SerializeField, Min(0.02f)] private float height = 0.12f;
    [SerializeField] private int sortingOrder = 20;

    [Header("Color")]
    [SerializeField] private Color backColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private Color fullColor = new Color(0.3f, 1f, 0.4f);
    [SerializeField] private Color lowColor = new Color(1f, 0.25f, 0.25f);

    [Tooltip("체력이 가득 찼을 때는 숨긴다 (적에게 유용).")]
    [SerializeField] private bool hideWhenFull = false;

    private HPController hp;
    private Transform root;
    private Transform fillTransform;
    private SpriteRenderer fillRenderer;
    private Vector2 spriteSize;

    private void Awake()
    {
        hp = GetComponent<HPController>();

        if (pixelSprite == null)
        {
            Debug.LogError("HPBarView:: pixelSprite is null  (Art/hp_pixel 을 연결하세요)");
            enabled = false;
            return;
        }

        spriteSize = pixelSprite.bounds.size;

        BuildBar();
        hp.HPChanged += Refresh;
        Refresh(hp.HP, hp.MaxHP);
    }

    private void OnDestroy()
    {
        if (hp != null) hp.HPChanged -= Refresh;
    }

    private void BuildBar()
    {
        root = new GameObject("HPBar").transform;
        root.SetParent(transform, false);   // false: 부모의 로컬 좌표계로 넣는다 (5강: SetParent 두 번째 인자)
        root.localPosition = offset;

        CreateRenderer("Back", sortingOrder, backColor, out Transform backTransform);
        backTransform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);

        fillRenderer = CreateRenderer("Fill", sortingOrder + 1, fullColor, out fillTransform);
    }

    private SpriteRenderer CreateRenderer(string objectName, int order, Color color, out Transform created)
    {
        GameObject go = new GameObject(objectName);
        created = go.transform;
        created.SetParent(root, false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = pixelSprite;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    private void Refresh(float current, float max)
    {
        if (fillTransform == null) return;

        float ratio = max <= 0f ? 0f : Mathf.Clamp01(current / max);

        // 왼쪽 끝을 고정한 채 오른쪽이 줄어들도록: 폭을 줄이고, 줄어든 만큼 중심을 왼쪽으로 민다.
        float fillWidth = width * ratio;
        fillTransform.localScale = new Vector3(fillWidth / spriteSize.x, height / spriteSize.y, 1f);
        fillTransform.localPosition = new Vector3(-(width - fillWidth) * 0.5f, 0f, 0f);

        fillRenderer.color = Color.Lerp(lowColor, fullColor, ratio);

        root.gameObject.SetActive(!(hideWhenFull && ratio >= 1f));
    }
}
