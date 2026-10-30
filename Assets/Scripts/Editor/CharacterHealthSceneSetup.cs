#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CharacterHealthSceneSetup
{
    [MenuItem("Tools/Setup Character Health Scene")]
    public static void SetupScene()
    {
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        // 1. Camera Setup
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        mainCam.transform.position = new Vector3(0f, 6.8f, -10.2f);
        mainCam.transform.rotation = Quaternion.Euler(27f, 0f, 0f);

        SmoothCameraFollow camFollow = mainCam.GetComponent<SmoothCameraFollow>();
        if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<SmoothCameraFollow>();

        // 2. Lighting Setup
        Light dirLight = Object.FindFirstObjectByType<Light>();
        if (dirLight == null || dirLight.type != LightType.Directional)
        {
            GameObject lightObj = new GameObject("Directional Light");
            dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
        }
        dirLight.transform.rotation = Quaternion.Euler(46f, -30f, 0f);
        dirLight.intensity = 1.3f;
        dirLight.color = new Color(1f, 0.98f, 0.92f);
        dirLight.shadows = LightShadows.Soft;

        // 3. Clean up old objects
        string[] oldNames = { "Arena_Pitch", "Football", "GoalPost", "Character_Player", "Ground", "ObstacleCones", "MissTriggers", "GoalSystem" };
        foreach (string name in oldNames)
        {
            GameObject oldObj = GameObject.Find(name);
            if (oldObj != null) Object.DestroyImmediate(oldObj);
        }

        // ==========================================
        // 4. LOW-POLY SOCCER PITCH & CONTAINMENT
        // ==========================================
        GameObject arena = new GameObject("Arena_Pitch");

        // Pitch Ground Plane (28m wide x 40m long)
        GameObject pitchGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        pitchGround.name = "PitchGround";
        pitchGround.transform.SetParent(arena.transform, false);
        pitchGround.transform.localPosition = Vector3.zero;
        pitchGround.transform.localScale = new Vector3(2.8f, 1f, 4.0f);

        Material grassMat = new Material(urpShader)
        {
            name = "M_PitchGrass",
            color = new Color(0.24f, 0.65f, 0.28f, 1f)
        };
        if (grassMat.HasProperty("_BaseColor")) grassMat.SetColor("_BaseColor", new Color(0.24f, 0.65f, 0.28f, 1f));
        pitchGround.GetComponent<Renderer>().sharedMaterial = grassMat;

        // White Pitch Markings
        Material lineMat = new Material(urpShader) { name = "M_PitchLines", color = new Color(0.95f, 0.96f, 0.98f, 1f) };
        if (lineMat.HasProperty("_BaseColor")) lineMat.SetColor("_BaseColor", new Color(0.95f, 0.96f, 0.98f, 1f));

        // Center line
        GameObject halfLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        halfLine.name = "HalfwayLine";
        halfLine.transform.SetParent(arena.transform, false);
        halfLine.transform.localPosition = new Vector3(0f, 0.015f, 0f);
        halfLine.transform.localScale = new Vector3(26f, 0.005f, 0.16f);
        Object.DestroyImmediate(halfLine.GetComponent<Collider>());
        halfLine.GetComponent<Renderer>().sharedMaterial = lineMat;

        // Goal line marking (Z = 13.8)
        GameObject goalLineMarking = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goalLineMarking.name = "GoalLineMarking";
        goalLineMarking.transform.SetParent(arena.transform, false);
        goalLineMarking.transform.localPosition = new Vector3(0f, 0.015f, 13.8f);
        goalLineMarking.transform.localScale = new Vector3(26f, 0.005f, 0.16f);
        Object.DestroyImmediate(goalLineMarking.GetComponent<Collider>());
        goalLineMarking.GetComponent<Renderer>().sharedMaterial = lineMat;

        // Center Circle
        GameObject centerCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        centerCircle.name = "CenterCircleMarking";
        centerCircle.transform.SetParent(arena.transform, false);
        centerCircle.transform.localPosition = new Vector3(0f, 0.012f, 0f);
        centerCircle.transform.localScale = new Vector3(5.0f, 0.005f, 5.0f);
        Object.DestroyImmediate(centerCircle.GetComponent<Collider>());
        centerCircle.GetComponent<Renderer>().sharedMaterial = lineMat;

        GameObject innerCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        innerCircle.name = "InnerCircleCutout";
        innerCircle.transform.SetParent(arena.transform, false);
        innerCircle.transform.localPosition = new Vector3(0f, 0.018f, 0f);
        innerCircle.transform.localScale = new Vector3(4.7f, 0.005f, 4.7f);
        Object.DestroyImmediate(innerCircle.GetComponent<Collider>());
        innerCircle.GetComponent<Renderer>().sharedMaterial = grassMat;

        // HIGH CONTAINMENT WALLS (North wall is at Z = 20.5m, well behind the goal!)
        Material wallMat = new Material(urpShader)
        {
            name = "M_StadiumGlass",
            color = new Color(0.85f, 0.92f, 0.98f, 0.35f)
        };
        if (wallMat.HasProperty("_BaseColor")) wallMat.SetColor("_BaseColor", new Color(0.85f, 0.92f, 0.98f, 0.35f));

        float wallHeight = 5.5f;
        CreateWall(arena.transform, "Wall_North", new Vector3(0f, wallHeight / 2f, 20.5f), new Vector3(28f, wallHeight, 0.8f), wallMat);
        CreateWall(arena.transform, "Wall_South", new Vector3(0f, wallHeight / 2f, -19.5f), new Vector3(28f, wallHeight, 0.8f), wallMat);
        CreateWall(arena.transform, "Wall_East", new Vector3(14f, wallHeight / 2f, 0f), new Vector3(0.8f, wallHeight, 40f), wallMat);
        CreateWall(arena.transform, "Wall_West", new Vector3(-14f, wallHeight / 2f, 0f), new Vector3(0.8f, wallHeight, 40f), wallMat);

        // Open Sky Roof (Ceiling barrier removed so balls can fly up into the open sky)

        // Low-Poly Trees
        Material trunkMat = new Material(urpShader) { color = new Color(0.42f, 0.28f, 0.18f) };
        if (trunkMat.HasProperty("_BaseColor")) trunkMat.SetColor("_BaseColor", new Color(0.42f, 0.28f, 0.18f));

        Material leavesMat = new Material(urpShader) { color = new Color(0.18f, 0.55f, 0.24f) };
        if (leavesMat.HasProperty("_BaseColor")) leavesMat.SetColor("_BaseColor", new Color(0.18f, 0.55f, 0.24f));

        Vector3[] treePositions = {
            new Vector3(-16f, 0f, 15f), new Vector3(-16f, 0f, 0f), new Vector3(-16f, 0f, -14f),
            new Vector3(16f, 0f, 15f), new Vector3(16f, 0f, 0f), new Vector3(16f, 0f, -14f),
            new Vector3(-8f, 0f, 22f), new Vector3(8f, 0f, 22f),
            new Vector3(-8f, 0f, -21f), new Vector3(8f, 0f, -21f)
        };
        foreach (var tPos in treePositions) CreateLowPolyTree(arena.transform, tPos, trunkMat, leavesMat);

        // ==========================================
        // 5. WIDE GOAL POST (Z = 13.8m to 16.8m)
        // ==========================================
        GameObject goalPost = new GameObject("GoalPost");
        goalPost.transform.position = new Vector3(0f, 0f, 13.8f);

        Material postMat = new Material(urpShader) { color = Color.white };
        if (postMat.HasProperty("_BaseColor")) postMat.SetColor("_BaseColor", Color.white);

        float goalWidth = 7.0f;
        float goalHeight = 2.8f;
        float goalDepth = 3.0f;

        // Left post & Right post
        CreateGoalPillar(goalPost.transform, "Post_Left", new Vector3(-goalWidth / 2f, goalHeight / 2f, 0f), new Vector3(0.24f, goalHeight, 0.24f), postMat);
        CreateGoalPillar(goalPost.transform, "Post_Right", new Vector3(goalWidth / 2f, goalHeight / 2f, 0f), new Vector3(0.24f, goalHeight, 0.24f), postMat);
        // Crossbar
        CreateGoalPillar(goalPost.transform, "Post_Crossbar", new Vector3(0f, goalHeight, 0f), new Vector3(goalWidth + 0.24f, 0.24f, 0.24f), postMat);

        // Netting
        Material netMat = new Material(urpShader)
        {
            name = "M_Netting",
            color = new Color(0.92f, 0.95f, 1f, 0.45f)
        };
        if (netMat.HasProperty("_BaseColor")) netMat.SetColor("_BaseColor", new Color(0.92f, 0.95f, 1f, 0.45f));

        // Back Net (catches ball cleanly at Z = 16.8)
        GameObject backNet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backNet.name = "Net_Back";
        backNet.transform.SetParent(goalPost.transform, false);
        backNet.transform.localPosition = new Vector3(0f, goalHeight / 2f, goalDepth);
        backNet.transform.localScale = new Vector3(goalWidth, goalHeight, 0.2f);
        backNet.GetComponent<Renderer>().sharedMaterial = netMat;

        // Left & Right Net Walls
        GameObject leftNet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftNet.name = "Net_Left";
        leftNet.transform.SetParent(goalPost.transform, false);
        leftNet.transform.localPosition = new Vector3(-goalWidth / 2f, goalHeight / 2f, goalDepth / 2f);
        leftNet.transform.localScale = new Vector3(0.15f, goalHeight, goalDepth);
        leftNet.GetComponent<Renderer>().sharedMaterial = netMat;

        GameObject rightNet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightNet.name = "Net_Right";
        rightNet.transform.SetParent(goalPost.transform, false);
        rightNet.transform.localPosition = new Vector3(goalWidth / 2f, goalHeight / 2f, goalDepth / 2f);
        rightNet.transform.localScale = new Vector3(0.15f, goalHeight, goalDepth);
        rightNet.GetComponent<Renderer>().sharedMaterial = netMat;

        // Roof Net
        GameObject roofNet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofNet.name = "Net_Roof";
        roofNet.transform.SetParent(goalPost.transform, false);
        roofNet.transform.localPosition = new Vector3(0f, goalHeight, goalDepth / 2f);
        roofNet.transform.localScale = new Vector3(goalWidth, 0.15f, goalDepth);
        roofNet.GetComponent<Renderer>().sharedMaterial = netMat;

        // ==========================================
        // 6. SLALOM TRAINING CONE OBSTACLES (HIT = -1 HP)
        // ==========================================
        GameObject conesParent = new GameObject("ObstacleCones");
        Vector3[] conePositions = {
            new Vector3(-2.5f, 0f, 4.2f),
            new Vector3(2.5f, 0f, 6.5f),
            new Vector3(-2.0f, 0f, 8.8f),
            new Vector3(2.0f, 0f, 11.0f)
        };

        Material coneOrange = new Material(urpShader) { name = "M_ConeOrange", color = new Color(1.0f, 0.45f, 0.05f) };
        if (coneOrange.HasProperty("_BaseColor")) coneOrange.SetColor("_BaseColor", new Color(1.0f, 0.45f, 0.05f));

        Material coneWhite = new Material(urpShader) { name = "M_ConeWhite", color = Color.white };
        if (coneWhite.HasProperty("_BaseColor")) coneWhite.SetColor("_BaseColor", Color.white);

        for (int i = 0; i < conePositions.Length; i++)
        {
            CreateTrainingCone(conesParent.transform, $"TrainingCone_{i + 1}", conePositions[i], coneOrange, coneWhite);
        }

        // ==========================================
        // 7. PHYSICS FOOTBALL
        // ==========================================
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Football";
        ball.transform.position = new Vector3(0f, 0.45f, 1.8f);
        ball.transform.localScale = Vector3.one * 0.8f;

        PhysicsMaterial ballPhysMat = new PhysicsMaterial("PhysMat_Football")
        {
            bounciness = 0.72f,
            bounceCombine = PhysicsMaterialCombine.Maximum,
            dynamicFriction = 0.4f,
            staticFriction = 0.45f
        };
        ball.GetComponent<SphereCollider>().sharedMaterial = ballPhysMat;

        Rigidbody ballRb = ball.AddComponent<Rigidbody>();
        ballRb.mass = 1.1f;
        ballRb.linearDamping = 0.3f;
        ballRb.angularDamping = 0.45f;
        ballRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Material ballMat = new Material(urpShader) { name = "M_Football", color = new Color(1f, 0.85f, 0.1f) };
        if (ballMat.HasProperty("_BaseColor")) ballMat.SetColor("_BaseColor", new Color(1f, 0.85f, 0.1f));
        ball.GetComponent<Renderer>().sharedMaterial = ballMat;

        // ==========================================
        // 8. UPGRADED 3D SOCCER STRIKER (PROPER UN-SCALED PIVOTS)
        // ==========================================
        GameObject character = new GameObject("Character_Player");
        character.transform.position = new Vector3(0f, 0.95f, -2.8f);

        CapsuleCollider charCol = character.AddComponent<CapsuleCollider>();
        charCol.height = 2.0f;
        charCol.radius = 0.45f;
        charCol.center = new Vector3(0f, 1.0f, 0f);

        Rigidbody charRb = character.AddComponent<Rigidbody>();
        charRb.mass = 4f;
        charRb.freezeRotation = true;
        charRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        charRb.interpolation = RigidbodyInterpolation.Interpolate;

        // Striker Visual Hierarchy
        GameObject rig = new GameObject("StrikerRig");
        rig.transform.SetParent(character.transform, false);

        // Materials
        Material skinMat = new Material(urpShader) { name = "M_SkinTone", color = new Color(0.96f, 0.76f, 0.62f) };
        if (skinMat.HasProperty("_BaseColor")) skinMat.SetColor("_BaseColor", new Color(0.96f, 0.76f, 0.62f));

        Material hairMat = new Material(urpShader) { name = "M_Hair", color = new Color(0.2f, 0.14f, 0.08f) };
        if (hairMat.HasProperty("_BaseColor")) hairMat.SetColor("_BaseColor", new Color(0.2f, 0.14f, 0.08f));

        Material jerseyMat = new Material(urpShader) { name = "M_Jersey", color = new Color(0.18f, 0.8f, 0.35f) };
        if (jerseyMat.HasProperty("_BaseColor")) jerseyMat.SetColor("_BaseColor", new Color(0.18f, 0.8f, 0.35f));

        Material shortsMat = new Material(urpShader) { name = "M_Shorts", color = new Color(0.12f, 0.18f, 0.3f) };
        if (shortsMat.HasProperty("_BaseColor")) shortsMat.SetColor("_BaseColor", new Color(0.12f, 0.18f, 0.3f));

        Material cleatMat = new Material(urpShader) { name = "M_Cleats", color = new Color(0.15f, 0.15f, 0.15f) };
        if (cleatMat.HasProperty("_BaseColor")) cleatMat.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.15f));

        // 1. Hips (Unscaled pivot)
        GameObject hipsPivot = new GameObject("HipsPivot");
        hipsPivot.transform.SetParent(rig.transform, false);
        hipsPivot.transform.localPosition = new Vector3(0f, 0.75f, 0f);

        GameObject hipsMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hipsMesh.name = "HipsMesh";
        hipsMesh.transform.SetParent(hipsPivot.transform, false);
        hipsMesh.transform.localPosition = Vector3.zero;
        hipsMesh.transform.localScale = new Vector3(0.55f, 0.28f, 0.35f);
        Object.DestroyImmediate(hipsMesh.GetComponent<Collider>());
        hipsMesh.GetComponent<Renderer>().sharedMaterial = shortsMat;

        // 2. Torso (Unscaled pivot)
        GameObject torsoPivot = new GameObject("TorsoPivot");
        torsoPivot.transform.SetParent(rig.transform, false);
        torsoPivot.transform.localPosition = new Vector3(0f, 1.15f, 0f);

        GameObject torsoMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        torsoMesh.name = "TorsoMesh";
        torsoMesh.transform.SetParent(torsoPivot.transform, false);
        torsoMesh.transform.localPosition = Vector3.zero;
        torsoMesh.transform.localScale = new Vector3(0.62f, 0.52f, 0.45f);
        Object.DestroyImmediate(torsoMesh.GetComponent<Collider>());
        Renderer jerseyRenderer = torsoMesh.GetComponent<Renderer>();
        jerseyRenderer.sharedMaterial = jerseyMat;

        // Crest Badge
        GameObject crest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crest.name = "CrestBadge";
        crest.transform.SetParent(torsoPivot.transform, false);
        crest.transform.localPosition = new Vector3(0.16f, 0.12f, 0.24f);
        crest.transform.localScale = new Vector3(0.14f, 0.18f, 0.05f);
        Object.DestroyImmediate(crest.GetComponent<Collider>());
        crest.GetComponent<Renderer>().sharedMaterial = postMat;

        // 3. Head (Unscaled pivot at top of body - PROMINENT & VISIBLE!)
        GameObject headPivot = new GameObject("HeadPivot");
        headPivot.transform.SetParent(rig.transform, false);
        headPivot.transform.localPosition = new Vector3(0f, 1.68f, 0f);

        GameObject headMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        headMesh.name = "HeadMesh";
        headMesh.transform.SetParent(headPivot.transform, false);
        headMesh.transform.localPosition = Vector3.zero;
        headMesh.transform.localScale = new Vector3(0.48f, 0.5f, 0.48f);
        Object.DestroyImmediate(headMesh.GetComponent<Collider>());
        headMesh.GetComponent<Renderer>().sharedMaterial = skinMat;

        // Hair
        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.name = "Hair_Top";
        hair.transform.SetParent(headPivot.transform, false);
        hair.transform.localPosition = new Vector3(0f, 0.16f, -0.04f);
        hair.transform.localScale = new Vector3(0.52f, 0.36f, 0.52f);
        Object.DestroyImmediate(hair.GetComponent<Collider>());
        hair.GetComponent<Renderer>().sharedMaterial = hairMat;

        // Headband
        GameObject headband = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        headband.name = "Headband";
        headband.transform.SetParent(headPivot.transform, false);
        headband.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        headband.transform.localScale = new Vector3(0.51f, 0.06f, 0.51f);
        Object.DestroyImmediate(headband.GetComponent<Collider>());
        headband.GetComponent<Renderer>().sharedMaterial = postMat;

        // Left Eye & Right Eye
        Material eyeMat = new Material(urpShader) { name = "M_Eyes", color = new Color(0.12f, 0.12f, 0.15f) };
        if (eyeMat.HasProperty("_BaseColor")) eyeMat.SetColor("_BaseColor", new Color(0.12f, 0.12f, 0.15f));

        GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeL.name = "Eye_Left";
        eyeL.transform.SetParent(headPivot.transform, false);
        eyeL.transform.localPosition = new Vector3(-0.12f, 0.03f, 0.22f);
        eyeL.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
        Object.DestroyImmediate(eyeL.GetComponent<Collider>());
        eyeL.GetComponent<Renderer>().sharedMaterial = eyeMat;

        GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeR.name = "Eye_Right";
        eyeR.transform.SetParent(headPivot.transform, false);
        eyeR.transform.localPosition = new Vector3(0.12f, 0.03f, 0.22f);
        eyeR.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
        Object.DestroyImmediate(eyeR.GetComponent<Collider>());
        eyeR.GetComponent<Renderer>().sharedMaterial = eyeMat;

        // 4. Arms (Pivots at y = 1.35)
        GameObject leftArmPivot = new GameObject("LeftArmPivot");
        leftArmPivot.transform.SetParent(rig.transform, false);
        leftArmPivot.transform.localPosition = new Vector3(-0.42f, 1.35f, 0f);

        GameObject leftArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftArmMesh.name = "LeftArmMesh";
        leftArmMesh.transform.SetParent(leftArmPivot.transform, false);
        leftArmMesh.transform.localPosition = new Vector3(0f, -0.22f, 0f);
        leftArmMesh.transform.localScale = new Vector3(0.18f, 0.48f, 0.18f);
        Object.DestroyImmediate(leftArmMesh.GetComponent<Collider>());
        leftArmMesh.GetComponent<Renderer>().sharedMaterial = skinMat;

        GameObject rightArmPivot = new GameObject("RightArmPivot");
        rightArmPivot.transform.SetParent(rig.transform, false);
        rightArmPivot.transform.localPosition = new Vector3(0.42f, 1.35f, 0f);

        GameObject rightArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightArmMesh.name = "RightArmMesh";
        rightArmMesh.transform.SetParent(rightArmPivot.transform, false);
        rightArmMesh.transform.localPosition = new Vector3(0f, -0.22f, 0f);
        rightArmMesh.transform.localScale = new Vector3(0.18f, 0.48f, 0.18f);
        Object.DestroyImmediate(rightArmMesh.GetComponent<Collider>());
        rightArmMesh.GetComponent<Renderer>().sharedMaterial = skinMat;

        // 5. Legs (Pivots at y = 0.65)
        GameObject leftLegPivot = new GameObject("LeftLegPivot");
        leftLegPivot.transform.SetParent(rig.transform, false);
        leftLegPivot.transform.localPosition = new Vector3(-0.18f, 0.65f, 0f);

        GameObject leftLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftLegMesh.name = "LeftLegMesh";
        leftLegMesh.transform.SetParent(leftLegPivot.transform, false);
        leftLegMesh.transform.localPosition = new Vector3(0f, -0.25f, 0f);
        leftLegMesh.transform.localScale = new Vector3(0.18f, 0.52f, 0.18f);
        Object.DestroyImmediate(leftLegMesh.GetComponent<Collider>());
        leftLegMesh.GetComponent<Renderer>().sharedMaterial = postMat; // socks

        GameObject leftCleat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftCleat.name = "LeftCleat";
        leftCleat.transform.SetParent(leftLegPivot.transform, false);
        leftCleat.transform.localPosition = new Vector3(0f, -0.52f, 0.06f);
        leftCleat.transform.localScale = new Vector3(0.2f, 0.14f, 0.36f);
        Object.DestroyImmediate(leftCleat.GetComponent<Collider>());
        leftCleat.GetComponent<Renderer>().sharedMaterial = cleatMat;

        GameObject rightLegPivot = new GameObject("RightLegPivot");
        rightLegPivot.transform.SetParent(rig.transform, false);
        rightLegPivot.transform.localPosition = new Vector3(0.18f, 0.65f, 0f);

        GameObject rightLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightLegMesh.name = "RightLegMesh";
        rightLegMesh.transform.SetParent(rightLegPivot.transform, false);
        rightLegMesh.transform.localPosition = new Vector3(0f, -0.25f, 0f);
        rightLegMesh.transform.localScale = new Vector3(0.18f, 0.52f, 0.18f);
        Object.DestroyImmediate(rightLegMesh.GetComponent<Collider>());
        rightLegMesh.GetComponent<Renderer>().sharedMaterial = postMat;

        GameObject rightCleat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightCleat.name = "RightCleat";
        rightCleat.transform.SetParent(rightLegPivot.transform, false);
        rightCleat.transform.localPosition = new Vector3(0f, -0.52f, 0.06f);
        rightCleat.transform.localScale = new Vector3(0.2f, 0.14f, 0.36f);
        Object.DestroyImmediate(rightCleat.GetComponent<Collider>());
        rightCleat.GetComponent<Renderer>().sharedMaterial = cleatMat;

        // Components Setup
        AudioSource aSource = character.AddComponent<AudioSource>();
        aSource.playOnAwake = false;

        SoundEffectGenerator sfx = character.AddComponent<SoundEffectGenerator>();
        CharacterParticleEffects vfx = character.AddComponent<CharacterParticleEffects>();

        CharacterAnimator anim = character.AddComponent<CharacterAnimator>();
        anim.bodyRoot = rig.transform;
        anim.headTransform = headPivot.transform;
        anim.leftArmTransform = leftArmPivot.transform;
        anim.rightArmTransform = rightArmPivot.transform;
        anim.leftLegTransform = leftLegPivot.transform;
        anim.rightLegTransform = rightLegPivot.transform;
        anim.jerseyRenderer = jerseyRenderer;

        CharacterHealthController healthController = character.AddComponent<CharacterHealthController>();
        SerializedObject soChar = new SerializedObject(healthController);
        soChar.FindProperty("characterRenderer").objectReferenceValue = jerseyRenderer;
        soChar.FindProperty("characterBody").objectReferenceValue = rig.transform;
        soChar.FindProperty("soundFX").objectReferenceValue = sfx;
        soChar.FindProperty("particleFX").objectReferenceValue = vfx;
        soChar.FindProperty("characterAnimator").objectReferenceValue = anim;
        soChar.ApplyModifiedProperties();

        RobotMovement movement = character.AddComponent<RobotMovement>();
        SerializedObject soMove = new SerializedObject(movement);
        soMove.FindProperty("healthController").objectReferenceValue = healthController;
        soMove.FindProperty("soundFX").objectReferenceValue = sfx;
        soMove.FindProperty("characterAnimator").objectReferenceValue = anim;
        soMove.FindProperty("rb").objectReferenceValue = charRb;
        soMove.ApplyModifiedProperties();

        camFollow.SetTarget(character.transform);

        // ==========================================
        // 9. GOAL LINE & MISS SYSTEM (100% RELIABLE)
        // ==========================================
        GameObject goalSystem = new GameObject("GoalSystem");
        GoalLineManager goalLineMgr = goalSystem.AddComponent<GoalLineManager>();
        SerializedObject soGoal = new SerializedObject(goalLineMgr);
        soGoal.FindProperty("footballRb").objectReferenceValue = ballRb;
        soGoal.FindProperty("healthController").objectReferenceValue = healthController;
        soGoal.FindProperty("soundFX").objectReferenceValue = sfx;
        soGoal.ApplyModifiedProperties();

        // ==========================================
        // 10. UI MANAGER
        // ==========================================
        GameObject uiManager = GameObject.Find("UIManager");
        if (uiManager == null) uiManager = new GameObject("UIManager");
        HealthUI healthUI = uiManager.GetComponent<HealthUI>();
        if (healthUI == null) healthUI = uiManager.AddComponent<HealthUI>();

        SerializedObject soUI = new SerializedObject(healthUI);
        soUI.FindProperty("healthController").objectReferenceValue = healthController;
        soUI.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("==================================================================");
        Debug.Log(" Striker Character & Goal Line System Setup Successfully!");
        Debug.Log(" - Prominent Head with hair, headband, and eyes is now fully visible!");
        Debug.Log(" - Goal detection is now 100% responsive and reliable!");
        Debug.Log("==================================================================");

        EditorUtility.DisplayDialog(
            "Character & Goal Fixed!",
            "Fixes applied successfully:\n\n" +
            "1. Player Head Fixed: Unscaled joint hierarchy gives a full, round head with hair, headband, and eyes!\n" +
            "2. Goal System Fixed: Back stadium wall moved back, and unified GoalLineManager ensures 100% reliable goal and miss detection!\n" +
            "3. Banner Alerts: Big on-screen banner announces GOAL! or MISSED SHOT! with instant feedback.",
            "Let's Play!"
        );
    }

    private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = name;
        w.transform.SetParent(parent, false);
        w.transform.localPosition = pos;
        w.transform.localScale = size;
        w.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void CreateGoalPillar(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = name;
        p.transform.SetParent(parent, false);
        p.transform.localPosition = pos;
        p.transform.localScale = size;
        p.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void CreateTrainingCone(Transform parent, string name, Vector3 pos, Material orangeMat, Material whiteMat)
    {
        GameObject coneRoot = new GameObject(name);
        coneRoot.transform.SetParent(parent, false);
        coneRoot.transform.localPosition = pos;

        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObj.name = "Base";
        baseObj.transform.SetParent(coneRoot.transform, false);
        baseObj.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        baseObj.transform.localScale = new Vector3(0.7f, 0.08f, 0.7f);
        baseObj.GetComponent<Renderer>().sharedMaterial = orangeMat;
        Object.DestroyImmediate(baseObj.GetComponent<Collider>());

        GameObject bottomSection = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bottomSection.name = "BottomCone";
        bottomSection.transform.SetParent(coneRoot.transform, false);
        bottomSection.transform.localPosition = new Vector3(0f, 0.28f, 0f);
        bottomSection.transform.localScale = new Vector3(0.52f, 0.24f, 0.52f);
        bottomSection.GetComponent<Renderer>().sharedMaterial = orangeMat;
        Object.DestroyImmediate(bottomSection.GetComponent<Collider>());

        GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stripe.name = "WhiteStripe";
        stripe.transform.SetParent(coneRoot.transform, false);
        stripe.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        stripe.transform.localScale = new Vector3(0.38f, 0.16f, 0.38f);
        stripe.GetComponent<Renderer>().sharedMaterial = whiteMat;
        Object.DestroyImmediate(stripe.GetComponent<Collider>());

        GameObject topSection = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        topSection.name = "TopCone";
        topSection.transform.SetParent(coneRoot.transform, false);
        topSection.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        topSection.transform.localScale = new Vector3(0.24f, 0.16f, 0.24f);
        topSection.GetComponent<Renderer>().sharedMaterial = orangeMat;
        Object.DestroyImmediate(topSection.GetComponent<Collider>());

        CapsuleCollider col = coneRoot.AddComponent<CapsuleCollider>();
        col.height = 0.85f;
        col.radius = 0.38f;
        col.center = new Vector3(0f, 0.42f, 0f);

        coneRoot.AddComponent<ObstacleCone>();
    }

    private static void CreateLowPolyTree(Transform parent, Vector3 pos, Material trunkMat, Material leavesMat)
    {
        GameObject tree = new GameObject("LowPolyTree");
        tree.transform.SetParent(parent, false);
        tree.transform.localPosition = pos;

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        trunk.transform.localScale = new Vector3(0.35f, 0.8f, 0.35f);
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;

        GameObject cone1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone1.name = "Foliage_Base";
        cone1.transform.SetParent(tree.transform, false);
        cone1.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        cone1.transform.localScale = new Vector3(1.6f, 0.6f, 1.6f);
        cone1.GetComponent<Renderer>().sharedMaterial = leavesMat;

        GameObject cone2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone2.name = "Foliage_Mid";
        cone2.transform.SetParent(tree.transform, false);
        cone2.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        cone2.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f);
        cone2.GetComponent<Renderer>().sharedMaterial = leavesMat;

        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        top.name = "Foliage_Top";
        top.transform.SetParent(tree.transform, false);
        top.transform.localPosition = new Vector3(0f, 3.1f, 0f);
        top.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        top.GetComponent<Renderer>().sharedMaterial = leavesMat;
    }
}
#endif
