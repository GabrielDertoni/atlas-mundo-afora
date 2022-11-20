using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class EndRoutine : MonoBehaviour
{
    GameController controller;

    void OnTriggerEnter2D(Collider2D collision) {
        controller = GameObject.Find("GameController").GetComponent<GameController>();

        if(collision.tag == "Player") {
            controller.GameOver();
        }
    }
}
