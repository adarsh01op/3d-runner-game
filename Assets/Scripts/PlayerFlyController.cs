using System.Collections;
using UnityEngine;

public class PlayerFlyController : MonoBehaviour
{
    private bool isFlying = false;
    private float normalY;
    private CharacterController controller;
    private PlayerController movement;  // your movement script reference
    private Animator animator;

    public ParticleSystem flyEffect;  // optiona trail or jetpack particles
    public AudioSource flySound; // optional sound


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        normalY = transform.position.y; 
    }

    public void ActivateFlyMode(float duration, float flyHeight)
    {
        if (isFlying) return;
        StartCoroutine(FlyRoutine(duration, flyHeight));
    }

    private IEnumerator FlyRoutine(float duration, float flyHeight)
    {
        isFlying = true;
        if (movement != null) movement.isFlying = true; // tell movement script to pause gravity

        // Disable ground collision if needed
        controller.detectCollisions = false;

        // Start flying animation

        animator.SetBool("isFlying", true);

        //if (flyEffect) flyEffect.Play();
        //if (flySound) flySound.Play();

        Vector3 targetPos = new Vector3(transform.position.x, normalY + flyHeight, transform.position.z);

        // Rise smoothly

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            transform.position = Vector3.Lerp(transform.position, targetPos, t);
            yield return null;
        }

        // Stay flying
        yield return new WaitForSeconds(duration);

        //smooth land
        t = 0;
        Vector3 landPos = new Vector3(transform.position.x, normalY, transform.position.z);
        while(t<1)
        {
            t += Time.deltaTime * 2f;
            transform.position = Vector3.Lerp(transform.position, landPos, t);
            yield return null;
        }

        // Re-enable ground and reset
        controller.detectCollisions = true;
        animator.SetBool("isFlying", false);

        if (movement != null) movement.isFlying = false; // resume normal movement
        isFlying = false;

    }
    public bool IsFlying() => isFlying;
}
