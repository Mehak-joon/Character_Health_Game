using UnityEngine;

/// <summary>
/// Dual-Perspective Orbit & First-Person Camera
/// - Press 'V' or 'C' to toggle between Third-Person and First-Person views
/// - In First-Person: Camera sits directly at the striker's eye level; looking down reveals your feet & ball!
/// - In Third-Person: Over-the-shoulder stadium view with full Arrow Key orbit controls
/// - Rotate camera using Arrow Keys (Left/Right for yaw, Up/Down for pitch)
/// </summary>
public class SmoothCameraFollow : MonoBehaviour
{
    public enum CameraMode
    {
        ThirdPerson,
        FirstPerson
    }

    [Header("Camera Mode")]
    [SerializeField] private CameraMode currentMode = CameraMode.ThirdPerson;
    [SerializeField] private float firstPersonHeight = 1.68f;
    [SerializeField] private float firstPersonForwardOffset = 0.22f;

    [Header("Third-Person Framing")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 8.0f;
    [SerializeField] private float heightOffset = 1.8f;
    [SerializeField] private float smoothSpeed = 0.035f;

    [Header("Arrow Key Orbit Controls")]
    [SerializeField] private float arrowTurnSpeed = 125f;
    [SerializeField] private float arrowTiltSpeed = 75f;

    [Header("Mouse Orbit Controls (Optional)")]
    [SerializeField] private float mouseSensitivityX = 2.5f;
    [SerializeField] private float mouseSensitivityY = 1.8f;
    [SerializeField] private float minPitch = -45f; // allow looking down in first person
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private bool requireRightClickForMouse = true;

    [Header("Auto-Align Behind Player")]
    [SerializeField] private bool autoAlignBehindPlayer = false;
    [SerializeField] private float autoAlignSpeed = 4.0f;

    public CameraMode CurrentMode => currentMode;

    private float currentYaw = 0f;
    private float currentPitch = 24f;
    private Vector3 currentVelocity = Vector3.zero;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            currentYaw = target.eulerAngles.y;
        }
    }

    private void Start()
    {
        if (target != null)
        {
            currentYaw = target.eulerAngles.y;
        }
    }

    private void Update()
    {
        CheckModeToggle();
        HandleCameraInput();
    }

    private void CheckModeToggle()
    {
        bool toggle = false;

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var k = UnityEngine.InputSystem.Keyboard.current;
            if (k.vKey.wasPressedThisFrame || k.cKey.wasPressedThisFrame) toggle = true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.C)) toggle = true;
#endif

        if (toggle)
        {
            ToggleCameraMode();
        }
    }

    public void ToggleCameraMode()
    {
        currentMode = (currentMode == CameraMode.ThirdPerson) ? CameraMode.FirstPerson : CameraMode.ThirdPerson;

        if (currentMode == CameraMode.FirstPerson)
        {
            currentPitch = 12f;
            if (target != null) currentYaw = target.eulerAngles.y;
            Debug.Log("<color=cyan>[Camera] Switched to FIRST-PERSON VIEW (Press 'V' to toggle)</color>");
        }
        else
        {
            currentPitch = 24f;
            Debug.Log("<color=cyan>[Camera] Switched to THIRD-PERSON VIEW (Press 'V' to toggle)</color>");
        }
    }

    private void HandleCameraInput()
    {
        float turnInput = 0f;
        float tiltInput = 0f;

        // 1. Arrow Key Input
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var k = UnityEngine.InputSystem.Keyboard.current;
            if (k.leftArrowKey.isPressed) turnInput -= 1f;
            if (k.rightArrowKey.isPressed) turnInput += 1f;
            if (k.upArrowKey.isPressed) tiltInput -= 1f;
            if (k.downArrowKey.isPressed) tiltInput += 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.LeftArrow)) turnInput -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) turnInput += 1f;
        if (Input.GetKey(KeyCode.UpArrow)) tiltInput -= 1f;
        if (Input.GetKey(KeyCode.DownArrow)) tiltInput += 1f;
#endif

        if (Mathf.Abs(turnInput) > 0.01f)
        {
            currentYaw += turnInput * arrowTurnSpeed * Time.deltaTime;
        }
        if (Mathf.Abs(tiltInput) > 0.01f)
        {
            currentPitch += tiltInput * arrowTiltSpeed * Time.deltaTime;
        }

        // 2. Mouse Look
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            bool rmbHeld = UnityEngine.InputSystem.Mouse.current.rightButton.isPressed;
            bool canMouseLook = (currentMode == CameraMode.FirstPerson) || !requireRightClickForMouse || rmbHeld;

            if (canMouseLook)
            {
                Vector2 delta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
                if (delta.sqrMagnitude > 0.05f)
                {
                    currentYaw += delta.x * mouseSensitivityX * 0.1f;
                    currentPitch -= delta.y * mouseSensitivityY * 0.1f;
                }
            }

            if (currentMode == CameraMode.ThirdPerson)
            {
                float scroll = UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    distance = Mathf.Clamp(distance - Mathf.Sign(scroll) * 0.8f, 3.5f, 15f);
                }
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        bool canLegacyMouseLook = (currentMode == CameraMode.FirstPerson) || !requireRightClickForMouse || Input.GetMouseButton(1);
        if (canLegacyMouseLook)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
            {
                currentYaw += mouseX * mouseSensitivityX;
                currentPitch -= mouseY * mouseSensitivityY;
            }
        }

        if (currentMode == CameraMode.ThirdPerson)
        {
            float legacyScroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(legacyScroll) > 0.01f)
            {
                distance = Mathf.Clamp(distance - legacyScroll * 4f, 3.5f, 15f);
            }
        }
#endif

        float effectiveMinPitch = (currentMode == CameraMode.FirstPerson) ? -65f : 6f;
        currentPitch = Mathf.Clamp(currentPitch, effectiveMinPitch, maxPitch);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (autoAlignBehindPlayer && currentMode == CameraMode.ThirdPerson)
        {
            float targetYaw = target.eulerAngles.y;
            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, autoAlignSpeed * Time.deltaTime);
        }

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition;

        if (currentMode == CameraMode.FirstPerson)
        {
            // Position directly at the player's eye height
            Vector3 eyePos = target.position + Vector3.up * firstPersonHeight + (rotation * Vector3.forward * firstPersonForwardOffset);
            desiredPosition = eyePos;
        }
        else
        {
            // Position behind in third-person view
            Vector3 targetFocusPos = target.position + Vector3.up * heightOffset;
            desiredPosition = targetFocusPos - (rotation * Vector3.forward * distance);
        }

        float trackingSmoothTime = (currentMode == CameraMode.FirstPerson) ? 0.015f : smoothSpeed;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, trackingSmoothTime);
        transform.rotation = rotation;
    }
}
