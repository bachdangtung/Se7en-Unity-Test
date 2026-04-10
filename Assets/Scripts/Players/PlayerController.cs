using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
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

    [Header("Input")]
    [SerializeField] private string moveActionName = "Move";

    [Header("Animator")]
    [SerializeField] private string blendParameterName = "Blend";
    [SerializeField] private string normalBoolName = "normal";
    [SerializeField] private bool forceNormalStatus = true;

    private CharacterController cc;
    private Animator anim;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private Vector2 moveInput;

    private int blendHash;
    private int normalHash;
    private bool hasBlendParameter;
    private bool hasNormalParameter;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        CacheAnimatorParameters();

        if (playerInput != null && playerInput.actions != null)
        {
            moveAction = playerInput.actions[moveActionName];
        }

        if (moveAction == null)
        {
            Debug.LogWarning($"PlayerController: Cannot find input action '{moveActionName}'.", this);
        }
    }

    private void CacheAnimatorParameters()
    {
        blendHash = Animator.StringToHash(blendParameterName);
        normalHash = Animator.StringToHash(normalBoolName);

        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.name == blendParameterName && parameter.type == AnimatorControllerParameterType.Float)
            {
                hasBlendParameter = true;
            }

            if (parameter.name == normalBoolName && parameter.type == AnimatorControllerParameterType.Bool)
            {
                hasNormalParameter = true;
            }
        }

        if (!hasBlendParameter)
        {
            Debug.LogWarning($"PlayerController: Animator is missing float parameter '{blendParameterName}'.", this);
        }

        if (forceNormalStatus && !hasNormalParameter)
        {
            Debug.LogWarning($"PlayerController: Animator is missing bool parameter '{normalBoolName}'.", this);
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        ReadMoveInput();
        HandleMovement();
    }

    private void ReadMoveInput()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        float inputMagnitude = moveInput.sqrMagnitude;

        if (hasBlendParameter)
        {
            anim.SetFloat(blendHash, moveInput.magnitude, 0.1f, Time.deltaTime);
        }

        if (forceNormalStatus && hasNormalParameter)
        {
            anim.SetBool(normalHash, true);
        }

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

}