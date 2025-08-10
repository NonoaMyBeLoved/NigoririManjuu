using UnityEngine;
using System;

public class CollisionRouter : MonoBehaviour
{
    public static event Action<Piece, Piece, Vector2> OnPieceCollision;

    void OnCollisionEnter2D(Collision2D col)
    {
        var a = GetComponent<Piece>();
        var b = col.collider.GetComponent<Piece>();
        if (a == null || b == null) return;

        // �� �ν��Ͻ� ID�� �� ���� �ʸ� ��ȣ�� ����(�ߺ� ����)
        if (a.GetInstanceID() < b.GetInstanceID())
        {
            var p = col.GetContact(0).point;
            OnPieceCollision?.Invoke(a, b, p);
        }
    }
}
