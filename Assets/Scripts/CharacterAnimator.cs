using System.Collections;
using UnityEngine;

/// <summary>
/// Procedural 3D Animator for Articulated Robot Character
/// Controls idle hover, arm swing, breathing, damage flinch, critical trembling, defeat collapse, and revive jump.
/// </summary>
public class CharacterAnimator : MonoBehaviour
{
    [Header("Articulated Body Parts")]
    public Transform bodyRoot;
    public Transform headTransform;
    public Transform chestCoreTransform;
    public Transform leftArmTransform;
    public Transform rightArmTransform;
    public Transform leftLegTransform;
    public Transform rightLegTransform;
    public Transform hoverBaseTransform;

    [Header("Core Light / Emissive / Materials")]
    public Renderer chestCoreRenderer;
    public Renderer leftEyeRenderer;
    public Renderer rightEyeRenderer;
    public Renderer jerseyRenderer;

    [Header("Movement Tracking")]
    public bool isMoving = false;
    public bool isSprinting = false;
    public bool isSliding = false;
    public float movementSpeed = 0f;

    private CharacterHealthController.HealthState currentState = CharacterHealthController.HealthState.Healthy;

    private Vector3 initialRootPos;
    private Quaternion initialRootRot;
    private Quaternion initialHeadRot;
    private Quaternion initialLeftArmRot;
    private Quaternion initialRightArmRot;
    private Quaternion initialLeftLegRot;
    private Quaternion initialRightLegRot;

    private Coroutine recoilCoroutine;
    private Coroutine reviveCoroutine;
    private Coroutine kickCoroutine;
    private Material chestCoreMat;
    private Material eyeMat;
    private float runStrideTime = 0f;

    private void Awake()
    {
        if (bodyRoot == null) bodyRoot = transform;

        initialRootPos = bodyRoot.localPosition;
        initialRootRot = bodyRoot.localRotation;

        if (headTransform != null) initialHeadRot = headTransform.localRotation;
        if (leftArmTransform != null) initialLeftArmRot = leftArmTransform.localRotation;
        if (rightArmTransform != null) initialRightArmRot = rightArmTransform.localRotation;
        if (leftLegTransform != null) initialLeftLegRot = leftLegTransform.localRotation;
        if (rightLegTransform != null) initialRightLegRot = rightLegTransform.localRotation;

        if (chestCoreRenderer != null) chestCoreMat = chestCoreRenderer.material;
        if (leftEyeRenderer != null) eyeMat = leftEyeRenderer.material;
    }

    private void Update()
    {
        if (reviveCoroutine != null) return; // let revive animation play exclusively

        if (currentState == CharacterHealthController.HealthState.Defeated)
        {
            AnimateDefeated();
            return;
        }

        if (isSliding)
        {
            AnimateSlide();
            return;
        }

        switch (currentState)
        {
            case CharacterHealthController.HealthState.Healthy:
                AnimateHealthy();
                break;

            case CharacterHealthController.HealthState.Injured:
                AnimateInjured();
                break;

            case CharacterHealthController.HealthState.Critical:
                AnimateCritical();
                break;
        }
    }

    private void AnimateSlide()
    {
        if (recoilCoroutine == null)
        {
            bodyRoot.localPosition = initialRootPos + new Vector3(0f, -0.42f, 0.2f);
            bodyRoot.localRotation = initialRootRot * Quaternion.Euler(52f, 0f, -22f);
        }

        if (rightLegTransform != null) rightLegTransform.localRotation = initialRightLegRot * Quaternion.Euler(-75f, 0f, 0f);
        if (leftLegTransform != null) leftLegTransform.localRotation = initialLeftLegRot * Quaternion.Euler(45f, 0f, 25f);
        if (leftArmTransform != null) leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(-30f, 0f, -40f);
        if (rightArmTransform != null) rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(20f, 0f, 30f);
    }

    private void AnimateHealthy()
    {
        float t = Time.time;

        if (isMoving)
        {
            float strideSpeed = isSprinting ? 18f : 12f;
            runStrideTime += Time.deltaTime * strideSpeed;
            float stride = Mathf.Sin(runStrideTime);

            // Real human double-frequency vertical bounce & lateral weight shift
            float doubleFreqBounce = Mathf.Abs(Mathf.Sin(runStrideTime)) * (isSprinting ? 0.08f : 0.05f);
            float lateralHipSway = Mathf.Sin(runStrideTime) * 0.035f;
            float pelvisTwist = Mathf.Sin(runStrideTime) * 7f;
            float shoulderCounterTwist = -Mathf.Sin(runStrideTime) * 10f;

            // Natural leg stride with knee bend
            float lPitch = stride * 32f;
            float lKneeBend = (stride < 0f) ? Mathf.Sin(runStrideTime) * -25f : 0f;
            if (leftLegTransform != null)
                leftLegTransform.localRotation = initialLeftLegRot * Quaternion.Euler(lPitch + lKneeBend, 0f, 0f);

            float rPitch = -stride * 32f;
            float rKneeBend = (stride > 0f) ? Mathf.Sin(runStrideTime) * 25f : 0f;
            if (rightLegTransform != null && kickCoroutine == null)
                rightLegTransform.localRotation = initialRightLegRot * Quaternion.Euler(rPitch + rKneeBend, 0f, 0f);

            // Counter-swinging arms with elbow flexion
            float armPitch = -stride * 28f;
            if (leftArmTransform != null)
                leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(armPitch, shoulderCounterTwist, -5f);
            if (rightArmTransform != null)
                rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(-armPitch, shoulderCounterTwist, 5f);

            if (recoilCoroutine == null)
            {
                bodyRoot.localPosition = initialRootPos + new Vector3(lateralHipSway, doubleFreqBounce, 0f);
                bodyRoot.localRotation = initialRootRot * Quaternion.Euler(isSprinting ? 11f : 6f, pelvisTwist, lateralHipSway * 35f);
            }
        }
        else
        {
            // Reset legs smoothly to neutral standing
            if (leftLegTransform != null) leftLegTransform.localRotation = Quaternion.Slerp(leftLegTransform.localRotation, initialLeftLegRot, Time.deltaTime * 9f);
            if (rightLegTransform != null && kickCoroutine == null) rightLegTransform.localRotation = Quaternion.Slerp(rightLegTransform.localRotation, initialRightLegRot, Time.deltaTime * 9f);

            // Athletic standing weight shift & breathing
            float breathingBob = Mathf.Sin(t * 2.2f) * 0.03f;
            float weightShift = Mathf.Sin(t * 1.1f) * 0.02f;

            if (recoilCoroutine == null)
            {
                bodyRoot.localPosition = initialRootPos + new Vector3(weightShift, breathingBob, 0f);
                bodyRoot.localRotation = initialRootRot * Quaternion.Euler(3f, weightShift * 20f, 0f);
            }

            if (headTransform != null) headTransform.localRotation = initialHeadRot * Quaternion.Euler(0f, Mathf.Sin(t * 0.9f) * 5f, 0f);
            if (leftArmTransform != null) leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(Mathf.Sin(t * 2.2f) * 6f, 0f, -4f);
            if (rightArmTransform != null) rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(-Mathf.Sin(t * 2.2f) * 6f, 0f, 4f);
        }

        if (chestCoreTransform != null)
        {
            float scale = 1f + Mathf.Sin(t * 3f) * 0.08f;
            chestCoreTransform.localScale = Vector3.one * scale;
        }
    }

    private void AnimateInjured()
    {
        float t = Time.time;

        if (isMoving)
        {
            runStrideTime += Time.deltaTime * 8.0f; // slower, painful limping stride
            float stride = Mathf.Sin(runStrideTime);

            // Asymmetric limp favoring injured leg
            float lPitch = stride * 24f;
            float rPitch = -stride * 14f;

            if (leftLegTransform != null) leftLegTransform.localRotation = initialLeftLegRot * Quaternion.Euler(lPitch, 0f, 6f);
            if (rightLegTransform != null && kickCoroutine == null) rightLegTransform.localRotation = initialRightLegRot * Quaternion.Euler(rPitch, 0f, -4f);

            if (leftArmTransform != null) leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(20f, 0f, 15f); // clutching side
            if (rightArmTransform != null) rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(-stride * 16f, 0f, -6f);

            if (recoilCoroutine == null)
            {
                float limpHitch = (stride > 0f) ? 0.04f : -0.04f;
                bodyRoot.localPosition = initialRootPos + new Vector3(stride * 0.03f, -0.06f + limpHitch, 0f);
                bodyRoot.localRotation = initialRootRot * Quaternion.Euler(12f, stride * 4f, 8f);
            }
        }
        else
        {
            float hover = Mathf.Sin(t * 1.5f) * 0.025f;
            if (recoilCoroutine == null)
            {
                bodyRoot.localPosition = initialRootPos + new Vector3(0f, hover - 0.06f, 0f);
                bodyRoot.localRotation = initialRootRot * Quaternion.Euler(10f, 0f, 7f);
            }

            if (headTransform != null) headTransform.localRotation = initialHeadRot * Quaternion.Euler(14f, 0f, -6f);
            if (leftArmTransform != null) leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(18f, 0f, 12f);
            if (rightArmTransform != null) rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(-8f, 0f, -6f);
        }
    }

    private void AnimateCritical()
    {
        float t = Time.time;

        // Jittery shivering / trembling
        float shakeX = (Mathf.PerlinNoise(t * 25f, 0f) - 0.5f) * 0.04f;
        float shakeZ = (Mathf.PerlinNoise(0f, t * 25f) - 0.5f) * 0.04f;
        float tiltJitter = (Mathf.PerlinNoise(t * 20f, 5f) - 0.5f) * 6f;

        if (recoilCoroutine == null)
        {
            bodyRoot.localPosition = initialRootPos + new Vector3(shakeX, -0.1f, shakeZ);
            bodyRoot.localRotation = initialRootRot * Quaternion.Euler(14f + tiltJitter, 0f, -8f + tiltJitter);
        }

        if (headTransform != null)
        {
            headTransform.localRotation = initialHeadRot * Quaternion.Euler(20f + tiltJitter, 0f, -6f);
        }

        // Erratic arm spasm
        if (leftArmTransform != null)
        {
            leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(20f + tiltJitter * 2f, 0f, 15f);
        }
        if (rightArmTransform != null)
        {
            rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(20f - tiltJitter * 2f, 0f, -15f);
        }
    }

    private void AnimateDefeated()
    {
        // Collapsed flat on the floor
        if (recoilCoroutine == null)
        {
            bodyRoot.localPosition = initialRootPos + new Vector3(0f, -0.45f, 0.45f);
            bodyRoot.localRotation = initialRootRot * Quaternion.Euler(85f, 0f, 0f);
        }

        if (headTransform != null)
        {
            headTransform.localRotation = initialHeadRot * Quaternion.Euler(30f, 25f, 0f);
        }

        if (leftArmTransform != null)
        {
            leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(0f, 0f, 40f);
        }
        if (rightArmTransform != null)
        {
            rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(0f, 0f, -40f);
        }
    }

    public void SetState(CharacterHealthController.HealthState state)
    {
        currentState = state;
        UpdateEmissives(state);
    }

    private void UpdateEmissives(CharacterHealthController.HealthState state)
    {
        Color eyeColor;
        Color coreColor;

        switch (state)
        {
            case CharacterHealthController.HealthState.Healthy:
                eyeColor = new Color(0.2f, 1.0f, 0.5f);
                coreColor = new Color(0.2f, 0.9f, 0.4f);
                break;
            case CharacterHealthController.HealthState.Injured:
                eyeColor = new Color(1.0f, 0.8f, 0.1f);
                coreColor = new Color(1.0f, 0.7f, 0.1f);
                break;
            case CharacterHealthController.HealthState.Critical:
                eyeColor = new Color(1.0f, 0.2f, 0.1f);
                coreColor = new Color(1.0f, 0.15f, 0.1f);
                break;
            case CharacterHealthController.HealthState.Defeated:
            default:
                eyeColor = new Color(0.1f, 0.1f, 0.1f);
                coreColor = new Color(0.1f, 0.1f, 0.1f);
                break;
        }

        SetRendererColor(chestCoreRenderer, coreColor);
        SetRendererColor(leftEyeRenderer, eyeColor);
        SetRendererColor(rightEyeRenderer, eyeColor);

        // Jersey color changes with health state
        Color jerseyCol;
        switch (state)
        {
            case CharacterHealthController.HealthState.Healthy: jerseyCol = new Color(0.18f, 0.8f, 0.35f); break; // Vibrant Green
            case CharacterHealthController.HealthState.Injured: jerseyCol = new Color(0.95f, 0.75f, 0.1f); break; // Athletic Gold
            case CharacterHealthController.HealthState.Critical: jerseyCol = new Color(0.9f, 0.2f, 0.15f); break; // Warning Red
            default: jerseyCol = new Color(0.32f, 0.32f, 0.38f); break; // Defeated Charcoal Gray
        }
        SetRendererColor(jerseyRenderer, jerseyCol);
    }

    public void TriggerKick()
    {
        if (kickCoroutine != null) StopCoroutine(kickCoroutine);
        kickCoroutine = StartCoroutine(KickRoutine());
    }

    private IEnumerator KickRoutine()
    {
        if (rightLegTransform == null) yield break;
        float elapsed = 0f;
        float duration = 0.22f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float kickAngle = Mathf.Sin(t * Mathf.PI) * -55f; // kick leg forward
            rightLegTransform.localRotation = initialRightLegRot * Quaternion.Euler(kickAngle, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rightLegTransform.localRotation = initialRightLegRot;
        kickCoroutine = null;
    }

    private void SetRendererColor(Renderer r, Color c)
    {
        if (r == null) return;
        Material mat = r.material;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", c * 1.5f);
            mat.EnableKeyword("_EMISSION");
        }
    }

    public void TriggerRecoil()
    {
        if (recoilCoroutine != null) StopCoroutine(recoilCoroutine);
        recoilCoroutine = StartCoroutine(RecoilRoutine());
    }

    private IEnumerator RecoilRoutine()
    {
        Vector3 startPos = bodyRoot.localPosition;
        Vector3 kickbackPos = startPos + new Vector3(0f, 0.08f, -0.2f); // kick back

        float elapsed = 0f;
        float duration = 0.18f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            bodyRoot.localPosition = Vector3.Lerp(kickbackPos, startPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        bodyRoot.localPosition = startPos;
        recoilCoroutine = null;
    }

    public void TriggerReviveJump()
    {
        if (reviveCoroutine != null) StopCoroutine(reviveCoroutine);
        reviveCoroutine = StartCoroutine(ReviveJumpRoutine());
    }

    private IEnumerator ReviveJumpRoutine()
    {
        float elapsed = 0f;
        float duration = 0.65f;

        Vector3 startPos = bodyRoot.localPosition;
        Quaternion startRot = bodyRoot.localRotation;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Arc trajectory with spin
            float heightArc = Mathf.Sin(t * Mathf.PI) * 0.45f;
            bodyRoot.localPosition = Vector3.Lerp(startPos, initialRootPos, t) + new Vector3(0f, heightArc, 0f);

            // 360 spin during recovery
            float spinAngle = Mathf.Lerp(0f, 360f, t);
            bodyRoot.localRotation = Quaternion.Slerp(startRot, initialRootRot, t) * Quaternion.Euler(0f, spinAngle, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        bodyRoot.localPosition = initialRootPos;
        bodyRoot.localRotation = initialRootRot;
        reviveCoroutine = null;
    }
}

