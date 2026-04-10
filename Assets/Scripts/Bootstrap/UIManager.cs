using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button kickButton;
    [SerializeField] private Button autoKickButton;
    [SerializeField] private Button resetButton;

    public event Action KickClicked;
    public event Action AutoKickClicked;
    public event Action ResetClicked;

    private void OnEnable()
    {
        ToggleButtonListener(kickButton, HandleKickClicked, true);
        ToggleButtonListener(autoKickButton, HandleAutoKickClicked, true);
        ToggleButtonListener(resetButton, HandleResetClicked, true);
    }

    private void OnDisable()
    {
        ToggleButtonListener(kickButton, HandleKickClicked, false);
        ToggleButtonListener(autoKickButton, HandleAutoKickClicked, false);
        ToggleButtonListener(resetButton, HandleResetClicked, false);
    }

    private void Start()
    {
        ShowKickButton(false);
    }

    public void ShowKickButton(bool show)
    {
        if (kickButton != null)
        {
            kickButton.gameObject.SetActive(show);
        }
    }

    private void HandleKickClicked()
    {
        KickClicked?.Invoke();
    }

    private void HandleAutoKickClicked()
    {
        AutoKickClicked?.Invoke();
    }

    private void HandleResetClicked()
    {
        ResetClicked?.Invoke();
    }

    private static void ToggleButtonListener(Button button, UnityEngine.Events.UnityAction action, bool subscribe)
    {
        if (button == null)
        {
            return;
        }

        if (subscribe)
        {
            button.onClick.AddListener(action);
        }
        else
        {
            button.onClick.RemoveListener(action);
        }
    }
}