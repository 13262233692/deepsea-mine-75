using UnityEngine;

public class MarineLifeSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int fishCount = 20;
    public int coralCount = 15;
    public int shrimpCount = 10;
    public int jellyfishCount = 8;
    public int spongeCount = 5;
    public int seaCucumberCount = 12;
    public int tubeWormCount = 10;
    public int seaAnemoneCount = 6;
    public int starfishCount = 8;
    public int deepSeaCrabCount = 5;

    [Header("Spawn Area")]
    public Vector3 spawnArea = new Vector3(100f, 30f, 100f);
    public float minHeight = 5f;
    public float maxHeight = 30f;

    [Header("References")]
    public SeabedTerrainGenerator terrainGenerator;

    private Transform lifeParent;

    private void Start()
    {
        lifeParent = new GameObject("Marine Life").transform;
        lifeParent.parent = transform;

        SpawnAllLife();
    }

    private void SpawnAllLife()
    {
        for (int i = 0; i < fishCount; i++)
            SpawnLife(MarineLife.LifeType.Fish);

        for (int i = 0; i < coralCount; i++)
            SpawnLife(MarineLife.LifeType.Coral);

        for (int i = 0; i < shrimpCount; i++)
            SpawnLife(MarineLife.LifeType.Shrimp);

        for (int i = 0; i < jellyfishCount; i++)
            SpawnLife(MarineLife.LifeType.Jellyfish);

        for (int i = 0; i < spongeCount; i++)
            SpawnLife(MarineLife.LifeType.Sponge);

        for (int i = 0; i < seaCucumberCount; i++)
            SpawnLife(MarineLife.LifeType.SeaCucumber);

        for (int i = 0; i < tubeWormCount; i++)
            SpawnLife(MarineLife.LifeType.TubeWorm);

        for (int i = 0; i < seaAnemoneCount; i++)
            SpawnLife(MarineLife.LifeType.SeaAnemone);

        for (int i = 0; i < starfishCount; i++)
            SpawnLife(MarineLife.LifeType.Starfish);

        for (int i = 0; i < deepSeaCrabCount; i++)
            SpawnLife(MarineLife.LifeType.DeepSeaCrab);
    }

    private void SpawnLife(MarineLife.LifeType type)
    {
        Vector3 spawnPos = GetRandomSpawnPosition(type);
        GameObject lifeObject = CreateLifeObject(type, spawnPos);
        lifeObject.transform.parent = lifeParent;
    }

    private Vector3 GetRandomSpawnPosition(MarineLife.LifeType type)
    {
        float x = Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f);
        float z = Random.Range(-spawnArea.z / 2f, spawnArea.z / 2f);
        float y = 0f;

        if (terrainGenerator != null)
        {
            y = terrainGenerator.GetTerrainHeight(x, z);
        }

        switch (type)
        {
            case MarineLife.LifeType.Fish:
            case MarineLife.LifeType.Jellyfish:
                y += Random.Range(minHeight, maxHeight);
                break;
            case MarineLife.LifeType.Coral:
            case MarineLife.LifeType.Sponge:
            case MarineLife.LifeType.SeaAnemone:
                y += 1f;
                break;
            case MarineLife.LifeType.Shrimp:
                y += 0.5f;
                break;
            case MarineLife.LifeType.SeaCucumber:
            case MarineLife.LifeType.TubeWorm:
            case MarineLife.LifeType.Starfish:
            case MarineLife.LifeType.DeepSeaCrab:
                y += 0.5f;
                break;
        }

        return new Vector3(x, y, z);
    }

    private GameObject CreateLifeObject(MarineLife.LifeType type, Vector3 position)
    {
        GameObject lifeObj = new GameObject(type.ToString());
        lifeObj.transform.position = position;

        MarineLife marineLife = lifeObj.AddComponent<MarineLife>();
        marineLife.lifeType = type;
        marineLife.SetTerrainGenerator(terrainGenerator);

        switch (type)
        {
            case MarineLife.LifeType.Fish:
                ConfigureFish(marineLife, lifeObj);
                break;
            case MarineLife.LifeType.Coral:
                ConfigureCoral(marineLife, lifeObj);
                break;
            case MarineLife.LifeType.Shrimp:
                ConfigureShrimp(marineLife, lifeObj);
                break;
            case MarineLife.LifeType.Jellyfish:
                ConfigureJellyfish(marineLife, lifeObj);
                break;
            case MarineLife.LifeType.Sponge:
                ConfigureSponge(marineLife, lifeObj);
                break;
            case MarineLife.LifeType.SeaCucumber:
                ConfigureSeaCucumber(marineLife, lifeObj);
                break;
            case MarineLife.LifeType.TubeWorm:
                ConfigureTubeWorm(marineLife, lifeObj);
                break;
            case MarineLife.LifeType.SeaAnemone:
                ConfigureSeaAnemone(marineLife, lifeObj);
                break;
            case MarineLife.LifeType.Starfish:
                ConfigureStarfish(marineLife, lifeObj);
                break;
            case MarineLife.LifeType.DeepSeaCrab:
                ConfigureDeepSeaCrab(marineLife, lifeObj);
                break;
        }

        marineLife.stressedColor = new Color(0.8f, 0.3f, 0.3f);
        marineLife.OnDeath += OnCreatureDeath;
        marineLife.OnMigrationStart += OnCreatureMigration;

        return lifeObj;
    }

    private void OnCreatureDeath(MarineLife creature)
    {
        FoodWebSystem.Instance?.OnCreatureDeath(creature);
    }

    private void OnCreatureMigration(MarineLife creature)
    {
        FoodWebSystem.Instance?.OnCreatureMigration(creature);
    }

    private void ConfigureFish(MarineLife ml, GameObject obj)
    {
        CreateFishVisual(obj);
        ml.moveSpeed = 3f;
        ml.migrationSpeed = 5f;
        ml.migrationDistance = 60f;
        ml.canMigrate = true;
        ml.trophicLevel = MarineLife.TrophicLevel.SecondaryConsumer;
        ml.foodWebRole = MarineLife.FoodWebRole.Predator;
        ml.sedimentSensitivity = 0.8f;
        ml.stressThreshold = 0.35f;
        ml.migrationTriggerStress = 0.5f;
        ml.stressDamageRate = 12f;
        ml.normalColor = new Color(0.4f, 0.7f, 0.9f);
    }

    private void ConfigureCoral(MarineLife ml, GameObject obj)
    {
        CreateCoralVisual(obj);
        ml.moveSpeed = 0f;
        ml.canMigrate = false;
        ml.trophicLevel = MarineLife.TrophicLevel.PrimaryProducer;
        ml.foodWebRole = MarineLife.FoodWebRole.Symbiont;
        ml.sedimentSensitivity = 2f;
        ml.stressThreshold = 0.2f;
        ml.stressDamageRate = 20f;
        ml.maxHealth = 150f;
        ml.normalColor = new Color(0.9f, 0.5f, 0.7f);
    }

    private void ConfigureShrimp(MarineLife ml, GameObject obj)
    {
        CreateShrimpVisual(obj);
        ml.moveSpeed = 1.5f;
        ml.migrationSpeed = 2.5f;
        ml.migrationDistance = 30f;
        ml.canMigrate = true;
        ml.trophicLevel = MarineLife.TrophicLevel.PrimaryConsumer;
        ml.foodWebRole = MarineLife.FoodWebRole.Grazer;
        ml.sedimentSensitivity = 1.5f;
        ml.stressThreshold = 0.3f;
        ml.migrationTriggerStress = 0.5f;
        ml.stressDamageRate = 18f;
        ml.maxHealth = 60f;
        ml.normalColor = new Color(0.9f, 0.7f, 0.4f);
    }

    private void ConfigureJellyfish(MarineLife ml, GameObject obj)
    {
        CreateJellyfishVisual(obj);
        ml.moveSpeed = 1f;
        ml.migrationSpeed = 2f;
        ml.migrationDistance = 40f;
        ml.canMigrate = true;
        ml.trophicLevel = MarineLife.TrophicLevel.SecondaryConsumer;
        ml.foodWebRole = MarineLife.FoodWebRole.Predator;
        ml.sedimentSensitivity = 1.2f;
        ml.stressThreshold = 0.35f;
        ml.migrationTriggerStress = 0.55f;
        ml.stressDamageRate = 10f;
        ml.normalColor = new Color(0.7f, 0.9f, 1f);
    }

    private void ConfigureSponge(MarineLife ml, GameObject obj)
    {
        CreateSpongeVisual(obj);
        ml.moveSpeed = 0f;
        ml.canMigrate = false;
        ml.trophicLevel = MarineLife.TrophicLevel.PrimaryConsumer;
        ml.foodWebRole = MarineLife.FoodWebRole.FilterFeeder;
        ml.sedimentSensitivity = 2.5f;
        ml.stressThreshold = 0.15f;
        ml.stressDamageRate = 25f;
        ml.maxHealth = 120f;
        ml.normalColor = new Color(0.6f, 0.8f, 0.5f);
    }

    private void ConfigureSeaCucumber(MarineLife ml, GameObject obj)
    {
        CreateSeaCucumberVisual(obj);
        ml.moveSpeed = 0.5f;
        ml.migrationSpeed = 0.8f;
        ml.migrationDistance = 20f;
        ml.canMigrate = true;
        ml.trophicLevel = MarineLife.TrophicLevel.Decomposer;
        ml.foodWebRole = MarineLife.FoodWebRole.Decomposer;
        ml.sedimentSensitivity = 3f;
        ml.stressThreshold = 0.25f;
        ml.migrationTriggerStress = 0.4f;
        ml.stressDamageRate = 22f;
        ml.maxHealth = 80f;
        ml.normalColor = new Color(0.6f, 0.45f, 0.3f);
        ml.stressedColor = new Color(0.9f, 0.4f, 0.2f);

        BenthicBehavior benthic = obj.AddComponent<BenthicBehavior>();
        benthic.staysOnSeafloor = true;
        benthic.seafloorOffset = 0.5f;
        benthic.sedimentFeedingRate = 1f;
        benthic.sedimentSensitivityMultiplier = 2f;
        benthic.feedingCutoffThreshold = 0.6f;
        benthic.canBeBuried = true;
        benthic.burialThreshold = 0.8f;
        benthic.canBurrow = true;
        benthic.burrowProtectionRatio = 0.5f;
        benthic.burrowStressThreshold = 0.35f;
        benthic.feedingRadius = 3f;
        benthic.feedingInterval = 5f;
        benthic.feedingHealthRecovery = 3f;
    }

    private void ConfigureTubeWorm(MarineLife ml, GameObject obj)
    {
        CreateTubeWormVisual(obj);
        ml.moveSpeed = 0f;
        ml.canMigrate = false;
        ml.trophicLevel = MarineLife.TrophicLevel.PrimaryConsumer;
        ml.foodWebRole = MarineLife.FoodWebRole.FilterFeeder;
        ml.sedimentSensitivity = 2.8f;
        ml.stressThreshold = 0.15f;
        ml.stressDamageRate = 28f;
        ml.maxHealth = 100f;
        ml.normalColor = new Color(0.8f, 0.3f, 0.2f);

        BenthicBehavior benthic = obj.AddComponent<BenthicBehavior>();
        benthic.staysOnSeafloor = true;
        benthic.seafloorOffset = 2f;
        benthic.sedimentFeedingRate = 0.5f;
        benthic.sedimentSensitivityMultiplier = 2.5f;
        benthic.feedingCutoffThreshold = 0.5f;
        benthic.canBeBuried = true;
        benthic.burialThreshold = 0.7f;
        benthic.canBurrow = false;
        benthic.feedingRadius = 2f;
        benthic.feedingInterval = 8f;
        benthic.feedingHealthRecovery = 2f;
    }

    private void ConfigureSeaAnemone(MarineLife ml, GameObject obj)
    {
        CreateSeaAnemoneVisual(obj);
        ml.moveSpeed = 0f;
        ml.canMigrate = false;
        ml.trophicLevel = MarineLife.TrophicLevel.SecondaryConsumer;
        ml.foodWebRole = MarineLife.FoodWebRole.Predator;
        ml.sedimentSensitivity = 2f;
        ml.stressThreshold = 0.2f;
        ml.stressDamageRate = 20f;
        ml.maxHealth = 90f;
        ml.normalColor = new Color(0.8f, 0.4f, 0.7f);

        BenthicBehavior benthic = obj.AddComponent<BenthicBehavior>();
        benthic.staysOnSeafloor = true;
        benthic.seafloorOffset = 1.5f;
        benthic.sedimentFeedingRate = 0f;
        benthic.sedimentSensitivityMultiplier = 1.8f;
        benthic.canBeBuried = true;
        benthic.burialThreshold = 0.75f;
        benthic.canBurrow = true;
        benthic.burrowProtectionRatio = 0.3f;
        benthic.burrowStressThreshold = 0.4f;
    }

    private void ConfigureStarfish(MarineLife ml, GameObject obj)
    {
        CreateStarfishVisual(obj);
        ml.moveSpeed = 0.8f;
        ml.migrationSpeed = 1.2f;
        ml.migrationDistance = 25f;
        ml.canMigrate = true;
        ml.trophicLevel = MarineLife.TrophicLevel.SecondaryConsumer;
        ml.foodWebRole = MarineLife.FoodWebRole.Omnivore;
        ml.sedimentSensitivity = 1.8f;
        ml.stressThreshold = 0.3f;
        ml.migrationTriggerStress = 0.45f;
        ml.stressDamageRate = 16f;
        ml.maxHealth = 90f;
        ml.normalColor = new Color(0.9f, 0.6f, 0.2f);

        BenthicBehavior benthic = obj.AddComponent<BenthicBehavior>();
        benthic.staysOnSeafloor = true;
        benthic.seafloorOffset = 0.3f;
        benthic.sedimentFeedingRate = 0.3f;
        benthic.sedimentSensitivityMultiplier = 1.5f;
        benthic.feedingCutoffThreshold = 0.65f;
        benthic.canBeBuried = false;
        benthic.canBurrow = false;
        benthic.feedingRadius = 4f;
        benthic.feedingInterval = 6f;
        benthic.feedingHealthRecovery = 2f;
    }

    private void ConfigureDeepSeaCrab(MarineLife ml, GameObject obj)
    {
        CreateDeepSeaCrabVisual(obj);
        ml.moveSpeed = 1.5f;
        ml.migrationSpeed = 2.5f;
        ml.migrationDistance = 40f;
        ml.canMigrate = true;
        ml.trophicLevel = MarineLife.TrophicLevel.TopPredator;
        ml.foodWebRole = MarineLife.FoodWebRole.Omnivore;
        ml.sedimentSensitivity = 1f;
        ml.stressThreshold = 0.4f;
        ml.migrationTriggerStress = 0.6f;
        ml.stressDamageRate = 10f;
        ml.maxHealth = 120f;
        ml.normalColor = new Color(0.5f, 0.35f, 0.25f);

        BenthicBehavior benthic = obj.AddComponent<BenthicBehavior>();
        benthic.staysOnSeafloor = true;
        benthic.seafloorOffset = 0.8f;
        benthic.sedimentFeedingRate = 0f;
        benthic.sedimentSensitivityMultiplier = 1.2f;
        benthic.canBeBuried = false;
        benthic.canBurrow = false;
    }

    private void CreateFishVisual(GameObject obj)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.transform.parent = obj.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(1.5f, 0.8f, 1f);

        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tail.transform.parent = obj.transform;
        tail.transform.localPosition = new Vector3(-1f, 0f, 0f);
        tail.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);

        ApplyMaterial(obj, new Color(0.4f, 0.7f, 0.9f));
        RemoveChildColliders(obj);
    }

    private void CreateCoralVisual(GameObject obj)
    {
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        main.transform.parent = obj.transform;
        main.transform.localPosition = Vector3.zero;
        main.transform.localScale = new Vector3(1f, 2f, 1f);

        GameObject branch1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        branch1.transform.parent = obj.transform;
        branch1.transform.localPosition = new Vector3(0.5f, 1f, 0f);
        branch1.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
        branch1.transform.localRotation = Quaternion.Euler(0, 0, 30f);

        GameObject branch2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        branch2.transform.parent = obj.transform;
        branch2.transform.localPosition = new Vector3(-0.3f, 0.8f, 0.4f);
        branch2.transform.localScale = new Vector3(0.4f, 1.2f, 0.4f);
        branch2.transform.localRotation = Quaternion.Euler(0, 0, -25f);

        ApplyMaterial(obj, new Color(0.9f, 0.5f, 0.7f));
        RemoveChildColliders(obj);
    }

    private void CreateShrimpVisual(GameObject obj)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.parent = obj.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.parent = obj.transform;
        head.transform.localPosition = new Vector3(0.6f, 0f, 0f);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        ApplyMaterial(obj, new Color(0.9f, 0.7f, 0.4f));
        RemoveChildColliders(obj);
    }

    private void CreateJellyfishVisual(GameObject obj)
    {
        GameObject bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bell.transform.parent = obj.transform;
        bell.transform.localPosition = Vector3.zero;
        bell.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);

        for (int i = 0; i < 4; i++)
        {
            GameObject tentacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tentacle.transform.parent = obj.transform;
            float angle = i * 90f;
            tentacle.transform.localPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 0.5f,
                -1f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 0.5f
            );
            tentacle.transform.localScale = new Vector3(0.1f, 1.5f, 0.1f);
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.7f, 0.9f, 1f, 0.6f);
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        foreach (Renderer r in renderers)
        {
            r.material = mat;
        }
        RemoveChildColliders(obj);
    }

    private void CreateSpongeVisual(GameObject obj)
    {
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.transform.parent = obj.transform;
        main.transform.localPosition = Vector3.zero;
        main.transform.localScale = new Vector3(1.5f, 2f, 1.2f);

        ApplyMaterial(obj, new Color(0.6f, 0.8f, 0.5f));
        RemoveChildColliders(obj);
    }

    private void CreateSeaCucumberVisual(GameObject obj)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.parent = obj.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.8f, 2f, 0.7f);

        GameObject mouth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mouth.transform.parent = obj.transform;
        mouth.transform.localPosition = new Vector3(1.5f, 0f, 0f);
        mouth.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        for (int i = 0; i < 3; i++)
        {
            GameObject tentacleRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tentacleRing.transform.parent = obj.transform;
            tentacleRing.transform.localPosition = new Vector3(1.8f, 0f, (i - 1) * 0.25f);
            tentacleRing.transform.localScale = new Vector3(0.05f, 0.4f, 0.05f);
            tentacleRing.transform.localRotation = Quaternion.Euler(0, 0, 90f + (i - 1) * 20f);
        }

        ApplyMaterial(obj, new Color(0.6f, 0.45f, 0.3f));
        RemoveChildColliders(obj);
    }

    private void CreateTubeWormVisual(GameObject obj)
    {
        GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tube.transform.parent = obj.transform;
        tube.transform.localPosition = Vector3.zero;
        tube.transform.localScale = new Vector3(0.3f, 3f, 0.3f);

        GameObject plume = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        plume.transform.parent = obj.transform;
        plume.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        plume.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);

        for (int i = 0; i < 6; i++)
        {
            GameObject filament = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            filament.transform.parent = obj.transform;
            float angle = i * 60f;
            filament.transform.localPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 0.3f,
                2.2f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 0.3f
            );
            filament.transform.localScale = new Vector3(0.03f, 0.6f, 0.03f);
            filament.transform.localRotation = Quaternion.Euler(0, 0, 30f + (i % 2) * 20f);
        }

        ApplyMaterial(obj, new Color(0.8f, 0.3f, 0.2f));
        RemoveChildColliders(obj);
    }

    private void CreateSeaAnemoneVisual(GameObject obj)
    {
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.transform.parent = obj.transform;
        baseObj.transform.localPosition = Vector3.zero;
        baseObj.transform.localScale = new Vector3(1f, 1f, 1f);

        for (int i = 0; i < 8; i++)
        {
            GameObject tentacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tentacle.transform.parent = obj.transform;
            float angle = i * 45f;
            float radius = 0.4f;
            tentacle.transform.localPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                0.8f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );
            tentacle.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
            tentacle.transform.localRotation = Quaternion.Euler(
                Mathf.Sin(angle * Mathf.Deg2Rad) * 30f,
                0,
                -Mathf.Cos(angle * Mathf.Deg2Rad) * 30f
            );
        }

        ApplyMaterial(obj, new Color(0.8f, 0.4f, 0.7f));
        RemoveChildColliders(obj);
    }

    private void CreateStarfishVisual(GameObject obj)
    {
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        center.transform.parent = obj.transform;
        center.transform.localPosition = Vector3.zero;
        center.transform.localScale = new Vector3(0.5f, 0.2f, 0.5f);

        for (int i = 0; i < 5; i++)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.transform.parent = obj.transform;
            float angle = i * 72f;
            arm.transform.localPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 0.8f,
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 0.8f
            );
            arm.transform.localScale = new Vector3(0.35f, 0.7f, 0.2f);
            arm.transform.localRotation = Quaternion.Euler(90f, -angle, 0f);
        }

        ApplyMaterial(obj, new Color(0.9f, 0.6f, 0.2f));
        RemoveChildColliders(obj);
    }

    private void CreateDeepSeaCrabVisual(GameObject obj)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.transform.parent = obj.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(1.2f, 0.6f, 1f);

        for (int i = 0; i < 3; i++)
        {
            GameObject legLeft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            legLeft.transform.parent = obj.transform;
            legLeft.transform.localPosition = new Vector3(-0.3f + i * 0.3f, -0.2f, 0.8f);
            legLeft.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f);
            legLeft.transform.localRotation = Quaternion.Euler(0, 0, 30f + i * 15f);

            GameObject legRight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            legRight.transform.parent = obj.transform;
            legRight.transform.localPosition = new Vector3(-0.3f + i * 0.3f, -0.2f, -0.8f);
            legRight.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f);
            legRight.transform.localRotation = Quaternion.Euler(0, 0, -30f - i * 15f);
        }

        GameObject clawLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        clawLeft.transform.parent = obj.transform;
        clawLeft.transform.localPosition = new Vector3(0.8f, 0.2f, 0.6f);
        clawLeft.transform.localScale = new Vector3(0.4f, 0.3f, 0.3f);

        GameObject clawRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        clawRight.transform.parent = obj.transform;
        clawRight.transform.localPosition = new Vector3(0.8f, 0.2f, -0.6f);
        clawRight.transform.localScale = new Vector3(0.35f, 0.25f, 0.25f);

        ApplyMaterial(obj, new Color(0.5f, 0.35f, 0.25f));
        RemoveChildColliders(obj);
    }

    private void ApplyMaterial(GameObject obj, Color color)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            foreach (Renderer r in renderers)
            {
                r.material = mat;
            }
        }
    }

    private void RemoveChildColliders(GameObject parent)
    {
        Collider[] colliders = parent.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            Destroy(col);
        }
    }
}
