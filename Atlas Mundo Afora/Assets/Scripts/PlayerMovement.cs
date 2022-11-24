using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    enum State
    {
        Airborne               = 0b00,
        GroundedWithFrontWheel = 0b01,
        GroundedWithBackWheel  = 0b10,
        GroundedWithBothWheels = 0b11,
    }

    [SerializeField] private float m_JumpForce = 10f;
    [SerializeField] private float m_Torque = 2f;
    [SerializeField] private float m_MaxAngularVelocity = 200f;
    [SerializeField] private float m_ComebackTorque = 1f;
    [SerializeField] private float m_AngularDrag = 2f;
    [SerializeField] private float m_BoostImpulse = 5f;
    [SerializeField] private Collider2D m_BackWheelColider;
    [SerializeField] private Collider2D m_FrontWheelColider;
    [SerializeField] private ParticleSystem m_SpeedLines;
    [SerializeField] private Animator m_AnimatorChar;
    [SerializeField] private Animator m_AnimatorPedal;
    [SerializeField] private AudioSource m_BikeSoundEffect;
    [SerializeField] private AudioSource m_HowlingSoundEffect;

    private Quaternion m_PrevRotation;
    private Vector3 m_PrevPosition;
    private float m_CumulativeRotation = 0f;
    private int m_CountFlipsWhileInAir = 0;
    private float m_IsTouchingGroundThreashold = .3f;
    // This variable indicates if a flip has been counted but the m_CumulativeRotation variable has not been reset yet.
    // This is necessary since we want to count 270deg as a flip, but the next flip only occurs at 360+270 deg.
    private bool m_HasCountedFlip = false;

    private State m_PrevState = State.Airborne;
    // Current state of the system.
    private State m_State = State.Airborne;

    // Variables to use in PID controller
    [SerializeField] private float m_PID_ProportionalGain = 1f;
    [SerializeField] private float m_PID_DerivativeGain = .25f;
    [SerializeField] private float m_PID_IntegralGain = 5e-6f;
    private float m_LastError = 0f;
    private float m_AngleIntegral = 0f;
    private bool m_HasLastValue = false;

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
        float velocity = GetComponent<Rigidbody2D>().velocity.magnitude;

        m_AnimatorChar.SetBool("Collision", m_IsGroundedWithBackWheel);
        m_AnimatorChar.SetFloat("Speed", velocity);
        
        m_AnimatorPedal.SetBool("Collision", m_IsGroundedWithBackWheel);
        m_AnimatorPedal.SetFloat("Speed", velocity);
        
        if(m_AnimatorChar.GetBool("Collision") == false) {
            m_BikeSoundEffect.Play();
        }
        
        if(velocity < 10) {
            m_HowlingSoundEffect.Play();
        }

        if (Input.GetButtonDown("Jump") && m_IsGroundedWithBackWheel)
            Jump();
        else if (Input.GetButton("Jump"))
            // Makes the bike lean backwards with player input
            ApplyTorque(m_Torque * Time.deltaTime);

        if (m_Jumped && m_SpeedLines.IsAlive())
            m_SpeedLines.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        DetectFlips();
        float distance = (transform.position - m_PrevPosition).magnitude;
        GameController.GetInstance().IncrementDistance(distance);
        m_PrevPosition = transform.position;

        // We also need to check if the player is outside the spawn point. Otherwise, it will just loop on game over.
        GameController gameController = GameController.GetInstance();
        Vector3 spawnPoint = gameController.GetSpawnPoint();
        if ((transform.position - spawnPoint).magnitude > 5f && GetComponent<Rigidbody2D>().velocity.x <= 0f)
        {
            // The player stopped moving forward.
            gameController.GameOver();
        }
    }

    private void FixedUpdate()
    {
        UpdateCachedVars();

        if (m_IsAirborne && !Input.GetButton("Jump"))
        {
            LayerMask groundLayer = LayerMask.GetMask("Ground");
            // Try to align with the ground.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, groundLayer.value);
            if (hit)
                AlignWithGroundPID(hit.normal);
            else
                GetComponent<Rigidbody2D>().angularVelocity = 0f;
        }
        else
        {
            // Reset the PID controller.
            m_HasLastValue = false;
        }
    }

    private void AlignWithGroundPID(Vector2 groundNormal)
    {
        // The angle between ourselves and the desired angle.
        float ang = Vector2.SignedAngle(transform.up, groundNormal);
        float error = ang / 180f;
        float P = error * m_PID_ProportionalGain;

        m_AngleIntegral += error / Time.deltaTime;
        float I = m_AngleIntegral * m_PID_IntegralGain;

        float angDeriv = (error - m_LastError) / Time.deltaTime;
        float D = 0f;
        if (m_HasLastValue)  D = angDeriv * m_PID_DerivativeGain;
        m_LastError = error;

        float value = P + I + D;
        float torque = m_ComebackTorque * value;
        ApplyTorque(torque * Time.deltaTime);

        m_HasLastValue = true;
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

        float delta = Vector2.SignedAngle(m_PrevRotation * Vector2.up, transform.up);

        // 270deg already counts as a flip.
        if (Mathf.Abs(m_CumulativeRotation) > 270f && !m_HasCountedFlip)
        {
            m_CountFlipsWhileInAir++;
            m_HasCountedFlip = true;
        }

        // We did a flip! Assum a single flip was made
        if (Mathf.Abs(m_CumulativeRotation) > 360f)
        {
            m_CumulativeRotation %= 360f;
            m_HasCountedFlip = false;
        }

        if (m_HasLanded && m_CountFlipsWhileInAir > 0)
        {
            rb.AddForce(Vector2.right * m_BoostImpulse, ForceMode2D.Impulse);
            m_SpeedLines.Play();
            GameController.GetInstance().IncrementFlips(m_CountFlipsWhileInAir);
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