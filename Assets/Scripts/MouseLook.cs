using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform playerBody; // Ссылка на Player

    private float xRotation = 0f;

    void Start()
    {
        // Прячем курсор в центре экрана и блокируем его
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Получаем движение мыши за кадр
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Вращаем ТЕЛО игрока влево-вправо (по оси Y)
        playerBody.Rotate(Vector3.up * mouseX);

        // Вращаем ТОЛЬКО КАМЕРУ вверх-вниз (по оси X)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // ограничение
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}