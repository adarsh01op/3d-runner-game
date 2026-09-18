using UnityEngine;

public class PickupDetector : MonoBehaviour
{
    private PlayerController player;
    public AudioClip collectSound;

    void Start()
    {
        // Cache reference to player controller
        player = GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
    //    // --- COIN ---
    //     if (other.CompareTag("Coin"))
    //      {
    //          GameManager.instance.AddCoin(1);
    //          Destroy(other.gameObject);
    //      }

    //    // --- DIAMOND ---
        if (other.CompareTag("Diamond"))
        {
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            GameManager.instance.AddDiamonds(1); // or a separate AddDiamond() if you have one
            Destroy(other.gameObject);
        }

    //     // --- HEART / EXTRA LIFE ---
    //      else if (other.CompareTag("Heart"))
    //      {
    //          GameManager.instance.GainHeart();
    //          Destroy(other.gameObject);
    //      }

    //      // --- SHIELD POWERUP ---
    //      else if (other.CompareTag("Shield"))
    //      {
    //          if (player != null)
    //              player.ActivateShield(5f); // 5 seconds of protection
    //          Destroy(other.gameObject);
    //      }

    //    // --- MAGNET POWERUP ---
    //     else if (other.CompareTag("Magnet"))
    //     {
    //         if (player != null)
    //             player.ActivateMagnet(8f); // 8 seconds of magnet
    //         Destroy(other.gameObject);
    //     }

    //     // --- JETPACK / FLY POWERUP ---
    //     else if (other.CompareTag("Jetpack"))
    //     {
    //         if (player != null)
    //             player.StartCoroutine("FlyRoutine"); // call your flying coroutine
    //         Destroy(other.gameObject);
    //     }
    }
}
