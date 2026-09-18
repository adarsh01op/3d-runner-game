using UnityEngine;

public class HoverboardPowerUp : MonoBehaviour
{
    public float duration = 7f;
    public float rotateSpeed = 180f;

    private void Update()
    {
        // Rotate the pickup for visual effect
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ActivateHoverboard(duration);
            }

            // Hide or destroy pickup
            Destroy(gameObject);
        }
    }
}
