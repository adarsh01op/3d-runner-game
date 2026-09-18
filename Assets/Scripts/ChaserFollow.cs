using UnityEngine;
using System.Collections;

public class ChaserFollow : MonoBehaviour
{
    public Transform player;
    public float followDistance = 5f;
    public float followSpeed = 6f;
    public float catchUpSpeed = 18f;
    public float catchDistance = 1.2f;

     Animator animator;
    bool isFollowing = false;
    bool isCatching = false;





    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false; // ensure manual movement works

        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No GameObject found with tag 'Player'!");
        }
    }

    void Start()
    {
        if (player == null) Debug.LogError("ChaserFollow: player not assigned!");
    }

    void Update()
    {
        if (player == null) return;
        if (!isFollowing || isCatching) return;

        Vector3 desired = player.position - player.forward * followDistance;
        desired.y = transform.position.y;

        // face player
        Vector3 look = player.position - transform.position;
        look.y = 0;
        if (look.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.unscaledDeltaTime * 8f);

        // move toward desired pos
        transform.position = Vector3.MoveTowards(transform.position, desired, followSpeed * Time.unscaledDeltaTime);

        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
            StartCoroutine(CatchSequence());
    }

    public void StartFollowing()
    {
        if (player == null)
        {
            //Debug.LogError("ChaserFollow.StartFollowing: no player assigned.");
            return;
        }

        transform.position = player.position - player.forward * (followDistance + 2f);
        transform.rotation = Quaternion.LookRotation(player.position - transform.position);

        isFollowing = true;
        isCatching = false;
        if (animator != null) animator.SetBool("isRunning", true);
        //Debug.Log("ChaserFollow: StartFollowing called; pos = " + transform.position);
    }

    public void CatchPlayer()
    {
        if (!isCatching)
        {
            // Debug.Log("ChaserFollow: CatchPlayer called");
            StartCoroutine(CatchSequence());
        }
    }

    IEnumerator CatchSequence()
    {
        if (player == null) yield break;

        isCatching = true;
        isFollowing = false;

        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetTrigger("Catch");
        }

        var pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        float elapsed = 0f;
        float maxTime = 2.5f;
        while (Vector3.Distance(transform.position, player.position) > 0.6f && elapsed < maxTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, catchUpSpeed * Time.unscaledDeltaTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.position = player.position - player.forward * 0.5f;
        transform.rotation = Quaternion.LookRotation(player.forward);

        // Debug.Log("ChaserFollow: reached player");
        yield return new WaitForSecondsRealtime(1.0f);

        // leave player disabled; GameManager handles next steps
        isCatching = false;
    }
}
