using UnityEngine;

public class MergeManager : MonoBehaviour
{
    public GameObject[] piecePrefabs;     // 0~7
    public float maxMergeSpeed = 6f;

    void OnEnable()
    {
        CollisionRouter.OnPieceCollision += TryMerge;
    }
    void OnDisable()
    {
        CollisionRouter.OnPieceCollision -= TryMerge;
    }

    void TryMerge(Piece a, Piece b, Vector2 _)
    {
        if (a == null || b == null) return;
        if (a.level != b.level) return;
        if (!a.canMerge || !b.canMerge) return;

        if (a.Speed() > maxMergeSpeed || b.Speed() > maxMergeSpeed)
        {
            a.LockMerge(0.05f);
            b.LockMerge(0.05f);
            return;
        }

        // �ִ�(7)���� ������ �� �� ���� + ���ʽ�
        if (a.level >= 7)
        {
            Destroy(a.gameObject);
            Destroy(b.gameObject);
            GameManager.I.AddScore(GameManager.I.ScoreForMerge(8)); // ������ ���ʽ�
            return;
        }

        int next = a.level + 1;
        Vector3 pos = (a.transform.position + b.transform.position) * 0.5f;

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        var merged = Instantiate(piecePrefabs[next], pos, Quaternion.identity, GameManager.I.piecesParent)
             .GetComponent<Piece>();
        merged.LockMerge(0.1f);
        merged.ArmCeilingAfter(0.2f);
        GameManager.I.AddScore(GameManager.I.ScoreForMerge(next));

        merged.LockMerge(0.1f);
        merged.ArmCeilingAfter(0.2f); // ��ü ���� �ٷ� õ�� �����Ǵ� �� ����

        GameManager.I.AddScore(GameManager.I.ScoreForMerge(next));
    }
}
