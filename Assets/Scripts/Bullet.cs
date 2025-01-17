using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 40f;
    public float damage = 25f;
    public float lifetime = 10f;
    public float spread = 0.05f;
    public int bulletsPerShot = 1;

    private Vector3 direction;
    private LineRenderer lineRenderer;
    private bool lineActive = false;

    // Delay before rendering the line
    public float lineRenderDelay = 0.02f;  // Adjust this delay as needed
    public float lineLength = 3f;
    private float elapsedTime = 0f;

    public void Initialize(Vector3 targetPosition, bool isAiming, float spreadFactor)
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("Failed to add LineRenderer to bullet.");
            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.red;
        lineRenderer.enabled = false;

        // Calculate the base direction to the target
        Vector3 baseDirection = (targetPosition - transform.position).normalized;

        // Spread calculation based on aiming status
        float finalSpread = isAiming ? spreadFactor / 2 : spreadFactor;

        // Random yaw (horizontal) and pitch (vertical) angles within spread range
        float yaw = Random.Range(-finalSpread, finalSpread); // Horizontal spread angle
        float pitch = Random.Range(-finalSpread, finalSpread); // Vertical spread angle

        // Create a rotation from the spread angles relative to the base direction
        Quaternion spreadRotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 spreadDirection = spreadRotation * baseDirection;

        // Set the direction for the bullet with spread applied
        direction = spreadDirection;

        // Start the bullet's life
        Invoke("DestroyBullet", lifetime);
    }


    void Update()
    {
        // Move the bullet continuously
        transform.position += direction * speed * Time.deltaTime;

        // Check if delay has passed to enable line rendering
        elapsedTime += Time.deltaTime;
        if (!lineActive && elapsedTime >= lineRenderDelay)
        {
            lineActive = true;
            lineRenderer.enabled = true;
        }

        // Update line renderer positions every frame if line is active
        if (lineActive && lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineLength = 5f;  // Adjust to set line length
            lineRenderer.SetPosition(1, transform.position - direction * lineLength);
        }
    }

    void OnCollisionEnter()
    {
        // Implement collision logic here
        DestroyBullet();
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
