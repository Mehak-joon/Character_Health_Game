# ⚽ Topic: Character Health State Machine

A production-ready **Character Health State Machine** game built in **Unity 6 (URP)**, featuring an interactive 3D Low-Poly Soccer Stadium, procedural human locomotion animations, dynamic physics ball mechanics, switchable First-Person / Third-Person camera perspectives, synthesized sound effects, and real-time visual FX.

---

## 📋 Table of Contents
1. [Overview & Concept](#-overview--concept)
2. [Character Health State Machine](#-character-health-state-machine)
3. [Gameplay Mechanics & Rules](#-gameplay-mechanics--rules)
4. [Controls Reference](#-controls-reference)
5. [Camera Perspectives (1st Person & 3rd Person)](#-camera-perspectives)
6. [Human Locomotion & Animations](#-human-locomotion--animations)
7. [System Architecture & Scripts](#-system-architecture--scripts)
8. [Audio & Particle Systems](#-audio--particle-systems)
9. [How to Run & Play in Unity](#-how-to-run--play-in-unity)
10. [Developer Setup & Modification Guide](#-developer-setup--modification-guide)
    * [Environment & Prerequisites](#1-environment--prerequisites)
    * [Project Settings & Input Handling](#2-project-settings--input-handling)
    * [1-Click Scene Generation Workflow](#3-1-click-scene-generation-workflow)
    * [How to Replace with Custom 3D Models (FBX)](#4-how-to-replace-with-custom-3d-models-fbx)
    * [How to Add New Health States](#5-how-to-add-new-health-states)
    * [Tuning Physics, Speeds & Balance](#6-tuning-physics-speeds--balance)
    * [Adding Custom Traps & Obstacles](#7-adding-custom-traps--obstacles)
    * [Replacing Procedural SFX with Custom Audio](#8-replacing-procedural-sfx-with-custom-audio)
    * [Version Control & Git Guidelines](#9-version-control--git-guidelines)

---

## 🌟 Overview & Concept

This project implements **Topic: Character Health** from the State Machine design specifications:
* **Core Subject**: Real-time management and visual feedback of a character's health state.
* **States**: `Healthy` $\rightarrow$ `Injured` $\rightarrow$ `Critical` $\rightarrow$ `Defeated`.
* **Primary Key Controls**:
  * **`H`**: Reduce health level by one step.
  * **`R`**: Reset health back to full `Healthy` state.
* **Interactive Mini-Game**: Built into a responsive, low-poly soccer training arena where health directly influences running speed, kicking force, posture, and stamina. Mistakes (hitting cones or missing shots) penalize your health, creating an engaging, gamified feedback loop.

<img width="1602" height="713" alt="image" src="https://github.com/user-attachments/assets/75913c7e-85a7-406a-8e05-acd2a3679422" />
<img width="1592" height="664" alt="image" src="https://github.com/user-attachments/assets/11833c92-28e9-4e88-aa42-6738d75c2eaf" />

---

## 🩺 Character Health State Machine

The character transitions smoothly across 4 discrete health levels, with immediate visual, physical, and acoustic feedback:

| State | Health Level | Jersey Color | Speed Multiplier | Locomotion / Animation | Acoustic & Particle Feedback |
| :--- | :---: | :---: | :---: | :--- | :--- |
| **`HEALTHY`** | **3 / 3** (100%) | Vibrant Green | **100%** ($6.8\text{ m/s}$) | Upright athletic stance, energetic stride, dual-frequency step bounce. | Crisp hit impacts, full power kicks. |
| **`INJURED`** | **2 / 3** (66%) | Athletic Gold | **65%** ($4.4\text{ m/s}$) | Slouched posture, painful asymmetric limp, left arm clutching side. | Hurt flinch recoil, impact sparks. |
| **`CRITICAL`** | **1 / 3** (33%) | Warning Red | **32%** ($2.2\text{ m/s}$) | Stumbling gait, high-frequency trembling, heavy head droop. | Continuous dark smoke & electric sparks, repeating alarm chime. |
| **`DEFEATED`** | **0 / 3** (0%) | Charcoal Gray | **0%** ($0.0\text{ m/s}$) | Knockout collapse flat onto the turf. Movement & kicks disabled. | Descending power-down sweep, ground impact dust cloud. |

*Pressing **`R`** from any state triggers an acrobatic 360° spin-jump recovery, a rising golden healing aura, and an uplifting 4-note chime ($C_5 \rightarrow E_5 \rightarrow G_5 \rightarrow C_6$).*

---

## 🎯 Gameplay Mechanics & Rules

```
                      [   GOAL POST (7.0m x 2.8m)   ]
                      ===============================
                                   |
                             (Zone Z >= 13.8m)
                             /               \
            [Inside Posts: GOAL! 🎉]    [Wide: MISSED SHOT! -1 HP ⚠️]
                                   |
                      -------------------------------
                               ▲ Cone 4 (Z=11.0m)
                               ▲ Cone 3 (Z=8.8m)
                               ▲ Cone 2 (Z=6.5m)
                               ▲ Cone 1 (Z=4.2m)
                      -------------------------------
                                   |
                             [⚽ FOOTBALL]
                                   |
                         [🏃 SOCCER STRIKER]
```

### 1. 🥅 Goal Scoring & Miss Penalties
* **The Goal**: A wide $7.0\text{m} \times 2.8\text{m} \times 3.0\text{m}$ regulation net.
* **Score a Goal**: Drive the football across the goal line ($Z \ge 13.8\text{m}$) between the posts.
  * Triggers **`GOAL! 🎉`** banner.
  * Fires celebratory multi-colored confetti particles.
  * Plays fanfare chime and increments score $+1$.
  * Automatically respawns football at kickoff spot after $1.8\text{s}$.
* **Missed Shot Penalty**: Kicking the ball past the endline wide of the posts triggers **`MISSED SHOT! -1 HP ⚠️`**, sounds an error buzzer, applies **$-1\text{ HP}$ damage** to the striker, and resets the ball.

### 2. ⚠️ Slalom Training Cones
* 4 orange & white striped cones placed in a slalom course between the kickoff spot and the goal.
* **Hazard**: Colliding with any cone causes it to wobble, plays a plastic bonk sound, and inflicts **$-1\text{ HP}$ damage** to your character.

### 3. 🌤️ Open Sky & Out-of-Bounds Respawn
* The stadium roof is completely open to the sky.
* Chipping or blasting the football over the stadium boundary walls automatically triggers a referee whistle, displays **`OUT OF BOUNDS! 📢`**, and respawns the football at center pitch.

---

## ⌨️ Controls Reference

| Control | Action | Details |
| :--- | :--- | :--- |
| **`W, A, S, D`** | **Move Player** | Fluid, camera-relative movement (W moves forward in camera direction). |
| **`Arrow Keys`** | **Rotate Camera** | **Left / Right**: Rotate 360° horizontally. **Up / Down**: Tilt pitch up and down. |
| **`Left Shift` (Hold)** | **Sprint** | Sprint at $1.6\times$ speed with athletic forward lean and accelerated strides. |
| **`Left Shift` (Tap while moving)** | **Slide Tackle** | Drops low, slides with momentum and whoosh SFX. **Sliding into the ball delivers a Power Blast!** |
| **`V` or `C`** | **Toggle Camera Mode** | Switch seamlessly between **1st-Person View** and **3rd-Person View**. |
| **Right Mouse Button** | **Mouse Look (Optional)** | Hold RMB to orbit camera freely with mouse. |
| **Mouse Scroll Wheel** | **Zoom Camera** | Zoom 3rd-person camera distance in or out ($3.5\text{m}$ to $15\text{m}$). |
| **`H`** | **Take Damage** | Degrades health level by 1 step (`Healthy` $\rightarrow$ `Injured` $\rightarrow$ `Critical` $\rightarrow$ `Defeated`). |
| **`R`** | **Reset System** | Restores character to full health, triggers revive jump and healing aura. |

---

## 👁️ Camera Perspectives

Press **`V`** or **`C`** (or click the HUD button) to alternate perspectives:

1. **First-Person View (FPV)**:
   * Positioned directly at eye level ($Y = 1.68\text{m}$).
   * Puts you in the shoes of the striker for maximum immersion.
   * Tilting the camera down allows you to look at your legs and cleats dribbling the ball in real time.
2. **Third-Person View (TPV)**:
   * Smooth, over-the-shoulder stadium camera ($8.0\text{m}$ distance).
   * Fully controllable with Arrow Keys and Mouse.
   * Zero trailing lag or rubber-banding.

---

## 🏃 Human Locomotion & Animations

The striker utilizes procedural human biomechanics implemented in [`CharacterAnimator.cs`](Assets/Scripts/CharacterAnimator.cs):
* **Dual-Frequency Step Bounce**: Real humans bounce vertically twice per full stride cycle (dropping at heel strike, rising at mid-stance).
* **Lateral Hip Sway & Pelvic Rotation**: Natural weight shifting ($0.035\text{m}$) over each planting foot.
* **Shoulder Counter-Twist**: Upper torso and shoulders twist in counter-rotation to the pelvis ($-10^\circ$) to preserve angular momentum balance.
* **Knee Flexion**: Lower legs flex backwards dynamically on swing phases instead of pivoting as rigid poles.
* **Elbow Flexing & Arm Swing**: Arms pump in natural counter-motion to legs.
* **Dynamic Recoil**: Immediate flinch kickback on taking hit damage.
* **Acrobatic Revive Jump**: Full 360° rotational flip back to standing position on reset.

---

## 🏗️ System Architecture & Scripts

All scripts reside in `Assets/Scripts/` with clean separation of concerns:

```
Assets/Scripts/
├── CharacterHealthController.cs  # Core state machine (Healthy, Injured, Critical, Defeated)
├── RobotMovement.cs              # WASD locomotion, sprint, slide tackle, ball physics kick
├── CharacterAnimator.cs          # Procedural human biomechanics & health posture animations
├── SmoothCameraFollow.cs         # Arrow-key orbit, 1st/3rd person toggle, zero-lag damping
├── GoalLineManager.cs            # Fail-proof goal & miss detection, out-of-bounds reset, live HUD banner
├── ObstacleCone.cs               # Slalom obstacle physics, cone wobble, -1 HP collision damage
├── SoundEffectGenerator.cs       # 100% procedural audio synthesizer (math & AudioClip.Create)
├── CharacterParticleEffects.cs   # Programmatic particle systems (sparks, smoke, dust, revive aura)
├── HealthUI.cs                   # Interactive on-screen HUD (HP bar, state badge, controls, buttons)
└── Editor/
    └── CharacterHealthSceneSetup.cs # 1-click automated scene builder and configurator
```

---

## 🔊 Audio & Particle Systems

### Procedural Sound Synthesis (`SoundEffectGenerator.cs`)
All sound effects are generated mathematically at runtime via C# without any external `.wav` or `.mp3` files:
* **Hit SFX**: 808-style pitch drop punch ($340\text{Hz} \rightarrow 60\text{Hz}$) with attack noise click.
* **Critical Alarm SFX**: Urgent alternating two-tone alert chime ($880\text{Hz} \leftrightarrow 660\text{Hz}$).
* **Defeated SFX**: Descending power-down frequency sweep with distortion shutdown thud.
* **Revive SFX**: Uplifting 4-note ascending chime ($C_5 - E_5 - G_5 - C_6$).
* **Kick SFX**: Deep bass impact thud ($180\text{Hz} \rightarrow 45\text{Hz}$).
* **Goal Fanfare SFX**: Triumphant 5-note celebratory melody.
* **Miss Buzzer SFX**: Low $130\text{Hz}$ square-wave penalty buzzer.
* **Cone Bonk SFX**: High-resonance hollow plastic impact sound.
* **Referee Whistle SFX**: Dual-tone frequency-modulated pea whistle ($2400\text{Hz} / 2850\text{Hz}$).
* **Slide Whoosh SFX**: Turf friction noise burst.

### Programmatic Particle FX (`CharacterParticleEffects.cs`)
* **Hit Sparks Burst**: Radial explosive burst of yellow, orange, and red sparks on impact.
* **Critical Smoke Plume**: Continuous dark smoke cloud and electric sparks rising while in Critical state.
* **Defeat Impact Dust**: Radial ground shockwave dust puff upon collapsing.
* **Revive Healing Aura**: Rising ring of emerald and golden star sparkles.
* **Goal Confetti**: Multi-colored celebration confetti shower inside the goal.

---

## 🚀 How to Run & Play in Unity

1. Open the project in **Unity 6** (or compatible 2022+ / 6000+ versions with Universal Render Pipeline).
2. In the top Unity menu bar, click:
   $$\mathbf{Tools} \rightarrow \mathbf{Setup\ Character\ Health\ Scene}$$
3. Open the **Game** window and press the **Play (▶)** button.
4. **Play**:
   * Run with **`W, A, S, D`**.
   * Rotate camera with **`Arrow Keys`**.
   * Toggle 1st / 3rd Person with **`V`**.
   * Sprint & Slide Tackle with **`Left Shift`**.
   * Test health degradation with **`H`** and revive with **`R`**!

---

## 🛠️ Developer Setup & Modification Guide

This guide is designed for developers cloning, collaborating on, or modifying this project.

### 1. Environment & Prerequisites
* **Engine Version**: **Unity 6 (`6000.6.7f1`)** or Unity 2022.3+ LTS.
* **Render Pipeline**: **Universal Render Pipeline (URP v17+)**.
* **Essential Packages** (configured automatically in `Packages/manifest.json`):
  * `com.unity.inputsystem`: Version `1.20.1+`
  * `com.unity.ugui`: Version `2.6.0+` (includes TextMeshPro built-in)
  * `com.unity.render-pipelines.universal`: Version `17.6.0+`

### 2. Project Settings & Input Handling
1. Open the project in **Unity Hub** using Unity 6 (`6000.6.7f1`).
2. Go to **Edit** $\rightarrow$ **Project Settings** $\rightarrow$ **Player** $\rightarrow$ **Other Settings**.
3. Locate **Active Input Handling**:
   * Set to **`Input System Package (New)`** or **`Both`**.
   * All scripts in this project support both APIs simultaneously using `#if ENABLE_INPUT_SYSTEM` and `#if ENABLE_LEGACY_INPUT_MANAGER` conditional compilation.

### 3. 1-Click Scene Generation Workflow
The entire scene hierarchy is code-generated and deterministic:
1. Open `Assets/Scenes/SampleScene.unity` (or any empty scene).
2. Click **`Tools`** $\rightarrow$ **`Setup Character Health Scene`** from the Unity top menu bar.
3. The Editor script [`CharacterHealthSceneSetup.cs`](Assets/Scripts/Editor/CharacterHealthSceneSetup.cs) will automatically:
   * Instantiate and position the Main Camera with [`SmoothCameraFollow.cs`](Assets/Scripts/SmoothCameraFollow.cs).
   * Configure directional stadium sun lighting and shadows.
   * Generate the low-poly stadium pitch, lines, perimeter walls, and decorative pine trees.
   * Construct the goal post, net frame, and wire up [`GoalLineManager.cs`](Assets/Scripts/GoalLineManager.cs).
   * Spawn the obstacle training cones with [`ObstacleCone.cs`](Assets/Scripts/ObstacleCone.cs).
   * Instantiate the physics football with physics materials.
   * Assemble the soccer striker character with joint hierarchy, materials, audio, VFX, and controller scripts.
   * Instantiate the Canvas UI and HUD with [`HealthUI.cs`](Assets/Scripts/HealthUI.cs).

---

### 4. How to Replace with Custom 3D Models (FBX)
To swap out the procedural low-poly striker for an external FBX 3D character model:
1. Import your 3D character `.fbx` into `Assets/Models/`.
2. Drag the FBX prefab into the scene.
3. Add the following components to the character's root GameObject:
   * `CapsuleCollider` (set height to $\approx 1.9\text{m}$, center $Y \approx 0.95\text{m}$).
   * `Rigidbody` (Mass: $4.0$, **Freeze Rotation X, Y, Z**, Collision Detection: `Continuous`).
   * [`CharacterHealthController`](Assets/Scripts/CharacterHealthController.cs)
   * [`RobotMovement`](Assets/Scripts/RobotMovement.cs)
   * [`CharacterAnimator`](Assets/Scripts/CharacterAnimator.cs)
   * [`SoundEffectGenerator`](Assets/Scripts/SoundEffectGenerator.cs)
   * [`CharacterParticleEffects`](Assets/Scripts/CharacterParticleEffects.cs)
   * `AudioSource` (Play On Awake = `false`).
4. In [`CharacterHealthController`](Assets/Scripts/CharacterHealthController.cs) Inspector:
   * Assign **Character Renderer**: The main body mesh renderer (for health state color changes).
   * Assign **Character Body**: The character's root transform.
5. In [`CharacterAnimator`](Assets/Scripts/CharacterAnimator.cs) Inspector:
   * Map the bone transforms: `Head`, `Left Arm`, `Right Arm`, `Left Leg`, `Right Leg`.
   * Assign **Jersey Renderer** to the torso/shirt mesh.
6. In [`SmoothCameraFollow`](Assets/Scripts/SmoothCameraFollow.cs) on Main Camera:
   * Drag your new character GameObject into the **Target** field.

---

### 5. How to Add New Health States
To add a new state (e.g. `Shielded` or `Poisoned`):
1. In [`CharacterHealthController.cs`](Assets/Scripts/CharacterHealthController.cs):
   ```csharp
   public enum HealthState
   {
       Healthy,
       Injured,
       Critical,
       Defeated,
       Shielded // New state
   }
   ```
2. Add a corresponding color field:
   ```csharp
   [SerializeField] private Color shieldedColor = new Color(0.2f, 0.7f, 1.0f); // Bright Blue
   ```
3. Update `GetStateColor()` and `GetStateDescription()` switch statements:
   ```csharp
   case HealthState.Shielded: return shieldedColor;
   ```
4. In `HealthUI.cs`, the new state and badge color will automatically update on the HUD.

---

### 6. Tuning Physics, Speeds & Balance
Key inspector fields can be adjusted directly in the Unity Editor:

* **In [`RobotMovement.cs`](Assets/Scripts/RobotMovement.cs)**:
  * `Base Move Speed` (default `6.8`): Walking / running speed when Healthy.
  * `Sprint Speed Multiplier` (default `1.6`): Multiplier when holding `Left Shift`.
  * `Slide Boost Speed` (default `12.0`): Impulse speed during a slide tackle.
  * `Rotation Speed` (default `26.0`): Turn responsiveness when changing direction.
  * `Base Kick Force` (default `12.0`): Impulse applied to the ball on contact.
* **In [`GoalLineManager.cs`](Assets/Scripts/GoalLineManager.cs)**:
  * `Goal Line Z` (default `13.8`): Z coordinate where shots cross into the net.
  * `Goal Width` (default `7.0`): Distance between left and right posts ($-3.5\text{m} \leftrightarrow +3.5\text{m}$).
  * `Ball Kickoff Spot`: Vector3 position where the ball respawns after a goal, miss, or out-of-bounds.
* **In Football GameObject `SphereCollider`**:
  * Adjust `PhysMat_Football` (Bounciness: `0.72`, Friction: `0.4`) to modify ball bounce height and roll resistance.

---

### 7. Adding Custom Traps & Obstacles
To create custom hazards (e.g. mud patches, defending dummies, or moving obstacles):
1. Create your 3D obstacle GameObject in the scene.
2. Add a `Collider` (Box, Sphere, or Capsule).
3. Attach [`ObstacleCone.cs`](Assets/Scripts/ObstacleCone.cs) (or duplicate it for custom behavior):
   * Inspect [`ObstacleCone.cs`](Assets/Scripts/ObstacleCone.cs):
     ```csharp
     CharacterHealthController health = hitObj.GetComponentInParent<CharacterHealthController>();
     if (health != null)
     {
         health.TakeDamage(); // Inflicts -1 HP
     }
     ```
4. Adjust `damageCooldown` in the Inspector to control invulnerability frames between hits.

---

### 8. Replacing Procedural SFX with Custom Audio
If you wish to use imported `.wav` or `.mp3` audio clips instead of procedural synthesis:
1. Import your audio files into `Assets/Audio/`.
2. In [`SoundEffectGenerator.cs`](Assets/Scripts/SoundEffectGenerator.cs), add serialized fields:
   ```csharp
   [SerializeField] private AudioClip customGoalClip;
   [SerializeField] private AudioClip customHitClip;
   ```
3. In `PlayGoalFanfare()`, check if `customGoalClip != null` before falling back to procedural generation:
   ```csharp
   if (customGoalClip != null)
   {
       audioSource.PlayOneShot(customGoalClip);
       return;
   }
   ```

---

### 9. Version Control & Git Guidelines
* **Ignored Folders**: Ensure your `.gitignore` includes:
  ```
  /[Ll]ibrary/
  /[Tt]emp/
  /[Oo]bj/
  /[Bb]uild/
  /[Bb]uilds/
  /[Uu]ser[Ss]ettings/
  /[Mm]emoryCaptures/
  ```
* **Critical**: Never delete or ignore `.meta` files. Every asset, script, and folder in Unity requires its paired `.meta` file to maintain GUID references across scenes and components.

---
*Maintained for collaborative game production.*

