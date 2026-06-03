using UnityEngine;

[RequireComponent(typeof(MarineLife))]
public class BenthicBehavior : MonoBehaviour
{
    [Header("Benthic Settings")]
    [Tooltip("是否紧贴海底移动")]
    public bool staysOnSeafloor = true;
    [Tooltip("海底偏移高度")]
    public float seafloorOffset = 0.5f;

    [Header("Sediment Interaction")]
    [Tooltip("沉积物摄食效率（海参等沉积食性动物）")]
    public float sedimentFeedingRate = 0f;
    [Tooltip("沉积物敏感度倍率")]
    public float sedimentSensitivityMultiplier = 2f;
    [Tooltip("沉积物浓度过高时停止摄食的阈值")]
    public float feedingCutoffThreshold = 0.6f;
    [Tooltip("是否因沉积物掩埋而死亡")]
    public bool canBeBuried = true;
    [Tooltip("掩埋致死的沉积物浓度")]
    public float burialThreshold = 0.8f;

    [Header("Burrowing")]
    [Tooltip("能否钻入沉积物躲避")]
    public bool canBurrow = true;
    [Tooltip("钻入沉积物时减少的伤害比例")]
    [Range(0f, 1f)]
    public float burrowProtectionRatio = 0.5f;
    [Tooltip("开始钻入的压力阈值")]
    public float burrowStressThreshold = 0.4f;

    [Header("Feeding")]
    [Tooltip("摄食范围")]
    public float feedingRadius = 3f;
    [Tooltip("摄食间隔(秒)")]
    public float feedingInterval = 5f;
    [Tooltip("摄食带来的健康恢复")]
    public float feedingHealthRecovery = 3f;

    [Header("State")]
    public bool isBurrowed = false;
    public bool isFeeding = false;
    public float currentSedimentExposure = 0f;
    public float feedingTimer = 0f;

    private MarineLife marineLife;
    private SeabedTerrainGenerator terrainGen;
    private float originalSedimentSensitivity;
    private float originalStressDamageRate;

    private void Start()
    {
        marineLife = GetComponent<MarineLife>();
        terrainGen = FindObjectOfType<SeabedTerrainGenerator>();

        originalSedimentSensitivity = marineLife.sedimentSensitivity;
        marineLife.sedimentSensitivity *= sedimentSensitivityMultiplier;
        originalStressDamageRate = marineLife.stressDamageRate;

        marineLife.OnMigrationStart += OnMigrationStart;
        marineLife.OnMigrationEnd += OnMigrationEnd;
    }

    private void Update()
    {
        if (marineLife == null || marineLife.isDead) return;

        UpdateSedimentExposure();

        if (staysOnSeafloor)
        {
            SnapToSeafloor();
        }

        HandleBurrowing();
        HandleFeeding();
        HandleBurial();
    }

    private void UpdateSedimentExposure()
    {
        float localSediment = PlumeManager.Instance?.GetSedimentDensityAtPosition(transform.position, 10f) ?? 0f;
        currentSedimentExposure = localSediment;
    }

    private void SnapToSeafloor()
    {
        if (terrainGen == null) return;

        float terrainY = terrainGen.GetTerrainHeight(transform.position.x, transform.position.z);
        Vector3 targetPos = transform.position;
        targetPos.y = terrainY + seafloorOffset;

        transform.position = Vector3.Lerp(transform.position, targetPos, 3f * Time.deltaTime);
    }

    private void HandleBurrowing()
    {
        if (!canBurrow) return;

        if (marineLife.currentStressLevel > burrowStressThreshold && !isBurrowed && !marineLife.isMigrating)
        {
            StartBurrow();
        }
        else if (marineLife.currentStressLevel < burrowStressThreshold * 0.5f && isBurrowed)
        {
            EndBurrow();
        }
    }

    private void StartBurrow()
    {
        isBurrowed = true;
        marineLife.stressDamageRate = originalStressDamageRate * (1f - burrowProtectionRatio);
        marineLife.moveSpeed *= 0.1f;

        transform.position -= Vector3.up * (seafloorOffset * 0.5f);
    }

    private void EndBurrow()
    {
        isBurrowed = false;
        marineLife.stressDamageRate = originalStressDamageRate;

        MarineLife.LifeType type = marineLife.lifeType;
        switch (type)
        {
            case MarineLife.LifeType.SeaCucumber:
                marineLife.moveSpeed = 0.5f;
                break;
            case MarineLife.LifeType.TubeWorm:
                marineLife.moveSpeed = 0f;
                break;
            case MarineLife.LifeType.Starfish:
                marineLife.moveSpeed = 0.8f;
                break;
            case MarineLife.LifeType.DeepSeaCrab:
                marineLife.moveSpeed = 1.5f;
                break;
            default:
                marineLife.moveSpeed = 1f;
                break;
        }
    }

    private void HandleFeeding()
    {
        if (sedimentFeedingRate <= 0f) return;

        feedingTimer += Time.deltaTime;

        if (currentSedimentExposure < feedingCutoffThreshold && feedingTimer >= feedingInterval)
        {
            feedingTimer = 0f;
            isFeeding = true;

            float feedQuality = 1f - (currentSedimentExposure / feedingCutoffThreshold);
            float healthGain = feedingHealthRecovery * feedQuality;
            marineLife.currentHealth = Mathf.Min(marineLife.maxHealth, marineLife.currentHealth + healthGain);

            Invoke("EndFeeding", 1f);
        }
        else if (currentSedimentExposure >= feedingCutoffThreshold)
        {
            isFeeding = false;
        }
    }

    private void EndFeeding()
    {
        isFeeding = false;
    }

    private void HandleBurial()
    {
        if (!canBeBuried) return;

        if (currentSedimentExposure >= burialThreshold && !isBurrowed)
        {
            float burialDamage = 30f * (currentSedimentExposure - burialThreshold) * Time.deltaTime;
            marineLife.currentHealth -= burialDamage;

            if (marineLife.currentHealth <= 0f && !marineLife.isDead)
            {
                Debug.LogWarningFormat("[Benthic] {0} 被沉积物掩埋致死！", marineLife.lifeType);
            }
        }
    }

    private void OnMigrationStart(MarineLife creature)
    {
        if (isBurrowed)
        {
            EndBurrow();
        }
    }

    private void OnMigrationEnd(MarineLife creature)
    {
        if (staysOnSeafloor && terrainGen != null)
        {
            float terrainY = terrainGen.GetTerrainHeight(transform.position.x, transform.position.z);
            transform.position = new Vector3(transform.position.x, terrainY + seafloorOffset, transform.position.z);
        }
    }

    private void OnDestroy()
    {
        if (marineLife != null)
        {
            marineLife.OnMigrationStart -= OnMigrationStart;
            marineLife.OnMigrationEnd -= OnMigrationEnd;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isBurrowed)
        {
            Gizmos.color = new Color(0.6f, 0.4f, 0.2f, 0.5f);
            Gizmos.DrawSphere(transform.position, 1.5f);
        }

        if (isFeeding)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, feedingRadius);
        }

        if (canBeBuried && currentSedimentExposure > 0.3f)
        {
            Gizmos.color = Color.Lerp(Color.yellow, Color.red, currentSedimentExposure / burialThreshold);
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}
