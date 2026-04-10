using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerBallSensor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float kickDetectRange = 3f;
    [SerializeField] private float detectionInterval = 0.2f;

    private BallController[] balls;
    private BallController nearestBall;
    private bool wasNearBall;
    private float nextDetectionTime;

    public event Action<BallController> OnNearBall;
    public event Action OnLeaveBall;

    private void Update()
    {
        if (Time.time < nextDetectionTime)
        {
            return;
        }

        DetectNearestBall();
        nextDetectionTime = Time.time + Mathf.Max(0.01f, detectionInterval);
    }

    public BallController GetNearestBall()
    {
        return nearestBall;
    }

    public BallController GetFarthestBall()
    {
        EnsureBallCache();

        BallController farthest = null;
        float maxDistanceSqr = -1f;

        foreach (BallController ball in balls)
        {
            if (!IsKickable(ball))
            {
                continue;
            }

            float distanceSqr = (transform.position - ball.transform.position).sqrMagnitude;
            if (distanceSqr > maxDistanceSqr)
            {
                maxDistanceSqr = distanceSqr;
                farthest = ball;
            }
        }

        return farthest;
    }

    private void DetectNearestBall()
    {
        EnsureBallCache();

        BallController closest = null;
        float minDistanceSqr = float.MaxValue;

        foreach (BallController ball in balls)
        {
            if (!IsKickable(ball))
            {
                continue;
            }

            float distanceSqr = (transform.position - ball.transform.position).sqrMagnitude;
            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                closest = ball;
            }
        }

        bool isNear = closest != null && minDistanceSqr <= kickDetectRange * kickDetectRange;

        if (isNear && (!wasNearBall || closest != nearestBall))
        {
            nearestBall = closest;
            OnNearBall?.Invoke(closest);
        }
        else if (!isNear && wasNearBall)
        {
            nearestBall = null;
            OnLeaveBall?.Invoke();
        }

        wasNearBall = isNear;
    }

    private void EnsureBallCache()
    {
        if (balls == null || balls.Length == 0)
        {
            balls = FindObjectsOfType<BallController>();
        }
    }

    private static bool IsKickable(BallController ball)
    {
        return ball != null && !ball.IsFlying && !ball.IsInGoal;
    }
}
