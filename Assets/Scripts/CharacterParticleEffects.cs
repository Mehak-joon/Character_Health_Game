using UnityEngine;

/// <summary>
/// Character Particle Effects Controller
/// Generates and manages dynamic visual particles for damage, critical state, defeat, and revival.
/// </summary>
public class CharacterParticleEffects : MonoBehaviour
{
    private ParticleSystem hitSparks;
    private ParticleSystem criticalSmoke;
    private ParticleSystem reviveAura;
    private ParticleSystem defeatDust;

    private void Awake()
    {
        CreateHitSparks();
        CreateCriticalSmoke();
        CreateReviveAura();
        CreateDefeatDust();
    }

    private void CreateHitSparks()
    {
        GameObject obj = new GameObject("FX_HitSparks");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = new Vector3(0f, 0.4f, 0f);

        hitSparks = obj.AddComponent<ParticleSystem>();
        var main = hitSparks.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.3f;
        main.startLifetime = 0.35f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 6.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.8f, 0.2f), new Color(1f, 0.2f, 0.1f));
        main.gravityModifier = 1.2f;

        var emission = hitSparks.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });

        var shape = hitSparks.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        var colorOverLifetime = hitSparks.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.red, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;
    }

    private void CreateCriticalSmoke()
    {
        GameObject obj = new GameObject("FX_CriticalSmoke");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        criticalSmoke = obj.AddComponent<ParticleSystem>();
        var main = criticalSmoke.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 1.0f;
        main.startLifetime = 0.8f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        main.startColor = new Color(0.2f, 0.2f, 0.2f, 0.65f); // Dark smoke
        main.gravityModifier = -0.4f; // rise up

        var emission = criticalSmoke.emission;
        emission.rateOverTime = 14;

        var shape = criticalSmoke.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.15f;

        var colorOverLifetime = criticalSmoke.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.4f, 0.1f, 0.1f), 0f), new GradientColorKey(Color.black, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;
    }

    private void CreateReviveAura()
    {
        GameObject obj = new GameObject("FX_ReviveAura");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = new Vector3(0f, 0f, 0f);

        reviveAura = obj.AddComponent<ParticleSystem>();
        var main = reviveAura.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.7f;
        main.startLifetime = 0.75f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 4.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.3f, 1f, 0.5f), new Color(1f, 0.95f, 0.4f));
        main.gravityModifier = -0.5f;

        var emission = reviveAura.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 40) });

        var shape = reviveAura.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.8f;
        shape.rotation = new Vector3(90f, 0f, 0f);

        var colorOverLifetime = reviveAura.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.green, 0f), new GradientColorKey(Color.yellow, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;
    }

    private void CreateDefeatDust()
    {
        GameObject obj = new GameObject("FX_DefeatDust");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = new Vector3(0f, 0.1f, 0.3f);

        defeatDust = obj.AddComponent<ParticleSystem>();
        var main = defeatDust.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.5f;
        main.startLifetime = 0.6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startColor = new Color(0.6f, 0.6f, 0.65f, 0.5f);

        var emission = defeatDust.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.1f, 30) });

        var shape = defeatDust.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.4f;
    }

    public void PlayHitSparks()
    {
        if (hitSparks != null)
        {
            hitSparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitSparks.Play();
        }
    }

    public void SetCriticalSmoke(bool active)
    {
        if (criticalSmoke == null) return;

        if (active && !criticalSmoke.isPlaying)
        {
            criticalSmoke.Play();
        }
        else if (!active && criticalSmoke.isPlaying)
        {
            criticalSmoke.Stop();
        }
    }

    public void PlayReviveAura()
    {
        if (reviveAura != null)
        {
            reviveAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            reviveAura.Play();
        }
    }

    public void PlayDefeatDust()
    {
        if (defeatDust != null)
        {
            defeatDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            defeatDust.Play();
        }
    }
}

