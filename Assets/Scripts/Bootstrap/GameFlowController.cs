using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerBallSensor ballSensor;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private UIManager uiManager;

    [Header("Timing")]
    [SerializeField] private float returnCameraDelayAfterGoal = 2f;

    private BallController nearbyBall;
    private BallController activeBall;

    private void Awake()
    {
        ballSensor = ResolveReference(ballSensor);

        if (ballSensor == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                ballSensor = player.GetComponent<PlayerBallSensor>();
                if (ballSensor == null)
                {
                    ballSensor = player.gameObject.AddComponent<PlayerBallSensor>();
                }
            }
        }

        cameraController = ResolveReference(cameraController);
        uiManager = ResolveReference(uiManager);

        if (ballSensor == null)
        {
            Debug.LogWarning("GameFlowController: Missing PlayerBallSensor in scene.", this);
        }
    }

    private void OnEnable()
    {
        if (ballSensor != null)
        {
            ballSensor.OnNearBall += HandleNearBall;
            ballSensor.OnLeaveBall += HandleLeaveBall;
        }

        if (uiManager != null)
        {
            uiManager.KickClicked += HandleKickClicked;
            uiManager.AutoKickClicked += HandleAutoKickClicked;
            uiManager.ResetClicked += HandleResetClicked;
        }
    }

    private void OnDisable()
    {
        if (ballSensor != null)
        {
            ballSensor.OnNearBall -= HandleNearBall;
            ballSensor.OnLeaveBall -= HandleLeaveBall;
        }

        if (uiManager != null)
        {
            uiManager.KickClicked -= HandleKickClicked;
            uiManager.AutoKickClicked -= HandleAutoKickClicked;
            uiManager.ResetClicked -= HandleResetClicked;
        }

        UnsubscribeActiveBall();
    }

    private void HandleNearBall(BallController ball)
    {
        nearbyBall = ball;
        uiManager?.ShowKickButton(ball != null && !ball.IsFlying);
    }

    private void HandleLeaveBall()
    {
        nearbyBall = null;
        uiManager?.ShowKickButton(false);
    }

    private void HandleKickClicked()
    {
        if (nearbyBall == null || nearbyBall.IsFlying)
        {
            return;
        }

        KickBall(nearbyBall);
        uiManager?.ShowKickButton(false);
    }

    private void HandleAutoKickClicked()
    {
        if (ballSensor == null)
        {
            return;
        }

        BallController farthest = ballSensor.GetFarthestBall();
        if (farthest == null || farthest.IsFlying || farthest.IsInGoal)
        {
            return;
        }

        KickBall(farthest);
        uiManager?.ShowKickButton(false);
    }

    private void KickBall(BallController ball)
    {
        if (ball == null || cameraController == null)
        {
            return;
        }

        UnsubscribeActiveBall();
        activeBall = ball;
        activeBall.OnBallLanded += HandleBallLanded;

        cameraController.FollowBall(activeBall.transform);
        activeBall.KickToNearestGoal();
    }

    private void HandleBallLanded()
    {
        cameraController?.ReturnToPlayerAfter(returnCameraDelayAfterGoal);
        UnsubscribeActiveBall();
    }

    private void UnsubscribeActiveBall()
    {
        if (activeBall != null)
        {
            activeBall.OnBallLanded -= HandleBallLanded;
            activeBall = null;
        }
    }

    private void HandleResetClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static T ResolveReference<T>(T target) where T : Object
    {
        if (target != null)
        {
            return target;
        }

        return FindObjectOfType<T>();
    }
}
