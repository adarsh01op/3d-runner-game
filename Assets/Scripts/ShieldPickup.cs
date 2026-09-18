using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    public float shieldDuration = 15f;     // seconds of invulnerability
    public AudioClip pickupSound;         // sound when shield is collected
    public GameObject pickupEffect;       // optional particle effect (destroyed on pickup)
    public float rotateSpeed = 180f; // degrees per second

    private void Update()
    {
        // rotate around Y axis
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ActivateShield(shieldDuration);

            // Play pickup sound
            if (pickupSound != null)
            {
                Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(pickupSound, pos);
            }

            // Spawn visual effect
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject); // remove pickup from scene
        }
    }
}
