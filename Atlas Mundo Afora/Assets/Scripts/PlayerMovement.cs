using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 10f;
    [SerializeField] private float m_Torque = 2f;
    [SerializeField] private float m_MaxAngularVelocity = 200f;
    [SerializeField] private float m_ComebackTorque = 1f;
    [SerializeField] private float m_AngularDrag = 2f;
    [SerializeField] private Collider2D m_BackWheelColider;
    [SerializeField] private Collider2D m_FrontWheelColider;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody2D>().angularDrag = m_AngularDrag;
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (Input.GetButtonDown("Jump") && IsGroundedWithBackWheel())
        {
            Jump();
        }
        else if (Input.GetButton("Jump"))
        {
            // Makes the bike lean backwards with player input
            ApplyTorque(m_Torque);
        }
        else if (!IsGroundedWithFrontWheel())
        {
            // If it is not grounded with both wheels, add a sligth torque forwards
            ApplyTorque(-m_ComebackTorque);
        }
    }

    private void FixedUpdate()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Debug.Log(rb.angularVelocity);
        if (Mathf.Abs(rb.angularVelocity) > m_MaxAngularVelocity)
            rb.angularVelocity = Mathf.Sign(rb.angularVelocity) * m_MaxAngularVelocity;
    }

    private void Jump()
    {
        GetComponent<Rigidbody2D>().AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
    }

    private bool IsGroundedWithBackWheel()
    {
        LayerMask mask = LayerMask.GetMask("Ground");
        // TODO: Can only jump when back wheel is touching the ground. Should we require both wheels to be touching? Only one of them?
        return m_BackWheelColider.IsTouchingLayers(mask);
    }

    private bool IsGroundedWithFrontWheel()
    {
        LayerMask mask = LayerMask.GetMask("Ground");
        // TODO: Can only jump when back wheel is touching the ground. Should we require both wheels to be touching? Only one of them?
        return m_FrontWheelColider.IsTouchingLayers(mask);
    }

    private void ApplyTorque(float torque)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (Mathf.Abs(rb.angularVelocity) < m_MaxAngularVelocity)
            rb.AddTorque(torque);
    }
}
