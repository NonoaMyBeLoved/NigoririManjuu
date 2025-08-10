using UnityEngine;

public class Piece : MonoBehaviour
{
    public int level;
    public bool canMerge = true;
    public float mergeCooldown = 0.1f;
    public float ceilingSafeUntil = 0f;

    // ★ 합체 중복 방지 플래그
    public bool isMerging = false;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "Piece";
    }

    public void ArmCeilingAfter(float seconds) => ceilingSafeUntil = Time.time + seconds;
    public bool IsCeilingArmed() => Time.time >= ceilingSafeUntil;

    public float Speed()
    {
#if UNITY_6000_0_OR_NEWER
        return rb ? rb.linearVelocity.magnitude : 0f;
#else
        return rb ? rb.velocity.magnitude : 0f;
#endif
    }

    public void LockMerge(float t)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(CoLock(t));
    }
    System.Collections.IEnumerator CoLock(float t)
    {
        canMerge = false;
        yield return new WaitForSeconds(t);
        canMerge = true;
    }
}
