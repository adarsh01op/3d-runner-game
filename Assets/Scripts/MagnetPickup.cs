using UnityEngine;

public class MagnetPickup : MonoBehaviour
{
    public float magnetDuration = 10f; // how long effect lasts
    public float rotateSpeed = 180f; // degrees per second
    public AudioClip collectSound;

    private void Update()
    {
        // rotate around Y axis
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ActivateMagnet(magnetDuration);
            }

            // destroy the pickup
            Destroy(gameObject);
        }
    }
}
