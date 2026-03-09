using UnityEngine;
using System.Collections;

public class ShrekController : MonoBehaviour
{
    [SerializeField] private GameObject box; // Коробка, в которой висит Шрек
    [SerializeField] private Animator animator; // Компонент Animator для анимаций
    [SerializeField] private float walkSpeed = 2f; // Скорость передвижения Шрека
    [SerializeField] private Transform groundCheck; // Точка проверки земли
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 0.5f); // Размер прямоугольника для проверки земли
    [SerializeField] private LayerMask groundLayer; // Слой земли

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isWaiting = false; // Флаг ожидания

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D не назначен для Шрека.");
        }

        if (animator == null)
        {
            Debug.LogError("Animator не назначен для Шрека.");
        }

        if (box == null)
        {
            Debug.LogError("Коробка не назначена в инспекторе.");
        }

        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck не назначен в инспекторе.");
        }
    }

    private void Update()
    {
        CheckGrounded();
    }

    // Метод для проверки, находится ли Шрек на земле с использованием прямоугольника
    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded && !wasGrounded && !isWaiting)
        {
            StartCoroutine(WaitBeforeWalking());
        }
    }

    // Сопрограмма ожидания перед началом движения
    private IEnumerator WaitBeforeWalking()
    {
        isWaiting = true;
        // Остановить Шрека
        rb.linearVelocity = Vector2.zero;
        animator.ResetTrigger("Walk");

        // Ждать 1 секунду
        yield return new WaitForSeconds(1f);

        // Начать движение
        TriggerWalkAnimation();
        // MoveLeft(); // Удаляем этот вызов

        isWaiting = false;
    }

    // Публичный метод для активации Шрека
    public void ActivateShrek()
    {
        if (box != null && box.activeSelf)
        {
            box.SetActive(false); // Скрыть коробку
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded && rb.linearVelocity.y <= 0 && !isWaiting)
        {
            MoveLeft(); // Перемещаем вызов движения сюда
        }
    }

    // Метод для запуска анимации ходьбы
    private void TriggerWalkAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Walk");
        }
    }

    // Метод для движения Шрека налево
    private void MoveLeft()
    {
        Vector2 direction = Vector2.left;
        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // Поворот налево

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction.x * walkSpeed, rb.linearVelocity.y); // Устанавливаем только горизонтальную скорость
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}