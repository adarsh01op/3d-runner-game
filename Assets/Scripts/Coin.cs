using UnityEngine;

public class Coin : MonoBehaviour
{
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

            GameManager.instance.AddCoin(1);

            Destroy(gameObject);
        }
    }
}
