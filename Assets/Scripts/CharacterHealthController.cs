using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Character Health State Machine Controller
/// Topic 2: Character Health
/// States: Healthy, Injured, Critical, Defeated
/// Controls:
///   - Press 'H' to reduce health by one level
///   - Press 'R' to reset the system back to Healthy
/// </summary>
public class CharacterHealthController : MonoBehaviour
{
    public enum HealthState
    {
        Healthy,
        Injured,
        Critical,
        Defeated
    }

    [Header("Health Configuration")]
    [Tooltip("Maximum health level (default is 3 corresponding to Healthy, Injured, Critical, Defeated)")]
    [SerializeField] private int maxHealthLevel = 3;
    [SerializeField] private int currentHealthLevel = 3;

    [Header("Visual References")]
    [SerializeField] private Renderer characterRenderer;
    [SerializeField] private Transform characterBody;

    [Header("Modular System References")]
    [SerializeField] private SoundEffectGenerator soundFX;
    [SerializeField] private CharacterParticleEffects particleFX;
    [SerializeField] private CharacterAnimator characterAnimator;

    [Header("State Colors")]
    [SerializeField] private Color healthyColor = new Color(0.2f, 0.85f, 0.3f, 1f);   // Bright Green
    [SerializeField] private Color injuredColor = new Color(1.0f, 0.75f, 0.1f, 1f);   // Yellow / Orange
    [SerializeField] private Color criticalColor = new Color(0.95f, 0.2f, 0.2f, 1f);  // Bright Red
    [SerializeField] private Color defeatedColor = new Color(0.35f, 0.35f, 0.4f, 1f); // Dark Charcoal Grey

    // Events for UI or external listeners
    public event Action<HealthState, int, int> OnHealthChanged;

    private Material runtimeMaterial;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Coroutine shakeCoroutine;
    private Coroutine pulseCoroutine;

    public HealthState CurrentState
    {
        get
        {
            switch (currentHealthLevel)
            {
                case 3: return HealthState.Healthy;
                case 2: return HealthState.Injured;
                case 1: return HealthState.Critical;
                default: return HealthState.Defeated;
            }
        }
    }

    public int CurrentHealthLevel => currentHealthLevel;
    public int MaxHealthLevel => maxHealthLevel;

    private void Awake()
    {
        if (characterRenderer == null)
        {
            characterRenderer = GetComponentInChildren<Renderer>();
        }

        if (characterBody == null)
        {
            characterBody = transform;
        }

        initialPosition = characterBody.localPosition;
        initialRotation = characterBody.localRotation;

        if (characterRenderer != null)
        {
            runtimeMaterial = characterRenderer.material;
        }

        if (soundFX == null) soundFX = GetComponentInChildren<SoundEffectGenerator>();
        if (particleFX == null) particleFX = GetComponentInChildren<CharacterParticleEffects>();
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<CharacterAnimator>();

        // Initialize to full health
        currentHealthLevel = maxHealthLevel;
    }

    private void Start()
    {
        ApplyStateVisuals(CurrentState, instant: true);
        if (characterAnimator != null) characterAnimator.SetState(CurrentState);
        NotifyHealthChanged();
    }

    private void Update()
    {
        bool hPressed = false;
        bool rPressed = false;

        // Check New Input System
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.hKey.wasPressedThisFrame)
            {
                hPressed = true;
            }
            if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
            {
                rPressed = true;
            }
        }
#endif

        // Check Legacy Input System fallback
#if ENABLE_LEGACY_INPUT_MANAGER
        if (UnityEngine.Input.GetKeyDown(KeyCode.H))
        {
            hPressed = true;
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            rPressed = true;
        }
#endif

        if (hPressed)
        {
            TakeDamage();
        }

        if (rPressed)
        {
            ResetHealth();
        }
    }

    /// <summary>
    /// Reduces character health by one level (Healthy -> Injured -> Critical -> Defeated).
    /// Bound to key 'H'.
    /// </summary>
    public void TakeDamage()
    {
        if (currentHealthLevel <= 0)
        {
            Debug.Log("[CharacterHealth] Character is already DEFEATED. Press 'R' to reset.");
            return;
        }

        currentHealthLevel--;
        HealthState newState = CurrentState;
        Debug.Log($"[CharacterHealth] 'H' pressed. Health Level: {currentHealthLevel}/{maxHealthLevel} | State: {newState}");

        // Trigger SFX, VFX, and Animation
        if (characterAnimator != null)
        {
            characterAnimator.TriggerRecoil();
            characterAnimator.SetState(newState);
        }

        if (particleFX != null)
        {
            particleFX.PlayHitSparks();
            particleFX.SetCriticalSmoke(newState == HealthState.Critical);
            if (newState == HealthState.Defeated)
            {
                particleFX.PlayDefeatDust();
            }
        }

        if (soundFX != null)
        {
            if (newState == HealthState.Defeated)
            {
                soundFX.PlayDefeated();
            }
            else if (newState == HealthState.Critical)
            {
                soundFX.PlayCriticalAlarm();
            }
            else
            {
                soundFX.PlayHit();
            }
        }

        ApplyStateVisuals(newState, instant: false);
        NotifyHealthChanged();
    }

    /// <summary>
    /// Resets character health back to full (Healthy state).
    /// Bound to key 'R'.
    /// </summary>
    public void ResetHealth()
    {
        currentHealthLevel = maxHealthLevel;
        HealthState newState = CurrentState;
        Debug.Log($"[CharacterHealth] 'R' pressed. System RESET to {newState} ({currentHealthLevel}/{maxHealthLevel})");

        // Trigger SFX, VFX, and Animation
        if (characterAnimator != null)
        {
            characterAnimator.SetState(newState);
            characterAnimator.TriggerReviveJump();
        }

        if (particleFX != null)
        {
            particleFX.SetCriticalSmoke(false);
            particleFX.PlayReviveAura();
        }

        if (soundFX != null)
        {
            soundFX.PlayRevive();
        }

        // Restore transforms
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);

        characterBody.localPosition = initialPosition;
        characterBody.localRotation = initialRotation;

        ApplyStateVisuals(newState, instant: false);
        NotifyHealthChanged();
    }

    private void ApplyStateVisuals(HealthState state, bool instant)
    {
        Color targetColor = GetStateColor(state);

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        if (runtimeMaterial != null)
        {
            if (runtimeMaterial.HasProperty("_BaseColor"))
            {
                runtimeMaterial.SetColor("_BaseColor", targetColor);
            }
            else if (runtimeMaterial.HasProperty("_Color"))
            {
                runtimeMaterial.SetColor("_Color", targetColor);
            }
        }

        // Animation/Pose based on state
        switch (state)
        {
            case HealthState.Healthy:
                characterBody.localPosition = initialPosition;
                characterBody.localRotation = initialRotation;
                break;

            case HealthState.Injured:
                characterBody.localPosition = initialPosition;
                characterBody.localRotation = initialRotation * Quaternion.Euler(0f, 0f, 6f); // slight injured tilt
                TriggerDamageShake();
                break;

            case HealthState.Critical:
                characterBody.localPosition = initialPosition + new Vector3(0f, -0.1f, 0f);
                characterBody.localRotation = initialRotation * Quaternion.Euler(0f, 0f, -12f); // heavy wobble
                TriggerDamageShake();
                pulseCoroutine = StartCoroutine(CriticalPulseRoutine());
                break;

            case HealthState.Defeated:
                // Knocked down on the ground
                characterBody.localPosition = initialPosition + new Vector3(0f, -0.4f, 0.4f);
                characterBody.localRotation = initialRotation * Quaternion.Euler(85f, 0f, 0f); // collapse backwards
                TriggerDamageShake();
                break;
        }
    }

    private void TriggerDamageShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(0.2f, 0.12f));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 basePos = characterBody.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = UnityEngine.Random.Range(-magnitude, magnitude);
            float offsetZ = UnityEngine.Random.Range(-magnitude, magnitude);
            characterBody.localPosition = basePos + new Vector3(offsetX, 0f, offsetZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        characterBody.localPosition = basePos;
        shakeCoroutine = null;
    }

    private IEnumerator CriticalPulseRoutine()
    {
        if (runtimeMaterial == null) yield break;

        string colorProp = runtimeMaterial.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";

        while (CurrentState == HealthState.Critical)
        {
            float pingPong = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
            Color pulsedColor = Color.Lerp(criticalColor, Color.white, pingPong * 0.45f);
            runtimeMaterial.SetColor(colorProp, pulsedColor);
            yield return null;
        }
    }

    public Color GetStateColor(HealthState state)
    {
        switch (state)
        {
            case HealthState.Healthy: return healthyColor;
            case HealthState.Injured: return injuredColor;
            case HealthState.Critical: return criticalColor;
            case HealthState.Defeated: return defeatedColor;
            default: return Color.white;
        }
    }

    public string GetStateDescription(HealthState state)
    {
        switch (state)
        {
            case HealthState.Healthy: return "Character is at optimal condition. Ready for action.";
            case HealthState.Injured: return "Character took damage! Performance slightly impaired.";
            case HealthState.Critical: return "WARNING: Health critical! Immediate danger of defeat.";
            case HealthState.Defeated: return "DEFEATED: Character cannot continue. Press 'R' to revive.";
            default: return "";
        }
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentState, currentHealthLevel, maxHealthLevel);
    }
}

