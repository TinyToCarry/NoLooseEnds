using UnityEngine;

public class ThrowableGun : MonoBehaviour
{
    public float damage = 20f;
    private bool hasCollided = false;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>(); // Add Rigidbody if missing
        }
    }

    // Method to initialize the throw with a specific force
    public void InitializeThrow(float throwForce)
    {
        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided)
        {
            hasCollided = true;

            // Destroy the gun 3 seconds after collision
            Destroy(gameObject, 3f);
        }
    }
}


