using System.Collections.Generic;
using UnityEngine;

public class FoodWebSystem : MonoBehaviour
{
    public static FoodWebSystem Instance { get; private set; }

    [Header("Food Web Definition")]
    public List<PredatorPreyLink> foodWebLinks = new List<PredatorPreyLink>();

    [Header("Population Tracking")]
    public Dictionary<MarineLife.LifeType, int> initialPopulations = new Dictionary<MarineLife.LifeType, int>();
    public Dictionary<MarineLife.LifeType, int> currentPopulations = new Dictionary<MarineLife.LifeType, int>();
    public Dictionary<MarineLife.LifeType, int> deathCounts = new Dictionary<MarineLife.LifeType, int>();
    public Dictionary<MarineLife.LifeType, int> migrationCounts = new Dictionary<MarineLife.LifeType, int>();

    [Header("Cascade Settings")]
    [Tooltip("捕食者缺失对下一级的影响乘数")]
    public float cascadeMultiplier = 1.5f;
    [Tooltip("营养级消失对生态健康的贡献")]
    public float trophicImpactWeight = 0.3f;
    [Tooltip("饥饿检测间隔")]
    public float starvationCheckInterval = 2f;

    [Header("Debug")]
    public int totalDeaths;
    public int totalMigrations;
    public float foodWebStability = 100f;

    [Header("Events")]
    public event System.Action<MarineLife.LifeType, int> OnPopulationChanged;
    public event System.Action<MarineLife.LifeType> OnSpeciesExtinct;
    public event System.Action<MarineLife.LifeType, MarineLife.LifeType> OnCascadeTriggered;
    public event System.Action<float> OnFoodWebStabilityChanged;

    private Dictionary<MarineLife.LifeType, List<MarineLife.LifeType>> predatorMap = new Dictionary<MarineLife.LifeType, List<MarineLife.LifeType>>();
    private Dictionary<MarineLife.LifeType, List<MarineLife.LifeType>> preyMap = new Dictionary<MarineLife.LifeType, List<MarineLife.LifeType>>();
    private float lastStarvationCheck;

    [System.Serializable]
    public class PredatorPreyLink
    {
        public MarineLife.LifeType predator;
        public MarineLife.LifeType prey;
        [Tooltip("该猎物占捕食者食物的比例")]
        [Range(0f, 1f)]
        public float dietFraction = 0.5f;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildDefaultFoodWeb();
        BuildLookupMaps();
    }

    private void BuildDefaultFoodWeb()
    {
        foodWebLinks.Clear();

        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Fish, prey = MarineLife.LifeType.Shrimp, dietFraction = 0.4f });
        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Fish, prey = MarineLife.LifeType.SeaCucumber, dietFraction = 0.3f });
        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Fish, prey = MarineLife.LifeType.TubeWorm, dietFraction = 0.2f });

        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.DeepSeaCrab, prey = MarineLife.LifeType.SeaCucumber, dietFraction = 0.35f });
        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.DeepSeaCrab, prey = MarineLife.LifeType.Shrimp, dietFraction = 0.3f });
        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.DeepSeaCrab, prey = MarineLife.LifeType.Starfish, dietFraction = 0.2f });

        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Starfish, prey = MarineLife.LifeType.SeaCucumber, dietFraction = 0.3f });
        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Starfish, prey = MarineLife.LifeType.TubeWorm, dietFraction = 0.25f });
        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Starfish, prey = MarineLife.LifeType.Sponge, dietFraction = 0.2f });

        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Jellyfish, prey = MarineLife.LifeType.Shrimp, dietFraction = 0.5f });
        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Jellyfish, prey = MarineLife.LifeType.TubeWorm, dietFraction = 0.2f });

        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.SeaAnemone, prey = MarineLife.LifeType.Shrimp, dietFraction = 0.4f });
        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.SeaAnemone, prey = MarineLife.LifeType.TubeWorm, dietFraction = 0.3f });

        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Shrimp, prey = MarineLife.LifeType.TubeWorm, dietFraction = 0.3f });
        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.Shrimp, prey = MarineLife.LifeType.Sponge, dietFraction = 0.3f });

        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.SeaCucumber, prey = MarineLife.LifeType.Sponge, dietFraction = 0.4f });

        foodWebLinks.Add(new PredatorPreyLink { predator = MarineLife.LifeType.TubeWorm, prey = MarineLife.LifeType.Sponge, dietFraction = 0.2f });
    }

    private void BuildLookupMaps()
    {
        predatorMap.Clear();
        preyMap.Clear();

        foreach (var link in foodWebLinks)
        {
            if (!predatorMap.ContainsKey(link.predator))
                predatorMap[link.predator] = new List<MarineLife.LifeType>();
            predatorMap[link.predator].Add(link.prey);

            if (!preyMap.ContainsKey(link.prey))
                preyMap[link.prey] = new List<MarineLife.LifeType>();
            preyMap[link.prey].Add(link.predator);
        }
    }

    private void Update()
    {
        if (Time.time - lastStarvationCheck >= starvationCheckInterval)
        {
            CheckStarvationEffects();
            UpdateFoodWebStability();
            lastStarvationCheck = Time.time;
        }
    }

    public void RegisterInitialPopulation(MarineLife.LifeType type, int count)
    {
        if (!initialPopulations.ContainsKey(type))
            initialPopulations[type] = 0;
        initialPopulations[type] += count;

        if (!currentPopulations.ContainsKey(type))
            currentPopulations[type] = 0;
        currentPopulations[type] += count;
    }

    public void OnCreatureDeath(MarineLife creature)
    {
        MarineLife.LifeType type = creature.lifeType;

        if (currentPopulations.ContainsKey(type))
        {
            currentPopulations[type] = Mathf.Max(0, currentPopulations[type] - 1);
        }
        else
        {
            currentPopulations[type] = 0;
        }

        if (!deathCounts.ContainsKey(type))
            deathCounts[type] = 0;
        deathCounts[type]++;

        totalDeaths++;

        OnPopulationChanged?.Invoke(type, currentPopulations[type]);

        if (currentPopulations[type] == 0 && initialPopulations.ContainsKey(type) && initialPopulations[type] > 0)
        {
            OnSpeciesExtinct?.Invoke(type);
            Debug.LogWarningFormat("[FoodWeb] 物种灭绝: {0}", type);
            TriggerCascadeEffect(type);
        }
    }

    public void OnCreatureMigration(MarineLife creature)
    {
        MarineLife.LifeType type = creature.lifeType;

        if (!migrationCounts.ContainsKey(type))
            migrationCounts[type] = 0;
        migrationCounts[type]++;

        totalMigrations++;
    }

    private void CheckStarvationEffects()
    {
        List<MarineLife> allLife = EcologyManager.Instance?.GetAllMarineLife();
        if (allLife == null) return;

        foreach (var creature in allLife)
        {
            if (creature == null || !creature.IsAlive()) continue;

            float starvationIntensity = CalculateStarvationIntensity(creature.lifeType);
            if (starvationIntensity > 0.1f)
            {
                creature.ApplyStarvation(starvationIntensity);
            }
        }
    }

    private float CalculateStarvationIntensity(MarineLife.LifeType predatorType)
    {
        if (!predatorMap.ContainsKey(predatorType)) return 0f;

        float totalFoodAvailability = 0f;
        float totalDietNeed = 0f;

        foreach (var link in foodWebLinks)
        {
            if (link.predator != predatorType) continue;

            totalDietNeed += link.dietFraction;

            int preyCount = 0;
            int initialCount = 0;
            currentPopulations.TryGetValue(link.prey, out preyCount);
            initialPopulations.TryGetValue(link.prey, out initialCount);

            float availabilityRatio = initialCount > 0 ? (float)preyCount / initialCount : 0f;
            totalFoodAvailability += link.dietFraction * availabilityRatio;
        }

        if (totalDietNeed == 0f) return 0f;

        float starvation = 1f - (totalFoodAvailability / totalDietNeed);
        return Mathf.Clamp01(starvation);
    }

    private void TriggerCascadeEffect(MarineLife.LifeType extinctType)
    {
        if (!preyMap.ContainsKey(extinctType)) return;

        foreach (var predator in preyMap[extinctType])
        {
            float starvation = CalculateStarvationIntensity(predator);

            if (starvation > 0.5f)
            {
                OnCascadeTriggered?.Invoke(extinctType, predator);
                Debug.LogWarningFormat("[FoodWeb] 级联效应: {0} 灭绝 → {1} 食物短缺 ({2:P0})", extinctType, predator, starvation);
            }
        }

        if (predatorMap.ContainsKey(extinctType))
        {
            foreach (var prey in predatorMap[extinctType])
            {
                int preyCount = 0;
                currentPopulations.TryGetValue(prey, out preyCount);
                if (preyCount > 0)
                {
                    Debug.LogFormat("[FoodWeb] 级联效应: {0} 灭绝 → {1} 失去天敌，可能过度繁殖", extinctType, prey);
                }
            }
        }
    }

    private void UpdateFoodWebStability()
    {
        float totalStability = 0f;
        int speciesTracked = 0;

        foreach (var kvp in initialPopulations)
        {
            if (kvp.Value <= 0) continue;

            int current = 0;
            currentPopulations.TryGetValue(kvp.Key, out current);

            float ratio = (float)current / kvp.Value;
            float speciesStability = ratio * 100f;

            if (predatorMap.ContainsKey(kvp.Key))
            {
                float starvation = CalculateStarvationIntensity(kvp.Key);
                speciesStability *= (1f - starvation * 0.5f);
            }

            totalStability += speciesStability;
            speciesTracked++;
        }

        foodWebStability = speciesTracked > 0 ? totalStability / speciesTracked : 0f;

        OnFoodWebStabilityChanged?.Invoke(foodWebStability);
    }

    public List<MarineLife.LifeType> GetPredatorsOf(MarineLife.LifeType preyType)
    {
        if (preyMap.ContainsKey(preyType))
            return new List<MarineLife.LifeType>(preyMap[preyType]);
        return new List<MarineLife.LifeType>();
    }

    public List<MarineLife.LifeType> GetPreyOf(MarineLife.LifeType predatorType)
    {
        if (predatorMap.ContainsKey(predatorType))
            return new List<MarineLife.LifeType>(predatorMap[predatorType]);
        return new List<MarineLife.LifeType>();
    }

    public float GetPopulationRatio(MarineLife.LifeType type)
    {
        int current = 0;
        int initial = 0;
        currentPopulations.TryGetValue(type, out current);
        initialPopulations.TryGetValue(type, out initial);

        return initial > 0 ? (float)current / initial : 0f;
    }

    public string GetSpeciesStatus(MarineLife.LifeType type)
    {
        float ratio = GetPopulationRatio(type);
        if (ratio <= 0f) return "灭绝";
        if (ratio < 0.2f) return "濒危";
        if (ratio < 0.5f) return "易危";
        if (ratio < 0.8f) return "下降";
        return "稳定";
    }

    public Dictionary<MarineLife.LifeType, SpeciesData> GetAllSpeciesData()
    {
        Dictionary<MarineLife.LifeType, SpeciesData> data = new Dictionary<MarineLife.LifeType, SpeciesData>();

        foreach (var kvp in initialPopulations)
        {
            int current = 0;
            int deaths = 0;
            int migrations = 0;
            currentPopulations.TryGetValue(kvp.Key, out current);
            deathCounts.TryGetValue(kvp.Key, out deaths);
            migrationCounts.TryGetValue(kvp.Key, out migrations);

            data[kvp.Key] = new SpeciesData
            {
                type = kvp.Key,
                initialCount = kvp.Value,
                currentCount = current,
                deathCount = deaths,
                migrationCount = migrations,
                status = GetSpeciesStatus(kvp.Key),
                populationRatio = GetPopulationRatio(kvp.Key),
                starvationLevel = CalculateStarvationIntensity(kvp.Key)
            };
        }

        return data;
    }

    public struct SpeciesData
    {
        public MarineLife.LifeType type;
        public int initialCount;
        public int currentCount;
        public int deathCount;
        public int migrationCount;
        public string status;
        public float populationRatio;
        public float starvationLevel;
    }
}
