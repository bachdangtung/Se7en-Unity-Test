using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private UIManager uiManager;

    [Header("Timing")]
    [SerializeField] private float returnCameraDelayAfterGoal = 2f;

    private BallController nearbyBall;
    private BallController activeBall;

    private void Awake()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnNearBall += HandleNearBall;
            player.OnLeaveBall += HandleLeaveBall;
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
        if (player != null)
        {
            player.OnNearBall -= HandleNearBall;
            player.OnLeaveBall -= HandleLeaveBall;
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
        if (player == null)
        {
            return;
        }

        BallController farthest = player.GetFarthestBall();
        if (farthest == null || farthest.IsFlying)
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
}
