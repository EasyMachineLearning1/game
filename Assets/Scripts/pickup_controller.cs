using UnityEngine;
using System.Collections.Generic;

public class PickupController : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float holdDistance = 2.0f;
    [SerializeField] private Vector3 holdOffsetLocal = new Vector3(0, -0.2f, -1.5f);
    [SerializeField] private float minHoldHeight = -2f;

    private GameObject heldObject;
    private Rigidbody heldRb;
    private List<Collider> heldColliders = new List<Collider>();
    private Collider playerCollider;

    void Start()
    {
        playerCollider = GetComponent<Collider>();
        if (playerCollider == null)
        {
            playerCollider = GetComponentInChildren<Collider>();
        }
    }

    void Update()
    {
        Camera cam = Camera.main;

        if (heldObject == null)
        {
            if (Input.GetMouseButtonDown(0))
                TryPickup();
        }
        else
        {
            Vector3 desiredPosition = cam.transform.position + cam.transform.forward * holdDistance + cam.transform.up * holdOffsetLocal.y + cam.transform.right * holdOffsetLocal.x;

            float playerY = transform.position.y - 0.5f;
            if (desiredPosition.y < playerY + minHoldHeight)
            {
                desiredPosition.y = playerY + minHoldHeight;
            }

            Quaternion desiredRotation = cam.transform.rotation;
            desiredRotation = Quaternion.Euler(0, desiredRotation.eulerAngles.y, 0);

            heldObject.transform.position = desiredPosition;
            heldObject.transform.rotation = desiredRotation;

            if (Input.GetMouseButtonDown(0))
                DropObject(false);
            if (Input.GetMouseButtonDown(1))
                DropObject(true);
        }
    }

    void TryPickup()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera не найдена!");
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            GameObject target = hit.collider.gameObject;
            while (target != null && !target.CompareTag("Pickup"))
            {
                target = target.transform.parent?.gameObject;
            }

            if (target != null)
            {
                heldObject = target;
                heldRb = target.GetComponent<Rigidbody>();
                
                if (heldRb == null)
                {
                    Rigidbody[] childRbs = target.GetComponentsInChildren<Rigidbody>();
                    if (childRbs.Length > 0)
                        heldRb = childRbs[0];
                }

                if (heldRb != null)
                {
                    heldRb.isKinematic = true;
                    heldRb.detectCollisions = false;
                    heldObject.transform.SetParent(null);

                    // Отключаем коллизии между игроком и объектом
                    DisableCollisions();

                    if (GameManager.Instance != null)
                        GameManager.Instance.OnPickup();
                }
            }
        }
    }

    void DisableCollisions()
    {
        if (playerCollider == null) return;

        Collider[] objectColliders = heldObject.GetComponentsInChildren<Collider>();
        heldColliders.Clear();

        foreach (Collider col in objectColliders)
        {
            heldColliders.Add(col);
            Physics.IgnoreCollision(playerCollider, col, true);
        }
    }

    void EnableCollisions()
    {
        if (playerCollider == null) return;

        foreach (Collider col in heldColliders)
        {
            Physics.IgnoreCollision(playerCollider, col, false);
        }
        heldColliders.Clear();
    }

    void DropObject(bool thrown)
    {
        // Включаем коллизии обратно
        EnableCollisions();

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.detectCollisions = true;

            if (thrown)
                heldRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }

        heldObject = null;
        heldRb = null;
    }

    void OnDrawGizmosSelected()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * pickupRange);
        }
    }
}
