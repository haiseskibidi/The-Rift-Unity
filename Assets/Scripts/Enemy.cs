using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory;
using Inventory.Model;

public abstract class Enemy : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackHitBoxHeight = 1.0f;
    [SerializeField] private int attackDamage;
    [SerializeField] private CapsuleCollider2D attackHitBox;
    [SerializeField] private float attackColliderDistance;
    [SerializeField] private float timeToDamage; 

    [Header("Health")]
    [SerializeField] private bool canRespawn;
    [SerializeField] private float respawnTime;
    public float health;
    public EnemyHealthBar healthBar;

    [Header("Drop")]
    [SerializeField]
    private List<DropItem> possibleDrops = new List<DropItem>();

    [SerializeField] private InventoryController inventoryController;

    [Header("Experience")]
    [SerializeField] private int experienceAmount;

    [Header("SpawnPoint")]
    [SerializeField] private Transform spawnPoint;
    
    private float maxHealth;
    private Animator anim;
    private Transform target;
    private SpriteRenderer spriteRenderer;
    private float cooldownTimer = Mathf.Infinity;
    private bool isDead;

    [Header("Enemy ID")]
    [SerializeField] private string enemyID;

    private EnemyPatrol enemyPatrol;
    private float timeInAttackRange = 0f; // Время, проведённое игроком в зоне атаки

    [Header("Layers")]
    [SerializeField] private string aliveLayer = "Enemy"; // Слой для живого врага
    [SerializeField] private string deadLayer = "EnemyDead"; // Слой для мёртвого врага
    [SerializeField] private LayerMask playerLayer;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Генерация уникального ID для врага, если он не задан
        if (string.IsNullOrEmpty(enemyID))
        {
            enemyID = System.Guid.NewGuid().ToString();
        }

        // Проверка, был ли этот враг уже убит
        if (GameManager.Instance != null && GameManager.Instance.IsEnemyKilled(enemyID))
        {
            Destroy(gameObject);
            return;
        }

        InitializeComponents();
        maxHealth = health;
        healthBar.SetHealth(health, maxHealth);
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();


        // Устанавливаем начальный слой как живой
        gameObject.layer = LayerMask.NameToLayer(aliveLayer);
    }

    /// <summary>
    /// Инициализация компонентов.
    /// </summary>
    private void InitializeComponents()
    {
        if (isDead)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDead)
        {
            // Если игрок мёртв, прекратить атаки и вернуть патруль
            if (enemyPatrol != null)
            {
                enemyPatrol.enabled = true;
            }
            return;
        }

        if (spriteRenderer == null)
            return;

        cooldownTimer += Time.deltaTime;

        if (IsInAttackRange())
        {
            timeInAttackRange += Time.deltaTime;
            if (timeInAttackRange >= timeToDamage && cooldownTimer >= attackCooldown)
            {
                cooldownTimer = 0;
                timeInAttackRange = 0f;
                anim.SetTrigger("IsAttacking");

                // Нанесение урона игроку
                Player player = target.GetComponent<Player>();
                if (player != null)
                {
                    player.PlayerTakeDamage(attackDamage);
                }
            }
        }
        else
        {
            timeInAttackRange = 0f;
        }

        if (enemyPatrol != null)
            enemyPatrol.enabled = !IsInAttackRange();
    }

    private bool IsInAttackRange()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            attackHitBox.bounds.center + transform.right * attackRange * transform.localScale.x * attackColliderDistance,
            new Vector2(attackHitBox.bounds.size.x * attackRange, attackHitBoxHeight), // Используем новую переменную
            0, Vector2.left, 0, playerLayer);

        return hit.collider != null;
    }

    /// <summary>
    /// Поворачивает спрайт врага в направлении движения.
    /// </summary>
    /// <param name="direction">Направление движения.</param>
    private void FlipSprite(Vector2 direction)
    {
        if (direction.x > 0)
        {
            spriteRenderer.flipX = false; // Поворачиваем направо
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = true; // Поворачиваем налево
        }
    }

    /// <summary>
    /// Метод для нанесения урона врагу.
    /// </summary>
    public void EnemyTakeDamage(float damage)
    {
        if (isDead)
            return;

        health -= damage;
        healthBar.SetHealth(health, maxHealth);

        if (health > 0f)
        {
            anim.SetTrigger("IsHurt");
        }
        else
        {
            Die();
        }
    }

    /// <summary>
    /// Метод для обработки смерти врага.
    /// </summary>
    private void Die()
    {
        anim.SetTrigger("IsDead");
        isDead = true;

        // Переход на слой "EnemyDead"
        gameObject.layer = LayerMask.NameToLayer(deadLayer);

        if (GameManager.Instance != null)
        {
            if (!canRespawn)
            {
                GameManager.Instance.AddKilledEnemy(enemyID);
            }
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddExperience(experienceAmount);
        }
        HandleDrops();

        // Добавление 20% шанса на получение 1 очка апгрейда
        float dropChance = 20f;
        if (Random.Range(0f, 100f) < dropChance)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.AddUpgradePoint();
                Debug.Log("Выпало 1 очко апгрейда!");
            }
        }
        if (canRespawn)
        {
            Invoke("Deactivate", 1.2f);
            Invoke("Respawn", respawnTime);
        }
        else
        {
            Destroy(gameObject, 1.2f);
        }
    }

    public void Respawn()
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        else
        {
            transform.position = transform.position;
        }
        gameObject.SetActive(true);
        health = maxHealth;
        healthBar.SetHealth(health, maxHealth);
        enemyPatrol.enabled = true;
        isDead = false;
        gameObject.layer = LayerMask.NameToLayer(aliveLayer);

    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void HandleDrops()
    {
        if (possibleDrops == null || possibleDrops.Count == 0)
            return;

        DropHandler dropHandler = new DropHandler(possibleDrops, inventoryController);
        dropHandler.HandleDrop();
    }

    private void OnDrawGizmosSelected()
    {
        // Отображение радиуса атаки в редакторе
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(
            attackHitBox.bounds.center + transform.right * attackRange * transform.localScale.x * attackColliderDistance,
            new Vector2(attackHitBox.bounds.size.x * attackRange, attackHitBoxHeight - 0.5f)); // Используем новую переменную
    }
}