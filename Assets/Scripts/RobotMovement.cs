using UnityEngine;

/// <summary>
/// Robot Movement Controller
/// Allows moving the robot around the pitch using WASD or Arrow Keys.
/// Movement speed and football kicking force dynamically scale with Character Health state!
/// </summary>
public class RobotMovement : MonoBehaviour
{
    [Header("Controller Reference")]
    [SerializeField] private CharacterHealthController healthController;
    [SerializeField] private SoundEffectGenerator soundFX;
    [SerializeField] private CharacterAnimator characterAnimator;

    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 6.8f;
    [SerializeField] private float sprintSpeedMultiplier = 1.6f;
    [SerializeField] private float slideBoostSpeed = 12.0f;
    [SerializeField] private float rotationSpeed = 26f;
    [SerializeField] private float baseKickForce = 12f;

    [Header("Components")]
    [SerializeField] private Rigidbody rb;

    private Vector3 movementInput;
    private bool isSprinting = false;
    private bool isSliding = false;
    private float slideTimer = 0f;
    private Vector3 slideDirection = Vector3.forward;

    private void Awake()
    {
        if (healthController == null) healthController = GetComponent<CharacterHealthController>();
        if (soundFX == null) soundFX = GetComponentInChildren<SoundEffectGenerator>();
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<CharacterAnimator>();
        if (rb == null) rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void Update()
    {
        // If defeated, cannot move
        if (healthController != null && healthController.CurrentState == CharacterHealthController.HealthState.Defeated)
        {
            movementInput = Vector3.zero;
            isSprinting = false;
            isSliding = false;
            if (characterAnimator != null)
            {
                characterAnimator.isMoving = false;
                characterAnimator.isSprinting = false;
                characterAnimator.isSliding = false;
            }
            return;
        }

        float h = 0f;
        float v = 0f;
        bool shiftHeld = false;
        bool shiftPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var k = UnityEngine.InputSystem.Keyboard.current;
            if (k.wKey.isPressed) v += 1f;
            if (k.sKey.isPressed) v -= 1f;
            if (k.aKey.isPressed) h -= 1f;
            if (k.dKey.isPressed) h += 1f;

            shiftHeld = k.leftShiftKey.isPressed || k.rightShiftKey.isPressed;
            shiftPressed = k.leftShiftKey.wasPressedThisFrame || k.rightShiftKey.wasPressedThisFrame;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) shiftHeld = true;
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) shiftPressed = true;
#endif

        // Camera-relative input direction
        Camera cam = Camera.main;
        Vector3 camForward = cam != null ? cam.transform.forward : Vector3.forward;
        Vector3 camRight = cam != null ? cam.transform.right : Vector3.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        movementInput = (camForward * v + camRight * h).normalized;

        // Slide tackle input
        if (!isSliding && shiftPressed && movementInput.sqrMagnitude > 0.1f)
        {
            isSliding = true;
            slideTimer = 0.55f;
            slideDirection = movementInput;
            if (soundFX != null) soundFX.PlaySlide();
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                isSliding = false;
            }
        }

        isSprinting = !isSliding && shiftHeld && (movementInput.sqrMagnitude > 0.05f);

        if (characterAnimator != null)
        {
            characterAnimator.isMoving = movementInput.sqrMagnitude > 0.05f;
            characterAnimator.isSprinting = isSprinting;
            characterAnimator.isSliding = isSliding;
        }

        // Smooth rotation towards movement or slide direction
        Vector3 faceDirection = isSliding ? slideDirection : movementInput;
        if (faceDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(faceDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        float speedMultiplier = GetHealthSpeedMultiplier();
        Vector3 targetVelocity;

        if (isSliding)
        {
            targetVelocity = slideDirection * slideBoostSpeed * speedMultiplier;
        }
        else
        {
            float speed = baseMoveSpeed * speedMultiplier * (isSprinting ? sprintSpeedMultiplier : 1f);
            targetVelocity = movementInput * speed;
        }

        if (rb != null)
        {
            Vector3 vel = rb.linearVelocity;
            vel.x = targetVelocity.x;
            vel.z = targetVelocity.z;
            rb.linearVelocity = vel;
        }
        else
        {
            transform.position += targetVelocity * Time.fixedDeltaTime;
        }
    }

    private float GetHealthSpeedMultiplier()
    {
        if (healthController == null) return 1f;

        switch (healthController.CurrentState)
        {
            case CharacterHealthController.HealthState.Healthy: return 1.0f;
            case CharacterHealthController.HealthState.Injured: return 0.65f;
            case CharacterHealthController.HealthState.Critical: return 0.32f;
            case CharacterHealthController.HealthState.Defeated: return 0f;
            default: return 1f;
        }
    }

    private float GetHealthKickMultiplier()
    {
        if (healthController == null) return 1f;

        switch (healthController.CurrentState)
        {
            case CharacterHealthController.HealthState.Healthy: return 1.0f;
            case CharacterHealthController.HealthState.Injured: return 0.65f;
            case CharacterHealthController.HealthState.Critical: return 0.35f;
            case CharacterHealthController.HealthState.Defeated: return 0f;
            default: return 1f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleBallKick(collision.collider, collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position);
    }

    private void OnCollisionStay(Collision collision)
    {
        // If continuing to push the ball
        if (movementInput.sqrMagnitude > 0.1f)
        {
            HandleBallKick(collision.collider, collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position);
        }
    }

    private void HandleBallKick(Collider col, Vector3 contactPoint)
    {
        if (healthController != null && healthController.CurrentState == CharacterHealthController.HealthState.Defeated)
            return;

        if (col.gameObject.name.Contains("Football"))
        {
            Rigidbody ballRb = col.attachedRigidbody;
            if (ballRb != null)
            {
                Vector3 kickDir = (col.transform.position - transform.position);
                kickDir.y = isSliding ? 0.48f : 0.32f; // slide gives higher chip lift
                kickDir.Normalize();

                float powerMultiplier = isSliding ? 1.75f : (isSprinting ? 1.35f : 1.0f);
                float force = baseKickForce * GetHealthKickMultiplier() * powerMultiplier;
                ballRb.AddForce(kickDir * force, ForceMode.Impulse);

                if (characterAnimator != null && !isSliding)
                {
                    characterAnimator.TriggerKick();
                }

                if (soundFX != null)
                {
                    soundFX.PlayKick();
                }
            }
        }
    }
}

