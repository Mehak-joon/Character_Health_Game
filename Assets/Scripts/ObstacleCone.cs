using System.Collections;
using UnityEngine;

/// <summary>
/// Training Obstacle Cone
/// Bumping into a cone deals 1 damage to the character, plays a bonk SFX, and wobbles the cone!
/// </summary>
public class ObstacleCone : MonoBehaviour
{
    [SerializeField] private float damageCooldown = 1.2f;

    private static float lastGlobalDamageTime = -10f;
    private Quaternion initialRot;
    private Coroutine wobbleCoroutine;

    private void Awake()
    {
        initialRot = transform.localRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    private void HandleHit(GameObject hitObj)
    {
        CharacterHealthController health = hitObj.GetComponentInParent<CharacterHealthController>();
        if (health != null)
        {
            // Check cooldown to avoid rapid double hits
            if (Time.time - lastGlobalDamageTime < damageCooldown) return;
            lastGlobalDamageTime = Time.time;

            Debug.Log("<color=orange>[Obstacle] Hit Cone! -1 HP</color>");

            // Play bonk SFX
            SoundEffectGenerator sfx = hitObj.GetComponentInChildren<SoundEffectGenerator>() ?? FindFirstObjectByType<SoundEffectGenerator>();
            if (sfx != null)
            {
                sfx.PlayConeBonk();
            }

            // Deal 1 damage to player
            health.TakeDamage();

            // Wobble animation
            if (wobbleCoroutine != null) StopCoroutine(wobbleCoroutine);
            wobbleCoroutine = StartCoroutine(WobbleRoutine());
        }
    }

    private IEnumerator WobbleRoutine()
    {
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float angle = Mathf.Sin(t * Mathf.PI * 6f) * 18f * (1f - t);
            transform.localRotation = initialRot * Quaternion.Euler(angle, 0f, angle * 0.7f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = initialRot;
        wobbleCoroutine = null;
    }
}

