using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject ecologyPanel;
    public GameObject miningPanel;
    public GameObject settingsPanel;
    public GameObject foodWebPanel;

    [Header("Ecology Indicators")]
    public Slider overallHealthSlider;
    public Text overallHealthText;
    public Text healthStatusText;
    public Slider waterQualitySlider;
    public Text waterQualityText;
    public Slider biodiversitySlider;
    public Text biodiversityText;
    public Slider habitatSlider;
    public Text habitatText;
    public Slider carbonSlider;
    public Text carbonText;
    public Slider sedimentSlider;
    public Text sedimentText;
    public Slider foodWebSlider;
    public Text foodWebText;

    [Header("Mining Stats")]
    public Text nodulesCollectedText;
    public Text mineralValueText;
    public Slider miningRateSlider;
    public Text miningRateText;

    [Header("Food Web Display")]
    public Text speciesStatusText;
    public Text deathCountText;
    public Text migrationCountText;
    public Text cascadeWarningText;

    [Header("Buttons")]
    public Button ecologyBtn;
    public Button miningBtn;
    public Button settingsBtn;
    public Button foodWebBtn;
    public Button resetBtn;
    public Button pauseBtn;

    [Header("Control Info")]
    public Text controlInfoText;

    private bool isPaused = false;
    private float foodWebUpdateTimer = 0f;
    private float foodWebUpdateInterval = 1f;

    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        ShowEcologyPanel();
    }

    private void InitializeUI()
    {
        if (miningRateSlider != null)
        {
            miningRateSlider.minValue = 0f;
            miningRateSlider.maxValue = 1f;
            miningRateSlider.value = GameManager.Instance?.miningRate ?? 0.5f;
            miningRateSlider.onValueChanged.AddListener(OnMiningRateChanged);
        }

        if (ecologyBtn != null) ecologyBtn.onClick.AddListener(ShowEcologyPanel);
        if (miningBtn != null) miningBtn.onClick.AddListener(ShowMiningPanel);
        if (settingsBtn != null) settingsBtn.onClick.AddListener(ShowSettingsPanel);
        if (foodWebBtn != null) foodWebBtn.onClick.AddListener(ShowFoodWebPanel);
        if (resetBtn != null) resetBtn.onClick.AddListener(ResetSimulation);
        if (pauseBtn != null) pauseBtn.onClick.AddListener(TogglePause);

        if (controlInfoText != null)
        {
            controlInfoText.text = "WASD - 移动采集机\n空格 - 开始采矿\nESC - 暂停\nF3 - 性能监控";
        }
    }

    private void SubscribeToEvents()
    {
        if (EcologyManager.Instance != null)
        {
            EcologyManager.Instance.OnOverallHealthChanged += UpdateOverallHealth;
            EcologyManager.Instance.OnWaterQualityChanged += UpdateWaterQuality;
            EcologyManager.Instance.OnBiodiversityChanged += UpdateBiodiversity;
            EcologyManager.Instance.OnHabitatIntegrityChanged += UpdateHabitat;
            EcologyManager.Instance.OnCarbonSequestrationChanged += UpdateCarbon;
            EcologyManager.Instance.OnFoodWebStabilityChanged += UpdateFoodWebStability;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSedimentLevelChanged += UpdateSediment;
        }

        if (MiningManager.Instance != null)
        {
            MiningManager.Instance.OnNodulesCollected += UpdateNodulesCollected;
            MiningManager.Instance.OnMineralValueChanged += UpdateMineralValue;
        }
    }

    private void UpdateOverallHealth(float value)
    {
        if (overallHealthSlider != null) overallHealthSlider.value = value / 100f;
        if (overallHealthText != null) overallHealthText.text = $"{value:F1}%";
        if (healthStatusText != null) healthStatusText.text = EcologyManager.Instance?.GetHealthStatus() ?? "";

        if (overallHealthSlider != null)
        {
            Color healthColor = GetHealthColor(value);
            overallHealthSlider.fillRect.GetComponent<Image>().color = healthColor;
        }
    }

    private void UpdateWaterQuality(float value)
    {
        if (waterQualitySlider != null) waterQualitySlider.value = value / 100f;
        if (waterQualityText != null) waterQualityText.text = $"{value:F1}%";
    }

    private void UpdateBiodiversity(float value)
    {
        if (biodiversitySlider != null) biodiversitySlider.value = value / 100f;
        if (biodiversityText != null) biodiversityText.text = $"{value:F1}%";
    }

    private void UpdateHabitat(float value)
    {
        if (habitatSlider != null) habitatSlider.value = value / 100f;
        if (habitatText != null) habitatText.text = $"{value:F1}%";
    }

    private void UpdateCarbon(float value)
    {
        if (carbonSlider != null) carbonSlider.value = value / 100f;
        if (carbonText != null) carbonText.text = $"{value:F1}%";
    }

    private void UpdateFoodWebStability(float value)
    {
        if (foodWebSlider != null) foodWebSlider.value = value / 100f;
        if (foodWebText != null) foodWebText.text = $"{value:F1}%";
    }

    private void UpdateSediment(float value)
    {
        if (sedimentSlider != null) sedimentSlider.value = value;
        if (sedimentText != null) sedimentText.text = $"{value * 100:F1}%";
    }

    private void UpdateNodulesCollected(int count)
    {
        if (nodulesCollectedText != null) nodulesCollectedText.text = count.ToString();
    }

    private void UpdateMineralValue(int value)
    {
        if (mineralValueText != null) mineralValueText.text = value.ToString();
    }

    private void OnMiningRateChanged(float value)
    {
        GameManager.Instance?.SetMiningRate(value);
        if (miningRateText != null) miningRateText.text = $"{value * 100:F0}%";
    }

    private Color GetHealthColor(float health)
    {
        if (health >= 80f) return Color.green;
        if (health >= 60f) return Color.yellow;
        if (health >= 40f) return new Color(1f, 0.5f, 0f);
        if (health >= 20f) return new Color(1f, 0.3f, 0f);
        return Color.red;
    }

    private void UpdateFoodWebDisplay()
    {
        if (FoodWebSystem.Instance == null) return;

        var speciesData = FoodWebSystem.Instance.GetAllSpeciesData();

        if (speciesStatusText != null)
        {
            string status = "";
            foreach (var kvp in speciesData)
            {
                var data = kvp.Value;
                Color statusColor = GetStatusColor(data.status);
                string bar = GenerateHealthBar(data.populationRatio);
                status += $"{GetSpeciesDisplayName(data.type)}: {data.currentCount}/{data.initialCount} [{data.status}] {bar}";
                if (data.starvationLevel > 0.1f)
                {
                    status += $" 饥饿:{data.starvationLevel:P0}";
                }
                status += "\n";
            }
            speciesStatusText.text = status;
        }

        if (deathCountText != null)
        {
            deathCountText.text = $"总死亡: {FoodWebSystem.Instance.totalDeaths}";
        }

        if (migrationCountText != null)
        {
            migrationCountText.text = $"总迁移: {FoodWebSystem.Instance.totalMigrations}";
        }

        if (cascadeWarningText != null)
        {
            string warnings = "";
            foreach (var kvp in speciesData)
            {
                if (kvp.Value.status == "灭绝")
                {
                    var predators = FoodWebSystem.Instance.GetPredatorsOf(kvp.Key);
                    foreach (var pred in predators)
                    {
                        warnings += $"⚠ {GetSpeciesDisplayName(kvp.Key)}灭绝 → {GetSpeciesDisplayName(pred)}食物短缺!\n";
                    }
                }
                else if (kvp.Value.status == "濒危")
                {
                    warnings += $"⚠ {GetSpeciesDisplayName(kvp.Key)}濒临灭绝!\n";
                }
            }
            cascadeWarningText.text = warnings;
            cascadeWarningText.color = string.IsNullOrEmpty(warnings) ? Color.white : Color.red;
        }
    }

    private string GetSpeciesDisplayName(MarineLife.LifeType type)
    {
        switch (type)
        {
            case MarineLife.LifeType.Fish: return "鱼类";
            case MarineLife.LifeType.Coral: return "珊瑚";
            case MarineLife.LifeType.Shrimp: return "虾类";
            case MarineLife.LifeType.Jellyfish: return "水母";
            case MarineLife.LifeType.Sponge: return "海绵";
            case MarineLife.LifeType.SeaCucumber: return "海参";
            case MarineLife.LifeType.TubeWorm: return "管虫";
            case MarineLife.LifeType.SeaAnemone: return "海葵";
            case MarineLife.LifeType.Starfish: return "海星";
            case MarineLife.LifeType.DeepSeaCrab: return "深海蟹";
            default: return type.ToString();
        }
    }

    private Color GetStatusColor(string status)
    {
        switch (status)
        {
            case "稳定": return Color.green;
            case "下降": return Color.yellow;
            case "易危": return new Color(1f, 0.5f, 0f);
            case "濒危": return Color.red;
            case "灭绝": return Color.gray;
            default: return Color.white;
        }
    }

    private string GenerateHealthBar(float ratio)
    {
        int bars = Mathf.RoundToInt(ratio * 10f);
        return "[" + new string('█', bars) + new string('░', 10 - bars) + "]";
    }

    public void ShowEcologyPanel()
    {
        ecologyPanel?.SetActive(true);
        miningPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        foodWebPanel?.SetActive(false);
    }

    public void ShowMiningPanel()
    {
        ecologyPanel?.SetActive(false);
        miningPanel?.SetActive(true);
        settingsPanel?.SetActive(false);
        foodWebPanel?.SetActive(false);
    }

    public void ShowSettingsPanel()
    {
        ecologyPanel?.SetActive(false);
        miningPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
        foodWebPanel?.SetActive(false);
    }

    public void ShowFoodWebPanel()
    {
        ecologyPanel?.SetActive(false);
        miningPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        foodWebPanel?.SetActive(true);
    }

    public void ResetSimulation()
    {
        MiningManager.Instance?.ResetStats();
        EcologyManager.Instance?.ResetEcology();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateSedimentLevel(0f);
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pauseBtn != null)
        {
            pauseBtn.GetComponentInChildren<Text>().text = isPaused ? "继续" : "暂停";
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        foodWebUpdateTimer += Time.deltaTime;
        if (foodWebUpdateTimer >= foodWebUpdateInterval)
        {
            foodWebUpdateTimer = 0f;
            if (foodWebPanel != null && foodWebPanel.activeSelf)
            {
                UpdateFoodWebDisplay();
            }
        }
    }

    private void OnDestroy()
    {
        if (EcologyManager.Instance != null)
        {
            EcologyManager.Instance.OnOverallHealthChanged -= UpdateOverallHealth;
            EcologyManager.Instance.OnWaterQualityChanged -= UpdateWaterQuality;
            EcologyManager.Instance.OnBiodiversityChanged -= UpdateBiodiversity;
            EcologyManager.Instance.OnHabitatIntegrityChanged -= UpdateHabitat;
            EcologyManager.Instance.OnCarbonSequestrationChanged -= UpdateCarbon;
            EcologyManager.Instance.OnFoodWebStabilityChanged -= UpdateFoodWebStability;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSedimentLevelChanged -= UpdateSediment;
        }

        if (MiningManager.Instance != null)
        {
            MiningManager.Instance.OnNodulesCollected -= UpdateNodulesCollected;
            MiningManager.Instance.OnMineralValueChanged -= UpdateMineralValue;
        }
    }
}
