using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed;
    public float damage;
    public float lifetime = 10f; // Lifetime before bullet is destroyed
    public float lineRenderDelay = 0.02f; // Delay before line appears
    public float lineLength = 3f; // Length of the line behind the bullet

    private Vector3 direction;
    private LineRenderer lineRenderer;
    private bool lineActive = false;
    private float elapsedTime = 0f;

    public void Initialize(Vector3 bulletVelocity, float bulletDamage)
    {
        // Set bullet properties
        speed = bulletVelocity.magnitude;
        direction = bulletVelocity.normalized;
        damage = bulletDamage;

        // Add and configure the LineRenderer component
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.red;

        // Set line renderer to inactive initially
        lineRenderer.enabled = false;

        // Schedule bullet destruction
        Invoke("DestroyBullet", lifetime);
    }

    private void Update()
    {
        // Move the bullet
        transform.position += direction * speed * Time.deltaTime;

        // Update elapsed time and enable line renderer after delay
        elapsedTime += Time.deltaTime;
        if (!lineActive && elapsedTime >= lineRenderDelay)
        {
            lineActive = true;
            lineRenderer.enabled = true;
        }

        // Update the line renderer's positions if line is active
        if (lineActive && lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position - direction * lineLength);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthBar>().TakeDamage(damage);
        }
        DestroyBullet();
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
