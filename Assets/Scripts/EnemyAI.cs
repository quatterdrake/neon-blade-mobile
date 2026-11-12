using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    enum State { Patrol, Chase, Attack }

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;
    public float patrolRange = 3f;

    [Header("Vision")]
    public float sightRange = 12f;
    public float loseSightTime = 1.5f;

    [Header("Attack")]
    public float attackRange = 2f;
    public float windupTime = 0.4f;
    public float cooldownTime = 1f;

    State state = State.Patrol;
    Rigidbody2D rb;
    SpriteRenderer sr;
    PlayerController player;

    float startX;
    float facing = 1f;
    float lostTimer;
    float attackTimer;
    bool hitDone;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        startX = transform.position.x;
        player = FindFirstObjectByType<PlayerController>();
        if (player == null) Debug.LogError("EnemyAI: PlayerController not found in scene!");
        SetState(State.Patrol);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        switch (state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase:  DoChase();  break;
            case State.Attack: DoAttack(); break;
        }
    }


    void DoPatrol()
    {
        if (transform.position.x > startX + patrolRange) facing = -1f;
        if (transform.position.x < startX - patrolRange) facing = 1f;

        Move(facing * patrolSpeed);

        if (CanSeePlayer()) SetState(State.Chase);
    }

    void DoChase()
    {
        float dx = player.transform.position.x - transform.position.x;
        facing = Mathf.Sign(dx);
        Move(facing * chaseSpeed);

        if (CanSeePlayer()) lostTimer = 0f;
        else lostTimer += Time.fixedDeltaTime;

        if (lostTimer >= loseSightTime)
        {
            SetState(State.Patrol);
            return;
        }

        float dist = Mathf.Abs(dx);
        if (dist <= attackRange) SetState(State.Attack);
    }

    void DoAttack()
    {
        Move(0f);
        attackTimer += Time.fixedDeltaTime;

        // strike at the end of the wind-up
        if (!hitDone && attackTimer >= windupTime)
        {
            hitDone = true;
            float dist = Mathf.Abs(player.transform.position.x - transform.position.x);
            Debug.Log(name + " strike! dist=" + dist + " dashing=" + player.IsDashing);

            if (dist <= attackRange * 1.3f && !player.IsDashing)
            {
                Debug.Log("Player hit");
                player.Die();
            }
        }

        if (attackTimer >= windupTime + cooldownTime)
            SetState(CanSeePlayer() ? State.Chase : State.Patrol);
    }


    void SetState(State newState)
    {
        state = newState;
        lostTimer = 0f;
        attackTimer = 0f;
        hitDone = false;

        if (sr != null)
        {
            if (state == State.Patrol) sr.color = new Color32(255, 42, 109, 255);   // pink
            if (state == State.Chase)  sr.color = new Color32(255, 160, 0, 255);    // orange
            if (state == State.Attack) sr.color = Color.white;
        }
        Debug.Log(name + " -> " + state);
    }

    void Move(float speed)
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    bool CanSeePlayer()
    {
        Vector2 origin = transform.position;
        Vector2 toPlayer = (Vector2)player.transform.position - origin;

        if (toPlayer.magnitude > sightRange) return false;
        if (Mathf.Sign(toPlayer.x) != facing) return false;   // only in front

        // The first thing the ray hits decides: the player = seen, a wall = blocked
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, toPlayer.normalized, sightRange);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        Debug.DrawRay(origin, toPlayer.normalized * sightRange, Color.red);

        foreach (var h in hits)
        {
            if (h.collider.gameObject == gameObject) continue;   // skip self
            if (h.collider.CompareTag("Enemy")) continue;        // skip other enemies
            return h.collider.gameObject == player.gameObject;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}