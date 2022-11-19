using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    enum State
    {
        Airborne = 0b00,
        GroundedWithFrontWheel = 0b01,
        GroundedWithBackWheel = 0b10,
        GroundedWithBothWheels = 0b11,
    }

    [SerializeField] private float m_JumpForce = 10f;
    [SerializeField] private float m_Torque = 2f;
    [SerializeField] private float m_MaxAngularVelocity = 200f;
    // [SerializeField] private float m_ComebackTorque = 1f;
    [SerializeField] private float m_AngularDrag = 2f;
    [SerializeField] private float m_BoostImpulse = 5f;
    [SerializeField] private Collider2D m_BackWheelColider;
    [SerializeField] private Collider2D m_FrontWheelColider;

    private Quaternion m_PrevRotation;
    private float m_CumulativeRotation = 0f;
    private int m_CountFlipsWhileInAir = 0;
    private State m_PrevState = State.Airborne;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody2D>().angularDrag = m_AngularDrag;
        m_PrevRotation = transform.rotation;
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
            ApplyTorque(m_Torque * Time.deltaTime);
        }
        else if (!IsGroundedWithFrontWheel())
        {
            // If it is not grounded with both wheels, just stop rotating
            rb.angularVelocity = 0f;
        }

        DetectFlips();
    }

    private State GetState()
    {
        int mask = 0;
        if (IsGroundedWithFrontWheel()) mask |= 0b01;
        if (IsGroundedWithBackWheel())  mask |= 0b10;
        return (State)mask;
    }

    private void DetectFlips()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        float delta = Quaternion.Angle(m_PrevRotation, transform.rotation);
        if (Mathf.Abs(m_CumulativeRotation) > 360f)
        {
            // We did a flip! Assum a single flip was made
            m_CumulativeRotation %= 360f;
            m_CountFlipsWhileInAir++;
        }

        State state = GetState();
        // If the player is not airborne anymore, but it was in the previous frame and there was a flip.
        if (state != 0 && m_PrevState == 0 && m_CountFlipsWhileInAir > 0)
        {
            rb.AddForce(Vector2.right * m_BoostImpulse, ForceMode2D.Impulse);
            m_CountFlipsWhileInAir = 0;
        }

        m_CumulativeRotation += delta;
        m_PrevRotation = transform.rotation;
        m_PrevState = state;
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

    // This function will directly manipulate the rigidbody's angular velocity. Use it only when the player is airborne
    private void ApplyTorque(float torque)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = Mathf.Clamp(rb.angularVelocity + torque / rb.inertia, -m_MaxAngularVelocity, m_MaxAngularVelocity);
    }
}
