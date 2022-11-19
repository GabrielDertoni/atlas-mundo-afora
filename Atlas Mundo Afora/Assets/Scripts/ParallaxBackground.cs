using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] Vector2 m_ParallaxEffectMultiplier = new Vector2(.5f, .5f);

    private Transform m_CameraTransform;
    private Vector3 m_PrevCameraPosition;

    // Start is called before the first frame update
    void Start()
    {
        m_CameraTransform = Camera.main.transform;
        m_PrevCameraPosition = m_CameraTransform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 delta = m_CameraTransform.position - m_PrevCameraPosition;
        // Only apply the parallax effect on the x axis.
        transform.position += new Vector3(
            delta.x * m_ParallaxEffectMultiplier.x,
            delta.y * m_ParallaxEffectMultiplier.y,
            delta.z
        );
        m_PrevCameraPosition = m_CameraTransform.position;
    }
}
