using UnityEngine;

public class CeilingTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Piece")) return;

        var p = other.GetComponent<Piece>();
        if (p == null) return;

        // 드랍 직후 면역 시간 동안은 무시
        if (!p.IsCeilingArmed()) return;

        GameManager.I.GameOver();
    }
}
