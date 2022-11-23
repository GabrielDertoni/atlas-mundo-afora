using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    // The distance the player has travelled in this playthrough.
    private float m_Distance = 0;
    // Number of collected stamps.
    private int m_StampsCollected = 0;
    // Number of flips
    private int m_Flips = 0;
    private bool[] m_Stamps = new bool[] { false, false, false };

    [SerializeField] private float m_DistanceMultiplier = 1f;
    [SerializeField] private float m_StampsMultiplier = 500f;
    [SerializeField] private float m_FlipsMultiplier = 100f;

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

    public Vector3 GetSpawnPoint()
    {
        return m_SpawnPoint.position;
    }

    public int GetPoints()
    {
        return Mathf.FloorToInt(m_Distance * m_DistanceMultiplier
                              + (float)m_StampsCollected * m_StampsMultiplier
                              + (float)m_Flips * m_FlipsMultiplier);
    }

    public void IncrementDistance(float increment)
    {
        m_Distance += increment;
    }

    public void IncrementFlips(int increment)
    {
        m_Flips += increment;
    }

    public void CollectStamp(int which)
    {
        // If we haven't collected that one already, count it.
        if (!m_Stamps[which])
            m_StampsCollected++;
        m_Stamps[which] = true;
    }

    public bool HasCollectedStamp(int which)
    {
        return m_Stamps[which];
    }

    public int GetStampCount()
    {
        return m_StampsCollected;
    }
}
