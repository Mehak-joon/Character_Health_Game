using UnityEngine;

/// <summary>
/// Procedural Sound Effect Generator
/// Generates crisp retro-modern sound effects dynamically using math & AudioClip.Create.
/// No external audio asset files needed!
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundEffectGenerator : MonoBehaviour
{
    private AudioSource audioSource;

    private AudioClip hitClip;
    private AudioClip criticalAlarmClip;
    private AudioClip defeatClip;
    private AudioClip reviveClip;
    private AudioClip kickClip;
    private AudioClip goalFanfareClip;
    private AudioClip missClip;
    private AudioClip coneBonkClip;
    private AudioClip whistleClip;
    private AudioClip slideClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for crisp UI/gameplay audio

        GenerateAudioClips();
    }

    private void GenerateAudioClips()
    {
        hitClip = CreateHitClip();
        criticalAlarmClip = CreateCriticalAlarmClip();
        defeatClip = CreateDefeatClip();
        reviveClip = CreateReviveClip();
        kickClip = CreateKickClip();
        goalFanfareClip = CreateGoalFanfareClip();
        missClip = CreateMissClip();
        coneBonkClip = CreateConeBonkClip();
        whistleClip = CreateWhistleClip();
        slideClip = CreateSlideClip();
    }

    /// <summary>
    /// Punchy hit impact sound (sine pitch drop + noise click)
    /// </summary>
    private AudioClip CreateHitClip()
    {
        int sampleRate = 44100;
        float duration = 0.22f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Exponential pitch drop from 340Hz down to 60Hz
            float freq = Mathf.Lerp(340f, 60f, progress * progress);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);

            // Punch envelope: fast attack, quick decay
            float env = Mathf.Exp(-14f * progress);

            // Subtle white noise transient on attack
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-45f * progress) * 0.4f;

            samples[i] = Mathf.Clamp((sine * 0.7f + noise) * env, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_Hit", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Urgent two-tone warning beep for Critical health
    /// </summary>
    private AudioClip CreateCriticalAlarmClip()
    {
        int sampleRate = 44100;
        float duration = 0.32f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Two-tone alternating beep (880Hz -> 660Hz)
            float freq = (progress < 0.5f) ? 880f : 660f;
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);

            // Pulse envelope
            float segmentT = (progress < 0.5f) ? (progress * 2f) : ((progress - 0.5f) * 2f);
            float env = Mathf.Sin(segmentT * Mathf.PI);

            samples[i] = Mathf.Clamp(sine * env * 0.5f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_CriticalAlarm", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Dramatic downward frequency sweep and low rumble for Defeated state
    /// </summary>
    private AudioClip CreateDefeatClip()
    {
        int sampleRate = 44100;
        float duration = 0.75f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Pitch falling from 440Hz down to 40Hz
            float freq = Mathf.Lerp(440f, 40f, Mathf.Pow(progress, 0.6f));
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);

            // Subtle square wave distortion for robotic shutdown flavor
            float square = Mathf.Sign(sine) * 0.25f;

            float env = (1f - progress) * (1f - progress);

            samples[i] = Mathf.Clamp((sine * 0.6f + square) * env, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_Defeated", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Uplifting 4-note ascending chime (C5 - E5 - G5 - C6) for Revive / Reset
    /// </summary>
    private AudioClip CreateReviveClip()
    {
        int sampleRate = 44100;
        float duration = 0.65f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
        float noteDuration = duration / freqs.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / noteDuration), 0, freqs.Length - 1);
            float noteLocalT = t - (noteIndex * noteDuration);

            float freq = freqs[noteIndex];
            // Bell-like fundamental + octave harmonic
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.3f;

            float noteProgress = noteLocalT / noteDuration;
            float env = Mathf.Exp(-4.5f * noteProgress);

            samples[i] = Mathf.Clamp(sine * env * 0.5f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_Revive", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlayHit()
    {
        if (audioSource != null && hitClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(hitClip, 0.8f);
        }
    }

    public void PlayCriticalAlarm()
    {
        if (audioSource != null && criticalAlarmClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(criticalAlarmClip, 0.75f);
        }
    }

    public void PlayDefeated()
    {
        if (audioSource != null && defeatClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(defeatClip, 0.9f);
        }
    }

    public void PlayRevive()
    {
        if (audioSource != null && reviveClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(reviveClip, 0.85f);
        }
    }

    /// <summary>
    /// Thumping kick sound for the football
    /// </summary>
    private AudioClip CreateKickClip()
    {
        int sampleRate = 44100;
        float duration = 0.16f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Deep punchy bass drop from 180Hz down to 45Hz
            float freq = Mathf.Lerp(180f, 45f, progress);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);
            float click = (Random.value * 2f - 1f) * Mathf.Exp(-60f * progress) * 0.5f;

            float env = Mathf.Exp(-12f * progress);
            samples[i] = Mathf.Clamp((sine * 0.85f + click) * env, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_Kick", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Festive celebratory fanfare chime when a goal is scored
    /// </summary>
    private AudioClip CreateGoalFanfareClip()
    {
        int sampleRate = 44100;
        float duration = 0.9f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] melody = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f, 1318.51f }; // C5, E5, G5, C6, E6
        float noteDuration = duration / melody.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / noteDuration), 0, melody.Length - 1);
            float noteLocalT = t - (noteIndex * noteDuration);

            float freq = melody[noteIndex];
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.55f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.25f;

            float noteProgress = noteLocalT / noteDuration;
            float env = Mathf.Exp(-3.2f * noteProgress);

            samples[i] = Mathf.Clamp(sine * env * 0.5f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_Goal", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlayKick()
    {
        if (audioSource != null && kickClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(kickClip, 0.9f);
        }
    }

    public void PlayGoalFanfare()
    {
        if (audioSource != null && goalFanfareClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(goalFanfareClip, 0.95f);
        }
    }

    /// <summary>
    /// Low error buzzer sound when missing a shot or incurring penalty
    /// </summary>
    private AudioClip CreateMissClip()
    {
        int sampleRate = 44100;
        float duration = 0.38f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Harsh square wave buzz at 130Hz
            float freq = 130f;
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);
            float buzz = Mathf.Sign(sine);

            float env = Mathf.Exp(-4f * progress);
            samples[i] = Mathf.Clamp(buzz * env * 0.45f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_MissBuzzer", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Hollow plastic bonk sound when hitting a training cone
    /// </summary>
    private AudioClip CreateConeBonkClip()
    {
        int sampleRate = 44100;
        float duration = 0.2f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // High resonant pop decaying rapidly (520Hz down to 240Hz)
            float freq = Mathf.Lerp(520f, 240f, progress);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);

            float env = Mathf.Exp(-18f * progress);
            samples[i] = Mathf.Clamp(sine * env * 0.7f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_ConeBonk", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlayMiss()
    {
        if (audioSource != null && missClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(missClip, 0.8f);
        }
    }

    public void PlayConeBonk()
    {
        if (audioSource != null && coneBonkClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.1f);
            audioSource.PlayOneShot(coneBonkClip, 0.85f);
        }
    }

    /// <summary>
    /// Crisp high-pitch referee whistle for out of bounds
    /// </summary>
    private AudioClip CreateWhistleClip()
    {
        int sampleRate = 44100;
        float duration = 0.45f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Whistle modulation: 2400Hz + vibrato
            float vibrato = Mathf.Sin(2f * Mathf.PI * 25f * t) * 60f;
            float freq1 = 2400f + vibrato;
            float freq2 = 2850f + vibrato; // dual-tone pea whistle
            float sine = Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.5f
                       + Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.4f;

            float env = Mathf.Sin(progress * Mathf.PI);
            samples[i] = Mathf.Clamp(sine * env * 0.45f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_Whistle", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Turf grass slide friction whoosh sound
    /// </summary>
    private AudioClip CreateSlideClip()
    {
        int sampleRate = 44100;
        float duration = 0.5f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float progress = (float)i / sampleCount;
            float noise = (Random.value * 2f - 1f);
            float env = Mathf.Sin(progress * Mathf.PI);
            samples[i] = Mathf.Clamp(noise * env * 0.35f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_Slide", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlayWhistle()
    {
        if (audioSource != null && whistleClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(whistleClip, 0.9f);
        }
    }

    public void PlaySlide()
    {
        if (audioSource != null && slideClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(slideClip, 0.75f);
        }
    }
}

