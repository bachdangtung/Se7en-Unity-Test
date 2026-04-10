using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    [Header("Field Bounds")]
    public float minX = -22f;
    public float maxX = 22f;
    public float minZ = -14f;
    public float maxZ = 14f;

    [Header("Detection")]
    public float kickDetectRange = 3f;
    public float detectionInterval = 0.2f;

    [Header("Input")]
    [Tooltip("Use Input.GetAxis fallback when PlayerInput callbacks are not configured.")]
    public bool useLegacyInputFallback = true;

    private CharacterController cc;
    private Animator anim;
    private Vector2 moveInput;
    private BallController[] balls;
    private BallController nearestBall;
    private bool wasNearBall = false;
    private float nextDetectionTime;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    public Action<BallController> OnNearBall;
    public Action OnLeaveBall;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        UpdateInput();
        HandleMovement();

        if (Time.time >= nextDetectionTime)
        {
            HandleBallDetection();
            nextDetectionTime = Time.time + detectionInterval;
        }
    }

    private void UpdateInput()
    {
        if (!useLegacyInputFallback)
        {
            return;
        }

        if (moveInput.sqrMagnitude <= 0.0001f)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(horizontal, vertical);
        }
    }

    void HandleMovement()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        float inputMagnitude = moveInput.sqrMagnitude;

        anim.SetFloat(SpeedHash, moveInput.magnitude, 0.1f, Time.deltaTime);

        if (inputMagnitude > 0.01f)
        {
            cc.Move(moveDir * moveSpeed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            ClampPosition();
        }

        if (!cc.isGrounded)
        {
            cc.Move(Vector3.down * 9.8f * Time.deltaTime);
        }
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        float clampedX = Mathf.Clamp(pos.x, minX, maxX);
        float clampedZ = Mathf.Clamp(pos.z, minZ, maxZ);

        if (pos.x != clampedX || pos.z != clampedZ)
        {
            cc.enabled = false;
            transform.position = new Vector3(clampedX, pos.y, clampedZ);
            cc.enabled = true;
        }
    }

    void HandleBallDetection()
    {
        if (balls == null || balls.Length == 0)
        {
            balls = FindObjectsOfType<BallController>();
        }

        BallController closest = null;
        float minDist = float.MaxValue;

        foreach (var ball in balls)
        {
            if (ball == null) continue;
            if (ball.IsFlying) continue;

            float d = Vector3.Distance(transform.position, ball.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = ball;
            }
        }

        bool isNear = closest != null && minDist <= kickDetectRange;

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

    public BallController GetNearestBall() => nearestBall;

    public BallController GetFarthestBall()
    {
        if (balls == null || balls.Length == 0)
        {
            balls = FindObjectsOfType<BallController>();
        }

        BallController farthest = null;
        float maxDistSqr = -1f;

        foreach (var ball in balls)
        {
            if (ball == null) continue;
            if (ball.IsFlying) continue;

            float distSqr = (transform.position - ball.transform.position).sqrMagnitude;
            if (distSqr > maxDistSqr)
            {
                maxDistSqr = distSqr;
                farthest = ball;
            }
        }
        return farthest;
    }
}