using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] public Transform target;
    [SerializeField] public float cameraSpeed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Transform selfTrasnform = GetComponent<Transform>();
        Vector2 delta = target.position - selfTrasnform.position;
        float speed = Mathf.Min(delta.magnitude, cameraSpeed);
        delta.Normalize();
        delta *= speed;
        selfTrasnform.position += new Vector3(delta.x, delta.y, 0f);
    }
}
