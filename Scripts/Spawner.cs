using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Clamp markers")]
    public Transform leftX;
    public Transform rightX;

    [Header("Drop position")]
    public float topMargin = 0.6f;       // ȭ�� �� ������ �󸶳� �Ʒ��� �������
    public float ceilingImmunity = 0.6f; // ��� ���� õ�� ���� �ð�

    [Header("Spawn Weights")]
    [Tooltip("���� 0~7������ ���� ����ġ (float). ���� Ŭ���� �� ����.")]
    [SerializeField]
    float[] levelWeights =
        { 0.45f, 0.30f, 0.15f, 0.07f, 0.02f, 0.008f, 0.003f, 0.001f };

    [Tooltip("�ִ� ���� ���� ����. ��: �ʹݿ� 3~4�� �����ؼ� ���� �ø���")]
    [SerializeField][Range(0, 7)] int maxSpawnLevel = 7;

    // ���� ����
    int current, next1, next2;
    float dropY;

    // ��Ʈ(�̸�����)
    GameObject ghost;
    SpriteRenderer ghostSR;

    void Start()
    {
        if (!Camera.main) { Debug.LogError("Main Camera ����"); enabled = false; return; }
        if (GameManager.I == null || GameManager.I.piecePrefabs == null || GameManager.I.piecePrefabs.Length == 0)
        { Debug.LogError("GameManager �Ǵ� piecePrefabs �̼���"); enabled = false; return; }

        // ť �ʱ�ȭ
        current = RandomWeightedLevel();
        next1 = RandomWeightedLevel();
        next2 = RandomWeightedLevel();
        GameManager.I.SetPreview(next1, next2);

        // ��Ʈ ���� (SpriteRenderer��, ���� ����)
        ghost = new GameObject("GhostPiece");
        ghost.transform.SetParent(transform);
        ghostSR = ghost.AddComponent<SpriteRenderer>();
        ghostSR.sortingLayerName = "Pieces"; // ������ Default�� OK
        ghostSR.sortingOrder = 999;
        var c = Color.white; c.a = 0.9f;
        ghostSR.color = c;

        UpdateGhost();
    }

    void Update()
    {
        RecalcDropY();

        // ���콺 X�� ���� Ŭ���� (+ ���� ���� ������ ����)
        Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float halfW = CurrentHalfWidth();
        float x = Mathf.Clamp(m.x, leftX.position.x + halfW, rightX.position.x - halfW);

        // ������/��Ʈ ��ġ ����
        transform.position = new Vector3(x, dropY, 0);
        if (ghost) ghost.transform.position = transform.position;

        if (Input.GetMouseButtonDown(0) && GameManager.I.CanDrop())
            Drop();
    }

    void RecalcDropY()
    {
        dropY = Camera.main.orthographicSize - topMargin; // �׻� ȭ�� ��
    }

    void Drop()
    {
        // ��¥ ���� ���� (Pieces �Ʒ�)
        var prefab = GameManager.I.piecePrefabs[current];
        var go = Instantiate(prefab, transform.position, Quaternion.identity, GameManager.I.piecesParent);
        var p = go.GetComponent<Piece>();
        p.LockMerge(0.15f);
        p.ArmCeilingAfter(ceilingImmunity);

        // ť ������Ʈ + ������/��Ʈ ����
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
            // ������ �������� �״�� �ݿ�
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

    // �� float ����ġ ��� ���� �̱�
    int RandomWeightedLevel()
    {
        // piecePrefabs ���̿� weights ���� �� ���� �ʱ��� ���
        int len = GameManager.I?.piecePrefabs?.Length ?? levelWeights.Length;
        len = Mathf.Min(len, levelWeights.Length);

        // ���� ĸ
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

    // ���̵� �ø����(���ϸ� ���): ��Ÿ�ӿ� ���� ����
    public void SetMaxSpawnLevel(int lv)
    {
        maxSpawnLevel = Mathf.Clamp(lv, 0, (GameManager.I?.piecePrefabs?.Length ?? 8) - 1);
    }
}
