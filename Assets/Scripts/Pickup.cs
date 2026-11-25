using UnityEngine;

// A neon crystal the player collects. Trigger collider: the player passes through it.
[RequireComponent(typeof(CircleCollider2D))]
public class Pickup : MonoBehaviour
{
    public float bobHeight = 0.15f;
    public float bobSpeed = 3f;
    public float spinSpeed = 90f;

    Vector3 start;

    void Reset() { GetComponent<CircleCollider2D>().isTrigger = true; }

    void Awake()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
        start = transform.position;
    }

    void Update()
    {
        transform.position = start + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;   // enemies do not collect
        AudioManager.Play("pickup");
        Destroy(gameObject);
    }
}
