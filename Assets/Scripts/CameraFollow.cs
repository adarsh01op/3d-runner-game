using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -6);
    public float smoothSpeed = 8f;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if(playerObj !=  null)
        {
            target = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No GameObject found with tag 'Player'!");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}
