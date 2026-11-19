using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Neon Blade/Enemy Data")]
public class EnemyData : ScriptableObject
{
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
}