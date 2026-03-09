using UnityEngine;
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;
    [Header("Enemy")]
    [SerializeField] private Transform enemy;
    [Header("Movement")]
    [SerializeField] private float speed;
    private Vector3 initScale;
    private bool movingLeft;
    [Header("Idle Behavior")]
    [SerializeField] private float idleDuration;
    private float idleTimer;
    [Header("Animation")]
    [SerializeField] private Animator anim;
    [Header("Aggro")]
    [SerializeField] private float aggroRadius = 5f; // Радиус агрессии спереди
    [SerializeField] private float aggroColliderDistance = 0.5f; // Расстояние до коллайдера агро зоны
    [SerializeField] private CapsuleCollider2D aggroHitBox; // Коллайдер агро зоны
    [SerializeField] private LayerMask playerLayer;
    private Transform player;
    private bool isChasing = false;

    private void Awake()
    {
        if (enemy != null)
        {
            initScale = enemy.localScale;
        }
        else
        {
            Debug.LogWarning("Enemy трансформ не назначен.");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Игрок не найден в сцене.");
        }
    }

    private void OnDisable()
    {
        if (anim != null)
        {
            anim.SetBool("IsWalking", false);
        }
    }

    private void Update()
    {
        if (player == null || enemy == null)
            return;

        // Проверка, мёртв ли игрок
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDead)
        {
            // Если игрок мёртв, прекратить преследование и вернуться к патрулированию
            if (isChasing)
            {
                isChasing = false;
                anim.SetBool("IsWalking", false);
                // Дополнительно можно добавить логику возврата на патрульные точки
            }
            Patrol();
            return;
        }

        if (IsPlayerInAggroRange())
        {
            if (!isChasing)
            {
                isChasing = true;
            }
            ChasePlayer();
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                idleTimer = idleDuration; // Инициирует смену направления после патрулирования
            }
            Patrol();
        }
    }

    /// <summary>
    /// Проверяет, находится ли игрок в агро зоне перед врагом.
    /// </summary>
    /// <returns>True, если игрок в агро зоне, иначе False.</returns>
    private bool IsPlayerInAggroRange()
    {
        if (enemy == null || aggroHitBox == null)
            return false;

        Vector2 direction = enemy.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)aggroHitBox.bounds.center + direction * aggroRadius * aggroColliderDistance;
        Vector2 size = new Vector2(aggroHitBox.bounds.size.x * aggroRadius, aggroHitBox.bounds.size.y);
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, direction, 0, playerLayer);
        return hit.collider != null;
    }

    private void Patrol()
    {
        if (enemy == null)
            return;

        if (movingLeft)
        {
            if (enemy.position.x >= leftEdge.position.x)
                MoveInDirection(-1);
            else
                DirectionChange();
        }
        else
        {
            if (enemy.position.x <= rightEdge.position.x)
                MoveInDirection(1);
            else
                DirectionChange();
        }
    }

    private void ChasePlayer()
    {
        if (player == null || enemy == null)
            return;

        Vector2 direction = (player.position - enemy.position).normalized;
        if (anim != null)
        {
            anim.SetBool("IsWalking", true);
        }
        if (enemy != null)
        {
            enemy.localScale = new Vector2(Mathf.Sign(direction.x) * Mathf.Abs(initScale.x), initScale.y);
            enemy.position = Vector2.MoveTowards(enemy.position, player.position, speed * Time.deltaTime);
        }
    }

    private void MoveInDirection(int direction)
    {
        if (enemy == null || anim == null)
            return;

        idleTimer = 0;
        anim.SetBool("IsWalking", true);
        enemy.localScale = new Vector2(Mathf.Abs(initScale.x) * direction, initScale.y);
        enemy.position = new Vector2(enemy.position.x + Time.deltaTime * direction * speed, enemy.position.y);
    }

    private void DirectionChange()
    {
        if (anim != null)
        {
            anim.SetBool("IsWalking", false);
        }
        idleTimer += Time.deltaTime;
        if (idleTimer > idleDuration)
            movingLeft = !movingLeft;
    }

    private void OnDrawGizmosSelected()
    {
        if (aggroHitBox == null || enemy == null)
            return;

        // Отображение агро зоны в редакторе
        Gizmos.color = Color.red;
        Vector2 direction = enemy.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)aggroHitBox.bounds.center + direction * aggroRadius * aggroColliderDistance;
        Vector2 size = new Vector2(aggroHitBox.bounds.size.x * aggroRadius, aggroHitBox.bounds.size.y + 2f);
        Gizmos.DrawWireCube(origin, size);
    }
}