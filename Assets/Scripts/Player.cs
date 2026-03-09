using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed; // Базовая скорость движения игрока
    [SerializeField] private float agility; // Ловкость игрока
    [SerializeField] private float jumpForce; // Сила прыжка игрока
    private bool isGrounded; // Флаг, указывающий, на земле ли игрок

    [Header("Health")]
    [SerializeField] private float startingHealth; // Начальное здоровье игрока
    public float StartingHealth => startingHealth; // Публичное свойство для доступа к startingHealth
    [SerializeField] private float protection; // Защита игрока, уменьшающая получаемый урон
    public float currentHealth { get; private set; } // Текущее здоровье игрока
    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private InstantDeathMessageDisplay instantDeathMessageDisplay;

    [Header("Attack")]
    [SerializeField] private GameObject attackHitBox; // Объект, отвечающий за область атаки
    [SerializeField] public float attackDamage;
    [SerializeField] private float attackCooldown = 1.0f; // Скорость атаки
    [SerializeField] private AgentWeapon weaponSystem;
    [SerializeField] private ItemParameterSO durabilityParameter;
    [SerializeField] private EquipAttention equipAttention;
    public float criticalChance; 
    private bool canAttack = true;
    private float minimumAttackDamage = 1.5f;
    private float defaultAttackDamage;

    [Header("Collider")]
    [SerializeField] private CapsuleCollider2D capsuleCollider; 
    [SerializeField] private string aliveLayer = "Player"; // Слой для живого игрока
    [SerializeField] private string deadLayer = "PlayerDead"; // Слой для мёртвого игрока

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer; // Слой для проверки земли
    [SerializeField] private Transform groundCheck; // Точка проверки земли
    [SerializeField] private float groundCheckRadius = 0.3f; 
    [SerializeField] private float jumpCooldown = 1.0f; // Кулдаун для прыжка
    private float lastJumpTime;

    private Vector2 initialColliderOffset; // Изначальное смещение коллайдера
    private bool isAttacking = false; // Флаг, указывающий, выполняется ли атака
    private Rigidbody2D rb; // Компонент Rigidbody2D для физики
    private SpriteRenderer sprite; // Компонент SpriteRenderer для отображения спрайта
    private Animator anim; // Компонент Animator для управления анимациями
    private List<Enemy> enemiesHit = new List<Enemy>(); // Список врагов, уже поражённых в текущей атаке
    private bool isDead; // Флаг, указывающий, мёртв ли игрок

    public float Speed => speed;
    public float Agility => agility;
    public float AttackDamage => attackDamage;
    public float Protection => protection;
    public float CriticalChance => criticalChance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Получение компонента Rigidbody2D
        sprite = GetComponentInChildren<SpriteRenderer>(); // Получение компонента SpriteRenderer из дочерних объектов
        anim = GetComponent<Animator>(); // Получение компонента Animator
        gameObject.layer = LayerMask.NameToLayer(aliveLayer);
        attackHitBox.SetActive(false); // Отключение области атаки при старте
        defaultAttackDamage = attackDamage;

        if (capsuleCollider != null)
        {
            initialColliderOffset = capsuleCollider.offset; // Сохранение изначального смещения
        }

        if (equipAttention != null)
        {
            equipAttention.Initialize(this); // Инициализация EquipAttention с доступом к Player
        }

        if (instantDeathMessageDisplay != null)
        {
            instantDeathMessageDisplay.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        LoadHealth(); 
        StatsUpgrade(); 
        CheckDeadState();
    }

    public void LoadHealth()
    {
        startingHealth = PlayerPrefs.GetFloat("Player Max Health", 10f); // Загрузка максимального здоровья с дефолтом 10f
        currentHealth = PlayerPrefs.GetFloat("Player Current Health", startingHealth); // Загрузка текущего здоровья
    }
    
    public void SetIdleAnimation()
    {
        if (isDead) return;
        
        anim.SetBool("Run", false);
        anim.SetBool("Grounded", true);
        anim.ResetTrigger("Hurt");
        anim.ResetTrigger("Die");
    }

    public void SetDeadAnimation()
    {
        anim.SetBool("Run", false);
        anim.SetBool("Grounded", true);
        anim.SetBool("Attack", false);
        anim.ResetTrigger("Hurt");
        anim.SetTrigger("Die");
        gameObject.layer = LayerMask.NameToLayer(deadLayer);
    }

    private void CheckDeadState()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDead)
        {
            isDead = true;
            deathScreen.gameObject.SetActive(true);
            anim.SetTrigger("Die"); // Воспроизведение анимации смерти при загрузке
            Debug.Log("Игрок мёртв. Активирован DeathScreen.");
        }
    }

    public void StatsUpgrade()
    {
        speed = PlayerPrefs.GetFloat("Player Speed", 2.8f); // Базовая скорость
        agility = PlayerPrefs.GetFloat("Player Agility", 1f); // Базовая ловкость
        attackDamage = PlayerPrefs.GetFloat("Player Attack", 3f); 
        protection = PlayerPrefs.GetFloat("Player Protection", 0f); 
        startingHealth = PlayerPrefs.GetFloat("Player Max Health", 10f); // Обновление startingHealth
        criticalChance = PlayerPrefs.GetFloat("Critical Chance", 5f); // Загрузка Critical Chance
    }

    public void IncreaseHealth(float amount)
    {
        startingHealth += amount;
        PlayerPrefs.SetFloat("Player Max Health", startingHealth);
        PlayerPrefs.Save();

        currentHealth = Mathf.Min(currentHealth + amount, startingHealth); // Обновляем текущее здоровье
        PlayerPrefs.SetFloat("Player Current Health", currentHealth);
        PlayerPrefs.Save();

        Debug.Log($"Максимальное здоровье увеличено до {startingHealth}");
        Debug.Log($"Текущее здоровье обновлено до {currentHealth}");
    }

    public void IncreaseAttack(float amount)
    {
        attackDamage += amount;
        PlayerPrefs.SetFloat("Player Attack", attackDamage);
        PlayerPrefs.Save();
    }

    public void DecreaseAttack(float amount)
    {
        attackDamage -= amount;
        attackDamage = Mathf.Max(attackDamage, 0);
        PlayerPrefs.SetFloat("Player Attack", attackDamage);
        PlayerPrefs.Save();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        PlayerPrefs.SetFloat("Player Current Health", currentHealth);
        PlayerPrefs.Save();
    }

    public void InstantKill()
    {
        if (isDead) return; 

        isDead = true;
        currentHealth = 0;
        PlayerPrefs.SetFloat("Player Current Health", currentHealth);
        PlayerPrefs.Save();

        SetDeadAnimation();
        deathScreen.gameObject.SetActive(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerDead(true);
        }

        if (instantDeathMessageDisplay != null)
        {
            instantDeathMessageDisplay.gameObject.SetActive(true);
            instantDeathMessageDisplay.ShowRandomMessage();
        }

        Debug.Log("Игрок мгновенно убит.");
    }


    public void ResetHealth()
    {
        currentHealth = startingHealth;
        PlayerPrefs.SetFloat("Player Current Health", currentHealth);

        // Сброс состояния смерти через GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerDead(false);
        }

        isDead = false;
        deathScreen.gameObject.SetActive(false); 
        anim.ResetTrigger("Die");
        anim.ResetTrigger("Hurt");
        anim.SetBool("Grounded", true);

        gameObject.layer = LayerMask.NameToLayer(aliveLayer);

        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (isDead) return; // Если игрок мёртв, прекращаем дальнейшие действия

        if (Time.timeScale > 0f && !UIManager.Instance.IsAnyUIOpen())
        {
            HandleInput();
            HandleJump();
            HandleRun();
        }
        CheckGround(); // Проверка, на земле ли игрок

        if (Input.GetKeyDown(KeyCode.J))
        {
            PlayerTakeDamage(1); // Пример получения урона при нажатии клавиши E
        }
    }

    private void HandleRun()
    {
        if (Input.GetButton("Horizontal"))
        {
            float horizontalInput = Input.GetAxis("Horizontal"); // Получение значения ввода по горизонтали
            Vector2 direction = transform.right * horizontalInput; // Направление движения
            float actualSpeed = speed; 
            transform.position = Vector3.MoveTowards(transform.position, transform.position + (Vector3)direction, actualSpeed * Time.deltaTime); // Перемещение игрока
            sprite.flipX = horizontalInput < 0.0f; // Отражение спрайта при движении влево
            FlipAttackHitBox(); // Поворот области атаки вместе со спрайтом

            if (capsuleCollider != null)
            {
                Vector2 newOffset = initialColliderOffset;
                newOffset.x = sprite.flipX ? -Mathf.Abs(initialColliderOffset.x) : Mathf.Abs(initialColliderOffset.x);
                capsuleCollider.offset = newOffset;
            }

            anim.SetBool("Run", true); // Включение анимации бега
        }
        else
        {
            anim.SetBool("Run", false); // Отключение анимации бега
        }
    }

    private void HandleJump()
    {
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded && Time.time >= lastJumpTime + jumpCooldown)
        {
            Jump();
            lastJumpTime = Time.time; // Обновление времени последнего прыжка
        }
    }
   
    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Применение силы вверх для прыжка
        isGrounded = false; // Установка флага на "не на земле"
    }

    private void CheckGround()
    {
        bool wasGrounded = isGrounded;
        Collider2D colliders = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer | (1 << LayerMask.NameToLayer("Enemy"))); // Проверка перекрытий в круге радиусом 0.3f
        isGrounded = colliders != null; // Если перекрытие найдено, считаем, что игрок на земле
        
        if (isGrounded)
        {
            anim.SetBool("Grounded", true); // Установка анимации состояния на земле
        }
        else
        {
            anim.SetBool("Grounded", false); // Отключение анимации состояния на земле
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking && canAttack)
        {
            StartAttack(); // Начало атаки при нажатии пробела и если не выполняется другая атака
        }
    }

    private void StartAttack()
    {
        if (isDead) return; // Если игрок мёртв, отменяем атаку
        if (isGrounded) 
        {   
            isAttacking = true; // Установка флага атаки
            canAttack = false; // Запрет на атаку
            enemiesHit.Clear(); // Очистка списка ранее поражённых врагов
            int attackChoice = UnityEngine.Random.Range(1, 3); // Случайный выбор варианта атаки
            anim.Play($"Player_Attack{attackChoice}"); // Воспроизведение анимации атаки
            StartCoroutine(DoAttack()); // Запуск корутины атаки
            StartCoroutine(AttackCooldownCoroutine());
        }
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown); // Ожидание времени перезарядки
        canAttack = true; // Разрешение на выполнение новой атаки
    }

    private void FlipAttackHitBox()
    {
        Vector2 hitBoxPosition = attackHitBox.transform.localPosition; // Получение позиции области атаки
        hitBoxPosition.x = sprite.flipX ? -Mathf.Abs(hitBoxPosition.x) : Mathf.Abs(hitBoxPosition.x); // Инверсия позиции по X при отражении спрайта
        attackHitBox.transform.localPosition = hitBoxPosition; // Применение новой позиции
    }

    private IEnumerator DoAttack()
    {
        yield return new WaitForSeconds(0.3f); // Ожидание ещё 0.3 секунд

        if (weaponSystem != null)
        {
            float currentDurability = weaponSystem.GetWeaponParameterValue(durabilityParameter);

            if (currentDurability > 0)
            {
                attackHitBox.SetActive(true); // Включение области атаки
            }
            else
            {
                attackHitBox.SetActive(true); 
                equipAttention.ShowAttention();
            }
        }

        yield return new WaitForSeconds(0.1f); // Ожидание 0.1 секунд
        attackHitBox.SetActive(false); // Выключение области атаки
        isAttacking = false;
    }

    private float CalculateDamage(float baseDamage)
    {
        float damage = baseDamage;

        // Проверка, равен ли параметр оружия нулю
        if (IsWeaponBroken())
        {
            damage *= 0.2f; // Уменьшаем урон на 80%
            Debug.Log("Оружие сломано. Урон уменьшен на 80%.");
        }

        return damage;
    }

    private bool IsWeaponBroken()
    {
        return weaponSystem != null && weaponSystem.GetWeaponParameterValue(durabilityParameter) <= 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return; // Если игрок мёртв, ничего не делаем

        if (collision.gameObject.CompareTag("Enemy") && attackHitBox.activeSelf)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>(); // Получение компонента Enemy из столкнувшегося объекта
            if (enemy != null && !enemiesHit.Contains(enemy))
            {
                bool isCritical = Random.Range(0f, 100f) < criticalChance;
                float damageToDeal = attackDamage;

                if (isCritical)
                {
                    damageToDeal *= 1.5f;
                    Debug.Log("Критический удар!");
                }

                // Применяем расчетный урон с учетом защиты врага
                damageToDeal = CalculateDamage(damageToDeal);

                enemy.EnemyTakeDamage(damageToDeal); // Нанесение урона врагу
                enemiesHit.Add(enemy); // Добавление врага в список поражённых

                if (weaponSystem != null)
                {
                    weaponSystem.DecreaseWeaponParameter(durabilityParameter, Random.Range(2, 4));
                }
            }
        }
    }
   
    private void OnEnable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelChanged += HandleLevelChange; // Подписка на событие изменения уровня
            LevelManager.Instance.OnExperienceChanged += HandleExperienceChange; // Подписка на событие изменения опыта
        }
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelChanged -= HandleLevelChange; // Отписка от события изменения уровня
            LevelManager.Instance.OnExperienceChanged -= HandleExperienceChange; // Отписка от события изменения опыта
        }
    }

    public void HandleLevelChange(int newLevel)
    {
        Debug.Log($"Игрок достиг уровня {newLevel}");
    }

    public void HandleExperienceChange(float newExperience)
    {
        Debug.Log($"Игрок получил {newExperience} опыта. Текущий уровень: {LevelManager.Instance.GetCurrentLevel()}");
    }

    public void SpendUpgradePoint()
    {
        if (LevelManager.Instance != null)
        {
            bool success = LevelManager.Instance.SpendUpgradePoint();
            if (success)
            {
                Debug.Log("Очко апгрейда успешно потрачено.");
            }
        }
        else
        {
            Debug.LogWarning("LevelManager не инициализирован.");
        }
    }
    
    public void PlayerTakeDamage(float damage)
    {
        if (isDead) return; 

        float dodgeChance = Mathf.Clamp(agility * 5f, 0f, 100f); // 5% за каждую единицу ловкости
        if (Random.Range(0f, 100f) < dodgeChance)
        {
            Debug.Log("Урон избегнут благодаря ловкости!");
            return; // Уклонение от урона
        }

        float reducedDamage = damage * (1 - protection); // Уменьшение урона за счёт защиты
        reducedDamage = Mathf.Max(reducedDamage, 0); // Гарантия, что урон не станет отрицательным
        currentHealth = Mathf.Clamp(currentHealth - reducedDamage, 0, startingHealth); // Вычитание урона и ограничение значения здоровья
        PlayerPrefs.SetFloat("Player Current Health", currentHealth);
        PlayerPrefs.Save();

        if (currentHealth > 0)
        {
            if (isGrounded)
            {
                anim.SetTrigger("Hurt"); // Воспроизведение анимации получения урона
            }
        }
        else
        {
            if (!isDead)
            {
                isDead = true; 
                SetDeadAnimation();
                deathScreen.gameObject.SetActive(true);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetPlayerDead(true);
                }
            }
        }

        Debug.Log($"Текущее здоровье: {currentHealth}");
    }
}