using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    public AudioClip collectSound;
    public float rotateSpeed = 180f; // degrees per second
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

            GameManager.instance.GainHeart();

            Destroy(gameObject);
        }
    }
}
