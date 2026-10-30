using System.Collections;
using UnityEngine;

/// <summary>
/// Football Goal Trigger
/// Detects goals, plays fanfare SFX, shoots goal confetti particles, and resets the football.
/// </summary>
public class FootballGoal : MonoBehaviour
{
    [SerializeField] private SoundEffectGenerator soundFX;
    [SerializeField] private Vector3 ballRespawnPosition = new Vector3(0f, 0.4f, 0f);

    public int Score { get; private set; } = 0;

    private ParticleSystem goalConfetti;
    private bool isResettingBall = false;

    private void Awake()
    {
        if (soundFX == null)
        {
            soundFX = FindFirstObjectByType<SoundEffectGenerator>();
        }

        CreateGoalConfetti();
    }

    private void CreateGoalConfetti()
    {
        GameObject obj = new GameObject("FX_GoalConfetti");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = new Vector3(0f, 1.5f, 0f);

        goalConfetti = obj.AddComponent<ParticleSystem>();
        var main = goalConfetti.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1.2f;
        main.startLifetime = 1.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.22f);
        main.gravityModifier = 0.8f;

        var emission = goalConfetti.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 60) });

        var shape = goalConfetti.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 1.5f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        var colorOverLifetime = goalConfetti.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.cyan, 0f),
                new GradientColorKey(Color.yellow, 0.35f),
                new GradientColorKey(Color.magenta, 0.7f),
                new GradientColorKey(Color.green, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isResettingBall) return;

        if (other.gameObject.name.Contains("Football"))
        {
            Score++;
            Debug.Log($"<color=cyan>GOOOOOOAL! Score: {Score}</color>");

            if (goalConfetti != null)
            {
                goalConfetti.Play();
            }

            if (soundFX != null)
            {
                soundFX.PlayGoalFanfare();
            }

            StartCoroutine(ResetBallRoutine(other.attachedRigidbody));
        }
    }

    private IEnumerator ResetBallRoutine(Rigidbody ballRb)
    {
        isResettingBall = true;
        yield return new WaitForSeconds(1.8f);

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.transform.position = ballRespawnPosition;
        }

        isResettingBall = false;
    }
}

