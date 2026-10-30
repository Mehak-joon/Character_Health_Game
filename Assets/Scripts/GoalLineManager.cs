using System.Collections;
using UnityEngine;

/// <summary>
/// Unified Goal & Miss Detection System
/// Monitors the football's trajectory crossing the goal endline (Z >= 13.8m).
/// - Inside the posts (-3.5m <= X <= 3.5m, Y <= 2.8m) => GOAL (+1 Score, Fanfare SFX, Confetti)
/// - Outside the posts (X < -3.5m or X > 3.5m) => MISSED SHOT (-1 HP Damage, Buzzer SFX)
/// </summary>
public class GoalLineManager : MonoBehaviour
{
    [Header("Field References")]
    [SerializeField] private Rigidbody footballRb;
    [SerializeField] private CharacterHealthController healthController;
    [SerializeField] private SoundEffectGenerator soundFX;

    [Header("Goal Dimensions")]
    [SerializeField] private float goalLineZ = 13.8f;
    [SerializeField] private float goalWidth = 7.0f; // from -3.5 to +3.5
    [SerializeField] private float goalHeight = 2.8f;

    [Header("Ball Reset Settings")]
    [SerializeField] private Vector3 ballKickoffSpot = new Vector3(0f, 0.45f, 1.8f);

    public int Score { get; private set; } = 0;
    public string BannerMessage { get; private set; } = "";
    public Color BannerColor { get; private set; } = Color.white;

    private ParticleSystem goalConfetti;
    private bool isHandlingShot = false;

    private void Awake()
    {
        if (footballRb == null)
        {
            GameObject ballObj = GameObject.Find("Football");
            if (ballObj != null) footballRb = ballObj.GetComponent<Rigidbody>();
        }

        if (healthController == null)
        {
            healthController = FindFirstObjectByType<CharacterHealthController>();
        }

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
        obj.transform.position = new Vector3(0f, 2.5f, goalLineZ + 1.2f);

        goalConfetti = obj.AddComponent<ParticleSystem>();
        var main = goalConfetti.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1.2f;
        main.startLifetime = 1.6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
        main.gravityModifier = 0.9f;

        var emission = goalConfetti.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 75) });

        var shape = goalConfetti.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 2.5f;
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

    private void Update()
    {
        if (footballRb == null || isHandlingShot) return;

        Vector3 ballPos = footballRb.position;

        // Check if ball flew over open roof/stadium walls out of bounds
        if (Mathf.Abs(ballPos.x) > 14.2f || ballPos.z < -20.0f || ballPos.z > 21.2f || ballPos.y < -1.0f)
        {
            StartCoroutine(TriggerOutOfBoundsRoutine());
            return;
        }

        // Has the ball reached or crossed the goal line?
        if (ballPos.z >= goalLineZ)
        {
            float halfWidth = goalWidth * 0.5f;

            if (ballPos.x >= -halfWidth && ballPos.x <= halfWidth && ballPos.y <= goalHeight)
            {
                // ON-TARGET: GOAL!
                StartCoroutine(TriggerGoalRoutine());
            }
            else
            {
                // OFF-TARGET: MISSED SHOT!
                StartCoroutine(TriggerMissRoutine());
            }
        }
    }

    private IEnumerator TriggerOutOfBoundsRoutine()
    {
        isHandlingShot = true;
        BannerMessage = "OUT OF BOUNDS! 📢";
        BannerColor = new Color(1.0f, 0.85f, 0.2f);

        Debug.Log("<color=yellow>[BOUNDS] Ball flew out of bounds! Respawing at kickoff spot...</color>");

        if (soundFX != null) soundFX.PlayWhistle();

        yield return new WaitForSeconds(1.3f);

        ResetFootball();
        BannerMessage = "";
        isHandlingShot = false;
    }

    private IEnumerator TriggerGoalRoutine()
    {
        isHandlingShot = true;
        Score++;
        BannerMessage = "GOAL! 🎉";
        BannerColor = new Color(0.2f, 1.0f, 0.4f);

        Debug.Log($"<color=cyan>[GOAL] Score: {Score}!</color>");

        if (goalConfetti != null) goalConfetti.Play();
        if (soundFX != null) soundFX.PlayGoalFanfare();

        yield return new WaitForSeconds(1.8f);

        ResetFootball();
        BannerMessage = "";
        isHandlingShot = false;
    }

    private IEnumerator TriggerMissRoutine()
    {
        isHandlingShot = true;
        BannerMessage = "MISSED SHOT! -1 HP ⚠️";
        BannerColor = new Color(1.0f, 0.3f, 0.2f);

        Debug.Log("<color=red>[PENALTY] Missed shot past goal line! -1 HP</color>");

        if (soundFX != null) soundFX.PlayMiss();
        if (healthController != null) healthController.TakeDamage();

        yield return new WaitForSeconds(1.4f);

        ResetFootball();
        BannerMessage = "";
        isHandlingShot = false;
    }

    private void ResetFootball()
    {
        if (footballRb != null)
        {
            footballRb.linearVelocity = Vector3.zero;
            footballRb.angularVelocity = Vector3.zero;
            footballRb.transform.position = ballKickoffSpot;
        }
    }
}

