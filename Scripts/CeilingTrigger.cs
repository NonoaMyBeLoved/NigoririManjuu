using UnityEngine;

public class CeilingTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Piece")) return;

        var p = other.GetComponent<Piece>();
        if (p == null) return;

        // ��� ���� �鿪 �ð� ������ ����
        if (!p.IsCeilingArmed()) return;

        GameManager.I.GameOver();
    }
}
