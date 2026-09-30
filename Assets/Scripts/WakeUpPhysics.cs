using UnityEngine;

public class WakeUpPhysics : MonoBehaviour
{
    void Start()
    {
        // Ищем компонент Rigidbody на этом объекте
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.WakeUp(); // Принудительно будим физику
        }
    }
}