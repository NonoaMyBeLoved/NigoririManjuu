using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Pieces")]
    public GameObject[] piecePrefabs;   // 0~7
    public Transform piecesParent;      // Playfield/Pieces
    public Sprite[] pieceSprites;       // 비워두면 자동 채움

    [Header("UI")]
    public TMP_Text scoreText;
    public Image nextImg1;
    public Image nextImg2;

    [Header("Scoring")]
    public int baseScore = 2;     // 기본 점수
    [Range(1f, 5f)] public float growth = 2f; // 레벨당 배수 (2면 2^level)
    public int[] scoreByLevel;     // 레벨별 고정 점수(원하면 채워 넣기, 0이면 무시)
    public int finalMergeBonus = 3; // 최종 레벨 합체 보너스 배수

    int score = 0;
    bool canDrop = true;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        // 스프라이트 자동 채움
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

        UpdateScoreUI(); // 시작 UI 0점으로 표시
        if (nextImg1) nextImg1.preserveAspect = true;
        if (nextImg2) nextImg2.preserveAspect = true;
    }

    public bool CanDrop() => canDrop;

    public void NotifyDropped()
    {
        canDrop = false;
        Invoke(nameof(AllowDrop), 0.15f); // 드랍 직후 물리 안정화 대기
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

    // ★ 여기만 바꿔서 점수 곡선 조절 ★
    public int ScoreForMerge(int level)
    {
        // scoreByLevel에 값이 있으면 그 값을 우선 사용 (예: L0~L7 길이로 채워 넣기)
        if (scoreByLevel != null && level >= 0 && level < scoreByLevel.Length && scoreByLevel[level] > 0)
            return scoreByLevel[level];

        // 아니면 baseScore * growth^level 사용
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
        // TODO: 패널/재시작
    }
}
