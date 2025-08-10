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

        // ★ 인스턴스 ID가 더 작은 쪽만 신호를 보냄(중복 방지)
        if (a.GetInstanceID() < b.GetInstanceID())
        {
            var p = col.GetContact(0).point;
            OnPieceCollision?.Invoke(a, b, p);
        }
    }
}
