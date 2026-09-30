using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [Header("Покачивание")]
    [SerializeField] private float bobSpeed = 10f;      // Частота
    [SerializeField] private float bobAmount = 0.05f;   // Амплитуда

    private Vector3 startPos;
    private PlayerMovement playerMovement;

    void Start()
    {
        startPos = transform.localPosition;
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        // Проверяем, двигается ли игрок (WASD)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        if (isMoving)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.localPosition = startPos + new Vector3(0, bob, 0);
        }
        else
        {
            // Плавно возвращаем в исходное
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Time.deltaTime * 5f);
        }
    }
}