using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems; // Added for UI touch and Simulator detection

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement (forward & lanes)")]
    public float forwardSpeed = 6f;            // starting forward speed
    public float speedIncreaseRate = 0.03f;    // units/sec added each second
    public float maxSpeed = 18f;
    public float laneDistance = 2.9f;          // distance between lanes (center-left/right)
    public float laneChangeSpeed = 8f;         // how quickly we interpolate horizontally

    [Header("Jump")]
    public float jumpHeight = 1.8f;            // meters
    public float gravity = -20f;               // negative value

    [Header("Slide")]
    [Tooltip("How long slide lasts (seconds)")]
    public float slideDuration = 0.8f;
    [Range(0.25f, 1f)]
    public float slideHeightMultiplier = 0.5f; // multiplies original CharacterController height

    [Header("Input / Touch")]
    public float swipeThreshold = 50f;         // pixels, min distance to count as swipe

    // internal
    CharacterController controller;
    Animator animator;

    int currentLane = 1; // 0 = left, 1 = center, 2 = right
    float verticalVelocity = 0f;
    float initialForwardSpeed;

    // store original controller dims so we can restore after slide
    float originalControllerHeight;
    Vector3 originalControllerCenter;
    bool isSliding = false;

    // touch detection
    Vector2 touchStartPos;
    bool touchStarted = false;

    private bool isInvulnerable = false;
    private bool isStunned = false;

    [Header("Magnet Powerup")]
    public float magnetRadius = 8f; // how fat it attracts coins
    public float magnetSpeed = 10f; // how fast coins move toward player
    private bool isMagnetActive = false;
    private Coroutine magnetCoroutine; // Stores the coroutine so it can be overwritten safely

    [Header("Power-ups")]
    public GameObject shieldVisual; // assign a shield/glow object around player 
    private bool shieldActive = false;
    private float shieldTimer = 0f;

    [HideInInspector]
    public bool isFlying = false;


    [Header("Hoverboard Settings")]
    public GameObject HoverboardObject;
    public Transform hoverBoardAttachPoint;
    public float hoverboardSpinSpeed = 360f;
    public float hoverboardReturnDelay = 0.8f;
    public float hoverboardReturnSpeed = 5f;

    private bool hoverboardDetached = false;
    private Quaternion hoverOriginalRot;
    public Vector3 hoverOriginalLocalPos;
    private bool isHovering = false;
    private float hoverTimer = 0f;


    // Expose check for obstacle
    public bool IsInvulnerable() => isInvulnerable;

    // Calll this to give temprary invulnerability(non-blocking)
    public void MakeInvulnerable(float duration)
    {
        StartCoroutine(InvulnerableRoutine(duration));
    }

    private IEnumerator InvulnerableRoutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public void StunAndRecover(float stunDuration, float invulnDuration)
    {
        StartCoroutine(StunAndRecoverRoutine(stunDuration, invulnDuration));
    }

    public void ActivateMagnet(float duration)
    {
        if (magnetCoroutine != null)
        {
            StopCoroutine(magnetCoroutine);
        }
        magnetCoroutine = StartCoroutine(MagnetRoutine(duration));
    }

    // Hover board
    public void ActivateHoverboard(float duration)
    {
        if (isHovering) return;
        
        isHovering = true;
        hoverTimer = duration;
        // Activate hoverboard visuals
        if (HoverboardObject != null)
            HoverboardObject.SetActive(true);
        // Start the hoverboard routine
        animator.SetBool("isHovering", true);
        MakeInvulnerable(duration);
    }




    private IEnumerator MagnetRoutine(float duration)
    {
        isMagnetActive = true;
        yield return new WaitForSeconds(duration);
        isMagnetActive = false;
    }

    private IEnumerator StunAndRecoverRoutine(float stunDuration, float invulnDuration)
    {
        isStunned = true;

        float savedSpeed = Mathf.Max(forwardSpeed, initialForwardSpeed);

        // stop movement
        forwardSpeed = 0f;
        verticalVelocity = 0f;

        animator.ResetTrigger("Die");
        animator.SetTrigger("Hurt");

        yield return new WaitForSeconds(stunDuration);

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Running"))
            yield return null;

        forwardSpeed = Mathf.Max(savedSpeed, initialForwardSpeed);

        isStunned = false;

        animator.ResetTrigger("Hurt");
        animator.SetFloat("ForwardSpeed", forwardSpeed);

        if (invulnDuration > 0f)
            MakeInvulnerable(invulnDuration);
    }

    void Start()
    {
        if(HoverboardObject != null)
        {
            hoverOriginalRot = HoverboardObject.transform.localRotation;
            hoverOriginalLocalPos = HoverboardObject.transform.localPosition;
         
        }


        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        originalControllerHeight = controller.height;
        originalControllerCenter = controller.center;
        initialForwardSpeed = forwardSpeed;

        Application.targetFrameRate = 60;
    }

    void Update()
    {

        if(isHovering)
        {
            hoverTimer -= Time.deltaTime;
            if (hoverTimer <= 0f)
            {
                isHovering = false;
               
                if (HoverboardObject != null)
                    HoverboardObject.SetActive(false);

                animator.SetBool("isHovering", false);
            }
        }


        if(isFlying)
        {
            // Stilll alloww lane change input while flying 
            HandleInput();

            // Calculate lane target position 
            float targetX = (currentLane - 1) * laneDistance;
            float deltaX = targetX - transform.position.x;
            float lateral = deltaX * laneChangeSpeed;

            //skip gravity and vertical move when flying
            Vector3 flyMove = Vector3.forward * forwardSpeed + Vector3.right * lateral;

            controller.Move(flyMove * Time.deltaTime);

            //Still update animations if needed
            UpdateAnimator();
            RampSpeed();

            return;
        }

        // Stop updating if game is over
        if (isStunned || (GameManager.instance != null && GameManager.instance.IsGameOver()) ||
            (GameManager.instance != null && !GameManager.instance.IsGameStarted()))
            return;

        // Tick shield timer

        if (shieldActive)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
                DeactivateShield();
        }

        if (isMagnetActive)
        {
            AttractCoins();
        }

        HandleInput();
        ApplyMovement();
        UpdateAnimator();
        RampSpeed();
    }

    public void ActivateShield(float duration)
    {
        shieldActive = true;
        shieldTimer = duration;
        MakeInvulnerable(duration);

        if (shieldVisual != null)
            shieldVisual.SetActive(true);
    }

    private void DeactivateShield()
    {
        shieldActive = false;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    void AttractCoins()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, magnetRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Coin"))
            {
                hit.transform.position = Vector3.MoveTowards(
                    hit.transform.position,
                    transform.position,
                    magnetSpeed * Time.deltaTime
                );
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            ChangeLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            ChangeLane(1);
        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.S))
            TrySlide();

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            // Ignore touch if it is over a UI element (fixes Simulator controls)
           if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
               return;

            if (t.phase == TouchPhase.Began)
            {
                touchStartPos = t.position;
                touchStarted = true;
            }
            else if (t.phase == TouchPhase.Ended && touchStarted)
            {
                Vector2 end = t.position;
                DetectSwipe(touchStartPos, end);
                touchStarted = false;
            }
        }
        else
        {
            // Ignore mouse click if it is over a UI element
           if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                touchStarted = true;
            }
            else if (Input.GetMouseButtonUp(0) && touchStarted)
            {
                Vector2 end = (Vector2)Input.mousePosition;
                DetectSwipe(touchStartPos, end);
                touchStarted = false;
            }
        }
    }

    void DetectSwipe(Vector2 start, Vector2 end)
    {
        Vector2 delta = end - start;
        if (delta.magnitude < swipeThreshold) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0) ChangeLane(1); else ChangeLane(-1);
        }
        else
        {
            if (delta.y > 0) TryJump(); else TrySlide();
        }
    }

    void ChangeLane(int dir)
    {
        currentLane = Mathf.Clamp(currentLane + dir, 0, 2);
    }

    void ApplyMovement()
    {
        float targetX = (currentLane - 1) * laneDistance;
        float deltaX = targetX - transform.position.x;
        float lateral = deltaX * laneChangeSpeed;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = Vector3.forward * forwardSpeed + Vector3.right * lateral + Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }

    void TryJump()
    {
        if (controller.isGrounded && !isSliding)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null) animator.SetTrigger("Jump");

            if(isHovering && HoverboardObject !=null && !hoverboardDetached)
            {
                StartCoroutine(DetachAndSpinHoverboard());
            }
        }
    }
    

    private IEnumerator DetachAndSpinHoverboard()
    {
        hoverboardDetached = true;
        // Detach hoverboard
        HoverboardObject.transform.SetParent(null);
        // Add spin effect
        float elapsed = 0f;
        float duration = hoverboardReturnDelay;


        Vector3 hoverVelocity = transform.forward * forwardSpeed;

        //Spin hoverboard while player is in air
        while (elapsed < duration)
        {
            // Keep moving forward roughly with player
            HoverboardObject.transform.position += hoverVelocity * Time.deltaTime;
            //spin
            HoverboardObject.transform.Rotate(Vector3.forward * hoverboardSpinSpeed * Time.deltaTime , Space.Self);
            elapsed += Time.deltaTime;
            yield return null;
        }

        //Quaternion startRot = HoverboardObject.transform.rotation;
        //Quaternion endRot = startRot * Quaternion.Euler(0, hoverboardSpeed, 0);
        //while (elapsed < hoverboardReturnDelay)
        //{
        //    elapsed += Time.deltaTime;
        //    float t = elapsed / hoverboardReturnDelay;
        //    HoverboardObject.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
        //    yield return null;
        //}

        // Wait until player lands
        while (!controller.isGrounded)
            yield return null;

        
       
        // Smoothly reattach hoverboard to player feet
        elapsed = 0f;
        Vector3 startPos = HoverboardObject.transform.position;
        Quaternion startRot = HoverboardObject.transform.rotation;

        Vector3 targetPos = hoverBoardAttachPoint.position;
        Quaternion targetRot = hoverOriginalRot;


        while (elapsed < (1f / hoverboardReturnSpeed))
        {
            elapsed += Time.deltaTime;
            //float t = elapsed / hoverboardReturnDelay;
            HoverboardObject.transform.position = Vector3.Lerp(startPos, targetPos, elapsed*hoverboardReturnSpeed);
            HoverboardObject.transform.rotation = Quaternion.Slerp(HoverboardObject.transform.rotation, targetRot, elapsed*hoverboardReturnSpeed);
            yield return null;
        }
        // Reattach hoverboard
        HoverboardObject.transform.SetParent(hoverBoardAttachPoint);
        HoverboardObject.transform.localPosition = hoverOriginalLocalPos;
        HoverboardObject.transform.localRotation = hoverOriginalRot;
        hoverboardDetached = false;
    }

    void TrySlide()
    {
        if (controller.isGrounded && !isSliding)
            StartCoroutine(DoSlide());
    }

    IEnumerator DoSlide()
    {
        isSliding = true;
        if (animator != null) animator.SetTrigger("Slide");

        float newHeight = originalControllerHeight * slideHeightMultiplier;
        float centerY = originalControllerCenter.y - (originalControllerHeight - newHeight) / 2f;

        controller.height = newHeight;
        controller.center = new Vector3(originalControllerCenter.x, centerY, originalControllerCenter.z);

        float savedSpeed = forwardSpeed;

        yield return new WaitForSeconds(slideDuration);

        controller.height = originalControllerHeight;
        controller.center = originalControllerCenter;
        forwardSpeed = savedSpeed;
        isSliding = false;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat("ForwardSpeed", forwardSpeed);
        animator.SetBool("isGrounded", controller.isGrounded);
        animator.SetBool("isSliding", isSliding);
    }

    void RampSpeed()
    {
        forwardSpeed = Mathf.Clamp(forwardSpeed + speedIncreaseRate * Time.deltaTime, initialForwardSpeed, maxSpeed);
    }
}