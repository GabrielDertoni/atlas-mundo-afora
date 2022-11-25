using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Stamp : MonoBehaviour, ICollectible
{
    public static event Action OnCollectible;

    public void Collect()
    {
        Destroy(gameObject);
        OnCollectible?.Invoke();
    }
}