using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Stamp : MonoBehaviour, ICollectible
{
    public static event Action OnStampCollected;
    public void Collect()
    {
        Debug.Log("Stamp collected");
        Destroy(gameObject);
        OnStampCollected?.Invoke();
    }
}