using System;
using System.Collections;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Kick Settings")]
    public float flightDuration = 1.2f;
    public float arcHeight = 4f;
    public float spinSpeed = 360f;
    public float hideAfterGoalDelay = 2f;

    [Header("Goals")]
    [SerializeField] private Transform[] goals;

    [Header("Effects")]
    public GameObject confettiPrefab;
    [SerializeField] private int maxSharedConfettiInstances = 20;
    [SerializeField] private float confettiAutoDisableDelay = 4f;

    public bool IsFlying { get; private set; } = false;
    public bool IsInGoal { get; private set; } = false;

    // Events
    public Action OnBallLanded;
    public Action OnBallKicked;

    private Vector3 startPos;
    private Quaternion startRot;
    private Renderer[] cachedRenderers;
    private Collider[] cachedColliders;

    private void Awake()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider>(true);
    }

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void KickToNearestGoal()
    {
        if (IsFlying || IsInGoal) return;

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
        IsInGoal = true;
        SetBallCollidable(false);

        if (confettiPrefab != null)
            SpawnConfetti(targetPos);

        StartCoroutine(HideBallAfterGoalDelay());

        OnBallLanded?.Invoke();
    }

    private IEnumerator HideBallAfterGoalDelay()
    {
        yield return new WaitForSeconds(hideAfterGoalDelay);
        SetBallVisible(false);
    }

    private void SpawnConfetti(Vector3 position)
    {
        GameObject effect = ConfettiPool.Rent(confettiPrefab, maxSharedConfettiInstances);
        if (effect == null)
        {
            return;
        }

        effect.transform.SetPositionAndRotation(position, Quaternion.identity);
        effect.SetActive(true);

        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        float maxLifetime = 0f;

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.gameObject.SetActive(true);
            ps.Clear(true);
            ps.Play(true);

            ParticleSystem.MainModule main = ps.main;
            float life = main.duration + main.startLifetime.constantMax;
            if (life > maxLifetime)
            {
                maxLifetime = life;
            }
        }

        StartCoroutine(ReturnConfettiToPool(effect, Mathf.Max(confettiAutoDisableDelay, maxLifetime)));
    }

    private IEnumerator ReturnConfettiToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (effect == null || confettiPrefab == null)
        {
            yield break;
        }

        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ConfettiPool.Return(confettiPrefab, effect);
    }

    private void SetBallVisibleAndCollidable(bool value)
    {
        SetBallVisible(value);
        SetBallCollidable(value);
    }

    private void SetBallVisible(bool value)
    {
        foreach (Renderer r in cachedRenderers)
        {
            if (r != null)
            {
                r.enabled = value;
            }
        }
    }

    private void SetBallCollidable(bool value)
    {
        foreach (Collider c in cachedColliders)
        {
            if (c != null)
            {
                c.enabled = value;
            }
        }
    }

    public void ResetBall()
    {
        StopAllCoroutines();
        IsFlying = false;
        IsInGoal = false;
        transform.position = startPos;
        transform.rotation = startRot;
        SetBallVisibleAndCollidable(true);

        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}