using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Pieces")]
    public GameObject[] piecePrefabs;   // 0~7
    public Transform piecesParent;      // Playfield/Pieces
    public Sprite[] pieceSprites;       // ����θ� �ڵ� ä��

    [Header("UI")]
    public TMP_Text scoreText;
    public Image nextImg1;
    public Image nextImg2;

    [Header("Scoring")]
    public int baseScore = 2;     // �⺻ ����
    [Range(1f, 5f)] public float growth = 2f; // ������ ��� (2�� 2^level)
    public int[] scoreByLevel;     // ������ ���� ����(���ϸ� ä�� �ֱ�, 0�̸� ����)
    public int finalMergeBonus = 3; // ���� ���� ��ü ���ʽ� ���

    int score = 0;
    bool canDrop = true;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        // ��������Ʈ �ڵ� ä��
        if (piecePrefabs != null && piecePrefabs.Length > 0 &&
            (pieceSprites == null || pieceSprites.Length != piecePrefabs.Length))
        {
            pieceSprites = new Sprite[piecePrefabs.Length];
            for (int i = 0; i < piecePrefabs.Length; i++)
            {
                var sr = piecePrefabs[i].GetComponentInChildren<SpriteRenderer>();
                pieceSprites[i] = sr ? sr.sprite : null;
            }
        }

        UpdateScoreUI(); // ���� UI 0������ ǥ��
        if (nextImg1) nextImg1.preserveAspect = true;
        if (nextImg2) nextImg2.preserveAspect = true;
    }

    public bool CanDrop() => canDrop;

    public void NotifyDropped()
    {
        canDrop = false;
        Invoke(nameof(AllowDrop), 0.15f); // ��� ���� ���� ����ȭ ���
    }
    void AllowDrop() => canDrop = true;

    public void AddScore(int v)
    {
        score += v;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = $"SCORE {score}";
    }

    // �� ���⸸ �ٲ㼭 ���� � ���� ��
    public int ScoreForMerge(int level)
    {
        // scoreByLevel�� ���� ������ �� ���� �켱 ��� (��: L0~L7 ���̷� ä�� �ֱ�)
        if (scoreByLevel != null && level >= 0 && level < scoreByLevel.Length && scoreByLevel[level] > 0)
            return scoreByLevel[level];

        // �ƴϸ� baseScore * growth^level ���
        return Mathf.RoundToInt(baseScore * Mathf.Pow(growth, level));
    }

    public void SetPreview(int a, int b)
    {
        if (pieceSprites == null || pieceSprites.Length == 0) return;
        if (nextImg1) nextImg1.sprite = pieceSprites[Mathf.Clamp(a, 0, pieceSprites.Length - 1)];
        if (nextImg2) nextImg2.sprite = pieceSprites[Mathf.Clamp(b, 0, pieceSprites.Length - 1)];
    }

    public void GameOver()
    {
        canDrop = false;
        Debug.Log("GAME OVER");
        // TODO: �г�/�����
    }
}
