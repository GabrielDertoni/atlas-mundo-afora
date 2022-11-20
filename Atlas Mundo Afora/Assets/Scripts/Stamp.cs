using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Stamp : MonoBehaviour, ICollectible
{
    [SerializeField] private CollectibleUI StampUI;
    
    public void Collect()
    {
        Destroy(gameObject);
        StampUI.FillStamp();
    }
}