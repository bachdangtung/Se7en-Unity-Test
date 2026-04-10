using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button kickButton;
    public Button autoKickButton;
    public Button resetButton;

    public event Action KickClicked;
    public event Action AutoKickClicked;
    public event Action ResetClicked;

    private void OnEnable()
    {
        if (kickButton != null)
        {
            kickButton.onClick.AddListener(HandleKickClicked);
        }

        if (autoKickButton != null)
        {
            autoKickButton.onClick.AddListener(HandleAutoKickClicked);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(HandleResetClicked);
        }
    }

    private void OnDisable()
    {
        if (kickButton != null)
        {
            kickButton.onClick.RemoveListener(HandleKickClicked);
        }

        if (autoKickButton != null)
        {
            autoKickButton.onClick.RemoveListener(HandleAutoKickClicked);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(HandleResetClicked);
        }
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
}