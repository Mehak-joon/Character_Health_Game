using System.Collections;
using UnityEngine;

/// <summary>
/// Trigger placed along the goal endline outside the goal posts.
/// When the ball misses the goal and enters this area, it incurs 1 damage penalty to the character!
/// </summary>
public class GoalMissTrigger : MonoBehaviour
{
    [SerializeField] private Vector3 ballRespawnPos = new Vector3(0f, 0.45f, 2.0f);

    private bool isHandlingMiss = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isHandlingMiss) return;

        if (other.gameObject.name.Contains("Football"))
        {
            StartCoroutine(HandleMissRoutine(other.attachedRigidbody));
        }
    }

    private IEnumerator HandleMissRoutine(Rigidbody ballRb)
    {
        isHandlingMiss = true;

        Debug.Log("<color=red>[PENALTY] Missed Shot! -1 HP</color>");

        // Play miss buzzer sound
        SoundEffectGenerator sfx = FindFirstObjectByType<SoundEffectGenerator>();
        if (sfx != null)
        {
            sfx.PlayMiss();
        }

        // Damage the character
        CharacterHealthController health = FindFirstObjectByType<CharacterHealthController>();
        if (health != null)
        {
            health.TakeDamage();
        }

        yield return new WaitForSeconds(1.2f);

        // Reset ball to center spot
        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.transform.position = ballRespawnPos;
        }

        isHandlingMiss = false;
    }
}
