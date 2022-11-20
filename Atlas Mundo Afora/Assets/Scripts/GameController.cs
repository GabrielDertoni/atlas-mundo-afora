using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public bool GameIsPaused = false;

    private static GameController s_Instance;

    [SerializeField] private Transform m_SpawnPoint;
    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private GameObject m_PlayerInstance;
    [SerializeField] private GameObject m_PauseMenu;
    [SerializeField] private GameObject m_EndLevelMenu;

    public static GameController GetInstance() { return s_Instance; }

    private void Awake()
    {
        s_Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: This is just for testing. In the actual game there will be a HUD button that does this.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void GameOver()
    {
        ReloadLevel();
    }

    public void PauseGame()
    {
        m_PauseMenu.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void ResumeGame()
    {
        m_PauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void WinLevel()
    {
        Time.timeScale = 0f;
        GameIsPaused = true;
        m_EndLevelMenu.SetActive(true);
    }

    public void ReloadLevel()
    {
        if (GameIsPaused) ResumeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        if (GameIsPaused) ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadNextLevel()
    {
        if (GameIsPaused) ResumeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
