using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleUI : MonoBehaviour
{
    public void FillStamp()
    {
        gameObject.GetComponent<Renderer> ().material.color = Color.yellow;
    }
}
