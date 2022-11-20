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
    private float m_IsTouchingGroundThreashold = .3f;

    // Current state of the system.
    private State m_State = State.Airborne;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody2D>().angularDrag = m_AngularDrag;
        m_PrevRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCachedVars();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (Input.GetButtonDown("Jump") && m_IsGroundedWithBackWheel)
        {
            Jump();
        }
        else if (Input.GetButton("Jump"))
        {
            // Makes the bike lean backwards with player input
            ApplyTorque(m_Torque * Time.deltaTime);
        }
        else if (!m_IsGroundedWithFrontWheel && !m_HasLanded)
        {
            // If it is not grounded with both wheels, just stop rotating
            rb.angularVelocity = 0f;
        } else if (m_IsAirborne)
        {
            // Try to align with the ground.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down);
            if (hit)
            {
                float ang = Vector2.Dot(hit.normal, transform.up);
                Debug.Log(ang);
            }
        }

        DetectFlips();
    }

    private bool m_IsGroundedWithFrontWheel
    {
        get { return (m_State & State.GroundedWithFrontWheel) != 0; }
    }

    private bool m_IsGroundedWithBackWheel
    {
        get { return (m_State & State.GroundedWithBackWheel) != 0; }
    }

    private bool m_IsAirborne
    {
        get { return m_State == State.Airborne; }
    }

    private bool m_HasLanded
    {
        get { return m_PrevState == State.Airborne && m_State != State.Airborne; }
    }

    private bool m_Jumped
    {
        get { return m_PrevState != State.Airborne && m_State == State.Airborne; }
    }


    // Updates some variables to use inside the Update function. Should always be called in the beginning of the Update funciton.
    private void UpdateCachedVars()
    {
        m_PrevState = m_State;
        m_State = GetState();

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

        // 270deg already counts as a flip.
        if (Mathf.Abs(m_CumulativeRotation) > 270f)
            m_CountFlipsWhileInAir++;

        // We did a flip! Assum a single flip was made
        if (Mathf.Abs(m_CumulativeRotation) > 360f)
            m_CumulativeRotation %= 360f;

        if (m_HasLanded && m_CountFlipsWhileInAir > 0)
        {
            // TODO: Maybe we should give extra impulse if the player did multiple flips.
            Debug.Log("Boost");
            rb.AddForce(Vector2.right * m_BoostImpulse, ForceMode2D.Impulse);
            m_CountFlipsWhileInAir = 0;
        }

        m_CumulativeRotation += delta;
        m_PrevRotation = transform.rotation;
    }

    private void Jump()
    {
        GetComponent<Rigidbody2D>().AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
    }

    private bool IsGroundedWithBackWheel()
    {
        LayerMask mask = LayerMask.GetMask("Ground");
        RaycastHit2D hit = Physics2D.Raycast(m_BackWheelColider.gameObject.transform.position, Vector2.down, Mathf.Infinity, mask.value);
        if (!hit.collider) return false;
        ColliderDistance2D dist = Physics2D.Distance(m_BackWheelColider, hit.collider);
        // TODO: Can only jump when back wheel is touching the ground. Should we require both wheels to be touching? Only one of them?
        return dist.distance < m_IsTouchingGroundThreashold;
    }

    private bool IsGroundedWithFrontWheel()
    {
        LayerMask mask = LayerMask.GetMask("Ground");
        RaycastHit2D hit = Physics2D.Raycast(m_BackWheelColider.gameObject.transform.position, Vector2.down, Mathf.Infinity, mask.value);
        if (!hit.collider) return false;
        ColliderDistance2D dist = Physics2D.Distance(m_FrontWheelColider, hit.collider);
        return dist.distance < m_IsTouchingGroundThreashold;
    }

    // This function will directly manipulate the rigidbody's angular velocity. Use it only when the player is airborne
    private void ApplyTorque(float torque)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = Mathf.Clamp(rb.angularVelocity + torque / rb.inertia, -m_MaxAngularVelocity, m_MaxAngularVelocity);
    }
}
