using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller for Character Health State Machine
/// Displays current health state, health bar, description, and interactive controls.
/// Features both uGUI Canvas support and immediate OnGUI fallback overlay.
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("Controller Reference")]
    [SerializeField] private CharacterHealthController healthController;

    [Header("uGUI Canvas References (Optional)")]
    [SerializeField] private TextMeshProUGUI stateLabelTMP;
    [SerializeField] private TextMeshProUGUI descriptionLabelTMP;
    [SerializeField] private TextMeshProUGUI hpTextTMP;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image stateBadgeImage;
    [SerializeField] private Button damageButton;
    [SerializeField] private Button resetButton;

    [Header("Immediate OnGUI Overlay")]
    [SerializeField] private bool showOnGUIOverlay = true;

    private void Awake()
    {
        if (healthController == null)
        {
            healthController = FindFirstObjectByType<CharacterHealthController>();
        }
    }

    private void Start()
    {
        if (damageButton != null)
        {
            damageButton.onClick.AddListener(OnDamageButtonClicked);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }

        if (healthController != null)
        {
            healthController.OnHealthChanged += HandleHealthChanged;
            // Initial update
            HandleHealthChanged(healthController.CurrentState, healthController.CurrentHealthLevel, healthController.MaxHealthLevel);
        }
    }

    private void OnDestroy()
    {
        if (healthController != null)
        {
            healthController.OnHealthChanged -= HandleHealthChanged;
        }

        if (damageButton != null)
        {
            damageButton.onClick.RemoveListener(OnDamageButtonClicked);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(OnResetButtonClicked);
        }
    }

    private void HandleHealthChanged(CharacterHealthController.HealthState state, int currentHealth, int maxHealth)
    {
        Color stateColor = healthController != null ? healthController.GetStateColor(state) : Color.white;
        string stateName = state.ToString().ToUpper();
        string desc = healthController != null ? healthController.GetStateDescription(state) : "";
        float fillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        if (stateLabelTMP != null)
        {
            stateLabelTMP.text = stateName;
            stateLabelTMP.color = stateColor;
        }

        if (descriptionLabelTMP != null)
        {
            descriptionLabelTMP.text = desc;
        }

        if (hpTextTMP != null)
        {
            hpTextTMP.text = $"HP: {currentHealth} / {maxHealth}";
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = fillAmount;
            healthBarFill.color = stateColor;
        }

        if (stateBadgeImage != null)
        {
            stateBadgeImage.color = new Color(stateColor.r, stateColor.g, stateColor.b, 0.25f);
        }
    }

    public void OnDamageButtonClicked()
    {
        if (healthController != null)
        {
            healthController.TakeDamage();
        }
    }

    public void OnResetButtonClicked()
    {
        if (healthController != null)
        {
            healthController.ResetHealth();
        }
    }

    private void OnGUI()
    {
        if (!showOnGUIOverlay || healthController == null) return;

        // Custom stylized IMGUI HUD in the top-left corner
        GUI.skin.box.fontSize = 14;
        GUI.skin.button.fontSize = 13;

        float panelWidth = 410f;
        float panelHeight = 350f;
        Rect panelRect = new Rect(20f, 20f, panelWidth, panelHeight);

        GUI.Box(panelRect, "");

        GUILayout.BeginArea(new Rect(panelRect.x + 15, panelRect.y + 15, panelRect.width - 30, panelRect.height - 30));

        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        titleStyle.normal.textColor = Color.white;
        GUILayout.Label("TOPIC: CHARACTER HEALTH", titleStyle);

        GUILayout.Space(6);

        // State Badge
        CharacterHealthController.HealthState state = healthController.CurrentState;
        Color stateColor = healthController.GetStateColor(state);

        GUIStyle stateStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };
        stateStyle.normal.textColor = stateColor;
        GUILayout.Label($"State: {state.ToString().ToUpper()}", stateStyle);

        // Health details
        GUIStyle hpStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        hpStyle.normal.textColor = Color.white;
        GUILayout.Label($"Health: {healthController.CurrentHealthLevel} / {healthController.MaxHealthLevel}", hpStyle);

        // Progress bar
        float pct = (float)healthController.CurrentHealthLevel / healthController.MaxHealthLevel;
        Color oldColor = GUI.color;
        GUI.color = stateColor;
        Rect barRect = GUILayoutUtility.GetRect(panelWidth - 30, 18);
        GUI.HorizontalScrollbar(barRect, 0f, pct, 0f, 1f);
        GUI.color = oldColor;

        // Description
        GUIStyle descStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            wordWrap = true
        };
        descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
        GUILayout.Label(healthController.GetStateDescription(state), descStyle);

        GUILayout.Space(8);

        // Banner Notification if Goal or Miss
        GoalLineManager goalLine = Object.FindFirstObjectByType<GoalLineManager>();
        if (goalLine != null && !string.IsNullOrEmpty(goalLine.BannerMessage))
        {
            GUIStyle bannerStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            Color oldC = GUI.color;
            GUI.color = goalLine.BannerColor;
            GUI.Box(new Rect(Screen.width * 0.5f - 180f, 25f, 360f, 55f), goalLine.BannerMessage, bannerStyle);
            GUI.color = oldC;
        }

        // Football Score & Movement Hint
        FootballGoal goal = Object.FindFirstObjectByType<FootballGoal>();
        int currentScore = goalLine != null ? goalLine.Score : (goal != null ? goal.Score : 0);

        GUIStyle scoreStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };
        scoreStyle.normal.textColor = Color.yellow;
        GUILayout.Label($"⚽ Goals Scored: {currentScore}", scoreStyle);

        GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Italic
        };
        hintStyle.normal.textColor = new Color(0.7f, 0.9f, 1f);
        GUILayout.Label("• [W,A,S,D] Move | [Arrows] Cam | [V] 1st/3rd View\n• [Left Shift] Sprint | [Shift Tap] Slide Tackle Power-Kick!", hintStyle);

        GUIStyle ruleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            wordWrap = true
        };
        ruleStyle.normal.textColor = new Color(1f, 0.75f, 0.5f);
        GUILayout.Label("• Hitting a cone = -1 HP\n• Missing wide past goal line = -1 HP", ruleStyle);

        GUILayout.Space(6);

        // Interactive Buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Damage [H]", GUILayout.Height(30)))
        {
            healthController.TakeDamage();
        }
        if (GUILayout.Button("Reset [R]", GUILayout.Height(30)))
        {
            healthController.ResetHealth();
        }

        SmoothCameraFollow camFollow = Object.FindFirstObjectByType<SmoothCameraFollow>();
        string camLabel = (camFollow != null && camFollow.CurrentMode == SmoothCameraFollow.CameraMode.FirstPerson) ? "3rd Cam [V]" : "1st Cam [V]";
        if (GUILayout.Button(camLabel, GUILayout.Height(30)))
        {
            if (camFollow != null) camFollow.ToggleCameraMode();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
}

