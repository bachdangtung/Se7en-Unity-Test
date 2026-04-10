using System;
using System.Collections;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Kick Settings")]
    public float flightDuration = 1.2f;
    public float arcHeight = 4f;
    public float spinSpeed = 360f;

    [Header("Goals")]
    [SerializeField] private Transform[] goals;

    [Header("Effects")]
    public GameObject confettiPrefab;

    public bool IsFlying { get; private set; } = false;

    // Events
    public Action OnBallLanded;
    public Action OnBallKicked;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void KickToNearestGoal()
    {
        if (IsFlying) return;

        Transform target = GetNearestGoal();
        if (target != null)
        {
            OnBallKicked?.Invoke();
            StartCoroutine(FlyToGoal(target.position));
        }
    }

    public Transform GetNearestGoal()
    {
        if (goals == null || goals.Length == 0) return null;

        Transform nearest = null;
        float minD = float.MaxValue;

        foreach (var g in goals)
        {
            if (g == null) continue;
            float dSqr = (transform.position - g.position).sqrMagnitude;
            if (dSqr < minD)
            {
                minD = dSqr;
                nearest = g;
            }
        }
        return nearest;
    }

    private IEnumerator FlyToGoal(Vector3 targetPos)
    {
        IsFlying = true;
        Vector3 origin = transform.position;
        float elapsed = 0f;

        Vector3 travelDir = (targetPos - origin).normalized;
        Vector3 spinAxis = Vector3.Cross(Vector3.up, travelDir);

        if (spinAxis.sqrMagnitude < 0.0001f)
        {
            spinAxis = Vector3.right;
        }

        while (elapsed < flightDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flightDuration;

            Vector3 flatPos = Vector3.Lerp(origin, targetPos, t);

            float height = Mathf.Sin(t * Mathf.PI) * arcHeight;

            transform.position = new Vector3(flatPos.x, flatPos.y + height, flatPos.z);

            transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.World);

            yield return null;
        }

        transform.position = targetPos;
        IsFlying = false;

        if (confettiPrefab != null)
            Instantiate(confettiPrefab, targetPos, Quaternion.identity);

        OnBallLanded?.Invoke();
    }

    public void ResetBall()
    {
        StopAllCoroutines();
        IsFlying = false;
        transform.position = startPos;
        transform.rotation = startRot;

        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}