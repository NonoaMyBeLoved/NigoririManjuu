using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Clamp markers")]
    public Transform leftX;
    public Transform rightX;

    [Header("Drop position")]
    public float topMargin = 0.6f;       // 화면 맨 위에서 얼마나 아래서 대기할지
    public float ceilingImmunity = 0.6f; // 드랍 직후 천장 무시 시간

    [Header("Spawn Weights")]
    [Tooltip("레벨 0~7까지의 등장 가중치 (float). 값이 클수록 잘 나옴.")]
    [SerializeField]
    float[] levelWeights =
        { 0.45f, 0.30f, 0.15f, 0.07f, 0.02f, 0.008f, 0.003f, 0.001f };

    [Tooltip("최대 스폰 레벨 상한. 예: 초반엔 3~4로 시작해서 점점 올리기")]
    [SerializeField][Range(0, 7)] int maxSpawnLevel = 7;

    // 내부 상태
    int current, next1, next2;
    float dropY;

    // 고스트(미리보기)
    GameObject ghost;
    SpriteRenderer ghostSR;

    void Start()
    {
        if (!Camera.main) { Debug.LogError("Main Camera 없음"); enabled = false; return; }
        if (GameManager.I == null || GameManager.I.piecePrefabs == null || GameManager.I.piecePrefabs.Length == 0)
        { Debug.LogError("GameManager 또는 piecePrefabs 미설정"); enabled = false; return; }

        // 큐 초기화
        current = RandomWeightedLevel();
        next1 = RandomWeightedLevel();
        next2 = RandomWeightedLevel();
        GameManager.I.SetPreview(next1, next2);

        // 고스트 생성 (SpriteRenderer만, 물리 없음)
        ghost = new GameObject("GhostPiece");
        ghost.transform.SetParent(transform);
        ghostSR = ghost.AddComponent<SpriteRenderer>();
        ghostSR.sortingLayerName = "Pieces"; // 없으면 Default도 OK
        ghostSR.sortingOrder = 999;
        var c = Color.white; c.a = 0.9f;
        ghostSR.color = c;

        UpdateGhost();
    }

    void Update()
    {
        RecalcDropY();

        // 마우스 X를 경계로 클램프 (+ 현재 만쥬 반지름 여유)
        Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float halfW = CurrentHalfWidth();
        float x = Mathf.Clamp(m.x, leftX.position.x + halfW, rightX.position.x - halfW);

        // 스포너/고스트 위치 갱신
        transform.position = new Vector3(x, dropY, 0);
        if (ghost) ghost.transform.position = transform.position;

        if (Input.GetMouseButtonDown(0) && GameManager.I.CanDrop())
            Drop();
    }

    void RecalcDropY()
    {
        dropY = Camera.main.orthographicSize - topMargin; // 항상 화면 안
    }

    void Drop()
    {
        // 진짜 만쥬 생성 (Pieces 아래)
        var prefab = GameManager.I.piecePrefabs[current];
        var go = Instantiate(prefab, transform.position, Quaternion.identity, GameManager.I.piecesParent);
        var p = go.GetComponent<Piece>();
        p.LockMerge(0.15f);
        p.ArmCeilingAfter(ceilingImmunity);

        // 큐 로테이트 + 프리뷰/고스트 갱신
        current = next1;
        next1 = next2;
        next2 = RandomWeightedLevel();

        GameManager.I.SetPreview(next1, next2);
        UpdateGhost();

        GameManager.I.NotifyDropped();
    }

    void UpdateGhost()
    {
        var prefab = GameManager.I.piecePrefabs[current];
        var sr = prefab.GetComponentInChildren<SpriteRenderer>();
        if (sr)
        {
            ghostSR.sprite = sr.sprite;
            // 프리팹 스케일을 그대로 반영
            ghost.transform.localScale = prefab.transform.localScale;
        }
    }

    float CurrentHalfWidth()
    {
        var prefab = GameManager.I.piecePrefabs[current];
        var sr = prefab.GetComponentInChildren<SpriteRenderer>();
        float baseHalf = sr ? sr.sprite.bounds.extents.x : 0.5f;
        return baseHalf * prefab.transform.localScale.x;
    }

    // ★ float 가중치 기반 랜덤 뽑기
    int RandomWeightedLevel()
    {
        // piecePrefabs 길이와 weights 길이 중 작은 쪽까지 사용
        int len = GameManager.I?.piecePrefabs?.Length ?? levelWeights.Length;
        len = Mathf.Min(len, levelWeights.Length);

        // 상한 캡
        int cap = Mathf.Clamp(maxSpawnLevel, 0, len - 1);

        float sum = 0f;
        for (int i = 0; i <= cap; i++)
            sum += Mathf.Max(0f, levelWeights[i]);

        if (sum <= 0f) return 0;

        float r = Random.value * sum;
        float acc = 0f;
        for (int i = 0; i <= cap; i++)
        {
            acc += Mathf.Max(0f, levelWeights[i]);
            if (r < acc) return i;
        }
        return 0;
    }

    // 난이도 올리기용(원하면 사용): 런타임에 상한 조절
    public void SetMaxSpawnLevel(int lv)
    {
        maxSpawnLevel = Mathf.Clamp(lv, 0, (GameManager.I?.piecePrefabs?.Length ?? 8) - 1);
    }
}
