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
    bool wasGrounded = true;
    float lastVy;

    public bool IsDashing => dashTimer > 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // find the counter automatically if the field was left empty
        if (enemyCounter == null)
            enemyCounter = FindFirstObjectByType<EnemyCounter>();
    }

    public void SetMove(float dir)
    {
        moveInput = dir;
        if (dir != 0f) facing = Mathf.Sign(dir);
    }

    public void Jump()
    {
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            AudioManager.Play("jump");
        }
    }

    public void Attack()
    {
        if (!IsDashing)
        {
            dashTimer = dashTime;
            AudioManager.Play("dash");
        }
    }

    void FixedUpdate()
    {
        // landing thud: only after a real fall, not when walking off a tiny step
        bool grounded = IsGrounded();
        if (grounded && !wasGrounded && lastVy < -3f) AudioManager.Play("land");
        wasGrounded = grounded;
        lastVy = rb.linearVelocity.y;

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

    public void Die()
    {
        // AudioManager survives the reload, so these sounds are not cut off
        AudioManager.Play("death");
        AudioManager.PlayDelayed("lose", 0.35f);

        SaveManager.Data.deaths++;
        SaveManager.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnCollisionEnter2D(Collision2D c) { TryKill(c); }
    void OnCollisionStay2D(Collision2D c)  { TryKill(c); }

    void TryKill(Collision2D c)
    {
        if (!IsDashing || !c.gameObject.CompareTag("Enemy")) return;

        Destroy(c.gameObject);
        AudioManager.Play("hit");

        if (enemyCounter != null)
        {
            Debug.Log("Enemy killed");
            enemyCounter.EnemyKilled();
        }
        else
        {
            Debug.LogError("EnemyCounter not found! Add the EnemyCounter script to GameManager.");
        }
    }
}