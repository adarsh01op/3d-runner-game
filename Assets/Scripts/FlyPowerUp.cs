using UnityEngine;

public class FlyPowerUp : MonoBehaviour
{
    public float flyDuration = 30f; // how long player flies
    public float flyHeight = 4.632f;  // how high above ground
    public AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerFlyController flyController = other.GetComponent<PlayerFlyController>();
            if (flyController != null)
            {
                flyController.ActivateFlyMode(flyDuration, flyHeight);
            }
            if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            Destroy(gameObject);

        }
    }
}