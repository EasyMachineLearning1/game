using UnityEngine;

public class LevelBounds : MonoBehaviour
{
    [SerializeField] private float deathY = -5f;

    private Vector3 spawnPoint;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        spawnPoint = transform.position;
    }

    void Update()
    {
        if (transform.position.y < deathY)
        {
            controller.enabled = false;
            transform.position = spawnPoint;
            controller.enabled = true;

            // Сообщаем GameManager
            if (GameManager.Instance != null)
                GameManager.Instance.OnDeath();
        }
    }
}