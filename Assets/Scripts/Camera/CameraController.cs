using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraState { FollowPlayer, FollowBall }

    [Header("Targets")]
    public Transform player;
    private Transform trackedBall;

    [Header("Top-Down Offset")]
    public Vector3 offset = new Vector3(0f, 80f, -80f);
    public float smoothSpeed = 8f;

    public CameraState State { get; private set; } = CameraState.FollowPlayer;
    private Coroutine returnCoroutine;

    void LateUpdate()
    {
        Transform target = (State == CameraState.FollowBall && trackedBall != null) ? trackedBall : player;
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);

        transform.LookAt(target.position);
    }

    public void FollowBall(Transform ballTransform)
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        trackedBall = ballTransform;
        State = CameraState.FollowBall;
    }

    public void FollowPlayer()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        trackedBall = null;
        State = CameraState.FollowPlayer;
    }

    public void ReturnToPlayerAfter(float delay)
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }

        returnCoroutine = StartCoroutine(ReturnToPlayerAfterRoutine(delay));
    }

    private IEnumerator ReturnToPlayerAfterRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        FollowPlayer();
    }
}