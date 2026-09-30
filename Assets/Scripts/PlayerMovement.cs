using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Проверка земли")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private LayerMask groundMask = ~0; // все слои

    private CharacterController controller;
    private Vector3 velocity;
    private float lastGroundedTime;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Луч из низа капсулы, игнорируем самого игрока
        Vector3 rayOrigin = transform.position + Vector3.down * (controller.height / 2f - controller.radius + 0.05f);

        // Создаём маску: все слои, КРОМЕ Player
        int mask = groundMask & ~(1 << gameObject.layer);

        bool grounded = Physics.Raycast(rayOrigin, Vector3.down,
                                        groundCheckDistance, mask,
                                        QueryTriggerInteraction.Ignore);
        if (grounded)
            lastGroundedTime = Time.time;

        // Движение
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Прыжок
        if (Input.GetButtonDown("Jump") && Time.time - lastGroundedTime < coyoteTime)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Гравитация
        if (grounded && velocity.y < 0)
            velocity.y = -2f;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        Vector3 rayOrigin = transform.position + Vector3.down * (controller.height / 2f - controller.radius + 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
    }
}