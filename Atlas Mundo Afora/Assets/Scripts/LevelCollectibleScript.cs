using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelCollectibleScript : MonoBehaviour
{
    public TextMeshProUGUI levelCollectible;

    void Start()
    {
        int leve1Collectible = PlayerPrefs.GetInt("Level1Collectible");
        levelCollectible.text = leve1Collectible + "/3";
    }
}
