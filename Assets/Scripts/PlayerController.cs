using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 14f;
    public float dashSpeed = 22f;
    public float dashTime = 0.2f;
    public LayerMask groundLayer;
    public EnemyCounter enemyCounter;

    Rigidbody2D rb;
    float moveInput;
    float facing = 1f;
    float dashTimer;

    bool IsDashing => dashTimer > 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetMove(float dir)
    {
        moveInput = dir;
        if (dir != 0f) facing = Mathf.Sign(dir);
    }

    public void Jump()
    {
        if (IsGrounded())
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    public void Attack()
    {
        if (!IsDashing) dashTimer = dashTime;
    }

    void FixedUpdate()
    {
        if (IsDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(facing * dashSpeed, 0f);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    bool IsGrounded()
    {
        Vector2 feet = (Vector2)transform.position + Vector2.down * 0.55f;
        return Physics2D.OverlapBox(feet, new Vector2(0.8f, 0.1f), 0f, groundLayer);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (!c.gameObject.CompareTag("Enemy")) return;

        if (IsDashing)
        {
            Destroy(c.gameObject);
            if (enemyCounter != null) enemyCounter.EnemyKilled();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}