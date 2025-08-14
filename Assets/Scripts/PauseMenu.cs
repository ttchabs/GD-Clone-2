using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public PlayerInputActions playerController;
    private bool isPaused = false;

    void Awake()
    {
        playerController = new PlayerInputActions();
        playerController.Gameplay.Pause.performed += ctx => TogglePause();
        pauseMenuUI.SetActive(false);
    }

    void OnEnable()
    {
        playerController.Gameplay.Enable();
    }

    void OnDisable()
    {
        playerController.Gameplay.Disable();
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
