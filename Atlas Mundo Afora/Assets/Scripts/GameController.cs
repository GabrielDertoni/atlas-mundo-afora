using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController s_Instance;

    [SerializeField] private Transform m_SpawnPoint;
    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private GameObject m_PlayerInstance;

    public static GameController GetInstance() { return s_Instance; }

    private void Awake()
    {
        s_Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameOver()
    {
        m_PlayerInstance.transform.position = m_SpawnPoint.position;
        m_PlayerInstance.transform.rotation = m_SpawnPoint.rotation;
        Rigidbody2D rb = m_PlayerInstance.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        Debug.Log("Game Over");
    }
}
