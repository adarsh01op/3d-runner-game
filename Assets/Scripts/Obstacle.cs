using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public AudioClip hitSound;

    [Header("Animation Timings")]
    public float deathAnimLength = 2f;          // length of DIE animation
    public float hurtAnimLength = 1f;         // length of HURT animation
    public float invulnerabilityAfterHit = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null && pc.IsInvulnerable())
            return; // ignore hit while invulnerable

        // Play hit sound near camera
        if (hitSound != null)
        {
            Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(hitSound, pos);
        }

        Animator anim = other.GetComponent<Animator>();

        if (GameManager.instance != null)
        {
            // Reduce hearts first
            GameManager.instance.LoseHeart();

            if (GameManager.instance.IsGameOver())
            {
                // No hearts left ? Death
                if (anim != null)
                    anim.SetTrigger("Die");

                GameManager.instance.GameOver(deathAnimLength);
            }
            else
            {
                // Still have hearts ? Hurt
                if (anim != null)
                    anim.SetTrigger("Hurt");

                if (pc != null)
                {
                    pc.StunAndRecover(hurtAnimLength, invulnerabilityAfterHit);
                }
            }
        }
    }
}
