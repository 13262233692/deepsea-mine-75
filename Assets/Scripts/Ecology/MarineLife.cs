using UnityEngine;
using System.Collections;

public class MarineLife : MonoBehaviour
{
    [Header("Identity")]
    public LifeType lifeType = LifeType.Fish;
    public TrophicLevel trophicLevel = TrophicLevel.SecondaryConsumer;
    public FoodWebRole foodWebRole = FoodWebRole.Predator;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRadius = 15f;

    [Header("Health & Survival")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float stressThreshold = 0.3f;
    public float deathThreshold = 0f;
    public float sedimentSensitivity = 1f;
    public float healthRecoveryRate = 2f;
    public float stressDamageRate = 15f;
    public float starvationDamageRate = 5f;

    [Header("Migration")]
    public bool canMigrate = true;
    public float migrationSpeed = 1f;
    public float migrationDistance = 50f;
    public float migrationTriggerStress = 0.5f;
    public bool isMigrating = false;
    public Vector3 migrationDestination;

    [Header("Stress Response")]
    public float currentStressLevel = 0f;
    public float stressRecoveryRate = 0.1f;
    public float fleeDistance = 20f;

    [Header("State")]
    public LifeState state = LifeState.Healthy;
    public bool isDead = false;
    public float timeInStress = 0f;
    public float deathAnimationDuration = 3f;

    [Header("Visuals")]
    public Color normalColor = Color.white;
    public Color stressedColor = Color.red;
    public Color dyingColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public Color migratingColor = new Color(1f, 0.8f, 0.2f);

    [Header("Death Effect")]
    public GameObject deathEffectPrefab;
    public bool fadeOnDeath = true;

    public enum LifeType
    {
        Fish,
        Coral,
        Shrimp,
        Jellyfish,
        Sponge,
        SeaCucumber,
        TubeWorm,
        SeaAnemone,
        Starfish,
        DeepSeaCrab
    }

    public enum TrophicLevel
    {
        Decomposer,
        PrimaryProducer,
        PrimaryConsumer,
        SecondaryConsumer,
        TopPredator
    }

    public enum FoodWebRole
    {
        Decomposer,
        FilterFeeder,
        Grazer,
        Predator,
        Prey,
        Omnivore,
        Symbiont
    }

    public enum LifeState
    {
        Healthy,
        Stressed,
        Migrating,
        Dying,
        Dead
    }

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Renderer[] allRenderers;
    private Color originalColor;
    private bool isFleeing = false;
    private float wanderTimer = 0f;
    private float wanderInterval = 3f;
    private bool deathCoroutineStarted = false;

    public System.Action<MarineLife> OnDeath;
    public System.Action<MarineLife> OnMigrationStart;
    public System.Action<MarineLife> OnMigrationEnd;

    private void Start()
    {
        originalPosition = transform.position;
        targetPosition = originalPosition;
        allRenderers = GetComponentsInChildren<Renderer>();

        if (allRenderers != null && allRenderers.Length > 0 && allRenderers[0] != null)
        {
            originalColor = allRenderers[0].material.color;
        }

        EcologyManager.Instance?.RegisterMarineLife(this);
    }

    private void Update()
    {
        if (isDead) return;

        UpdateState();

        switch (state)
        {
            case LifeState.Healthy:
                Wander();
                break;
            case LifeState.Stressed:
                HandleStressed();
                break;
            case LifeState.Migrating:
                HandleMigration();
                break;
            case LifeState.Dying:
                HandleDying();
                break;
        }

        UpdateVisuals();
    }

    private void UpdateState()
    {
        if (currentHealth <= deathThreshold && !deathCoroutineStarted)
        {
            state = LifeState.Dying;
            return;
        }

        if (currentStressLevel > stressThreshold)
        {
            timeInStress += Time.deltaTime;

            if (canMigrate && currentStressLevel > migrationTriggerStress && !isMigrating)
            {
                state = LifeState.Migrating;
                StartMigration();
            }
            else if (!canMigrate || timeInStress > 10f)
            {
                state = LifeState.Stressed;
            }
            else
            {
                state = LifeState.Stressed;
            }
        }
        else
        {
            timeInStress = 0f;
            currentStressLevel = Mathf.Max(0f, currentStressLevel - stressRecoveryRate * Time.deltaTime);

            if (state != LifeState.Migrating)
            {
                currentHealth = Mathf.Min(maxHealth, currentHealth + healthRecoveryRate * Time.deltaTime);
                state = LifeState.Healthy;
            }
        }
    }

    private void HandleStressed()
    {
        currentHealth -= stressDamageRate * currentStressLevel * sedimentSensitivity * Time.deltaTime;

        if (canMigrate && currentStressLevel > migrationTriggerStress * 0.8f && !isMigrating)
        {
            state = LifeState.Migrating;
            StartMigration();
            return;
        }

        if (moveSpeed > 0f)
        {
            FleeFromDanger();
        }
    }

    private void HandleMigration()
    {
        isMigrating = true;
        currentHealth -= stressDamageRate * 0.3f * Time.deltaTime;

        float dist = Vector3.Distance(transform.position, migrationDestination);
        if (dist < 2f)
        {
            EndMigration();
            return;
        }

        float effectiveSpeed = migrationSpeed * (currentHealth / maxHealth);
        transform.position = Vector3.MoveTowards(transform.position, migrationDestination, effectiveSpeed * Time.deltaTime);
        transform.LookAt(migrationDestination);

        if (currentHealth <= deathThreshold)
        {
            state = LifeState.Dying;
        }
    }

    private void HandleDying()
    {
        if (!deathCoroutineStarted)
        {
            StartCoroutine(DieCoroutine());
        }
    }

    private void StartMigration()
    {
        isMigrating = true;
        OnMigrationStart?.Invoke(this);

        Vector3 awayFromSource = (transform.position - FindPlumeSourceDirection()).normalized;
        if (awayFromSource == Vector3.zero)
        {
            awayFromSource = Random.onUnitSphere;
            awayFromSource.y = Mathf.Abs(awayFromSource.y) * 0.3f;
        }

        migrationDestination = originalPosition + awayFromSource * Random.Range(migrationDistance * 0.5f, migrationDistance);
        migrationDestination.y = Mathf.Max(migrationDestination.y, originalPosition.y - 5f);

        if (terrainGenerator != null)
        {
            float terrainY = terrainGenerator.GetTerrainHeight(migrationDestination.x, migrationDestination.z);
            switch (lifeType)
            {
                case LifeType.SeaCucumber:
                case LifeType.Starfish:
                case LifeType.DeepSeaCrab:
                    migrationDestination.y = terrainY + 1f;
                    break;
                default:
                    migrationDestination.y = terrainY + Random.Range(5f, 15f);
                    break;
            }
        }
    }

    private void EndMigration()
    {
        isMigrating = false;
        originalPosition = transform.position;
        targetPosition = originalPosition;
        state = LifeState.Healthy;
        currentStressLevel *= 0.3f;
        OnMigrationEnd?.Invoke(this);
    }

    private Vector3 FindPlumeSourceDirection()
    {
        if (MiningManager.Instance == null) return Vector3.zero;

        Vector3 closestSource = Vector3.zero;
        float closestDist = float.MaxValue;

        MiningMachineController[] machines = FindObjectsOfType<MiningMachineController>();
        foreach (var machine in machines)
        {
            float dist = Vector3.Distance(transform.position, machine.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestSource = machine.transform.position;
            }
        }

        return closestSource;
    }

    public void UpdateStressLevel(float sedimentAmount)
    {
        float effectiveSediment = sedimentAmount * sedimentSensitivity;
        currentStressLevel = Mathf.Clamp01(currentStressLevel + effectiveSediment * Time.deltaTime * 2f);
    }

    public void ApplyStarvation(float intensity)
    {
        currentHealth -= starvationDamageRate * intensity * Time.deltaTime;
    }

    private void FleeFromDanger()
    {
        if (Vector3.Distance(transform.position, originalPosition) < fleeDistance)
        {
            Vector3 fleeDirection = (transform.position - originalPosition).normalized;
            if (fleeDirection == Vector3.zero) fleeDirection = Random.onUnitSphere;
            targetPosition = originalPosition + fleeDirection * fleeDistance * 2f;
            targetPosition.y += Random.Range(-3f, 3f);
        }

        float speedMultiplier = 1f + currentStressLevel;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * speedMultiplier * Time.deltaTime);
        transform.LookAt(targetPosition);
    }

    private void Wander()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval)
        {
            wanderTimer = 0f;
            wanderInterval = Random.Range(2f, 5f);

            Vector2 randomOffset = Random.insideUnitCircle * 5f;
            targetPosition = originalPosition + new Vector3(randomOffset.x, Random.Range(-2f, 2f), randomOffset.y);
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * 0.5f * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.LookAt(targetPosition);
        }
    }

    private void UpdateVisuals()
    {
        if (allRenderers == null || allRenderers.Length == 0) return;

        Color targetColor;
        float lerpSpeed = 2f * Time.deltaTime;

        switch (state)
        {
            case LifeState.Dying:
                targetColor = dyingColor;
                lerpSpeed = 1f * Time.deltaTime;
                break;
            case LifeState.Migrating:
                targetColor = Color.Lerp(normalColor, migratingColor, 0.5f);
                break;
            case LifeState.Stressed:
                float stressLerp = Mathf.Clamp01(currentStressLevel / stressThreshold);
                targetColor = Color.Lerp(normalColor, stressedColor, stressLerp);
                break;
            default:
                float healthRatio = currentHealth / maxHealth;
                targetColor = Color.Lerp(stressedColor, normalColor, healthRatio);
                break;
        }

        foreach (var r in allRenderers)
        {
            if (r != null && r.material != null)
            {
                r.material.color = Color.Lerp(r.material.color, targetColor, lerpSpeed);
            }
        }
    }

    private IEnumerator DieCoroutine()
    {
        deathCoroutineStarted = true;
        state = LifeState.Dying;

        OnDeath?.Invoke(this);

        if (fadeOnDeath)
        {
            float elapsed = 0f;
            while (elapsed < deathAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float fadeRatio = elapsed / deathAnimationDuration;

                foreach (var r in allRenderers)
                {
                    if (r != null && r.material != null)
                    {
                        Color c = dyingColor;
                        c.a = Mathf.Lerp(1f, 0f, fadeRatio);
                        r.material.color = c;
                    }
                }

                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.3f, fadeRatio);
                yield return null;
            }
        }

        isDead = true;
        state = LifeState.Dead;

        EcologyManager.Instance?.UnregisterMarineLife(this);
        FoodWebSystem.Instance?.OnCreatureDeath(this);

        Destroy(gameObject, 0.1f);
    }

    private SeabedTerrainGenerator terrainGenerator;
    public void SetTerrainGenerator(SeabedTerrainGenerator gen)
    {
        terrainGenerator = gen;
    }

    public float GetHealthRatio()
    {
        return currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return !isDead && state != LifeState.Dead && state != LifeState.Dying;
    }

    private void OnDestroy()
    {
        if (!isDead)
        {
            EcologyManager.Instance?.UnregisterMarineLife(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = state == LifeState.Migrating ? Color.yellow :
                       state == LifeState.Stressed ? Color.red :
                       state == LifeState.Dying ? Color.gray : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (isMigrating)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, migrationDestination);
            Gizmos.DrawSphere(migrationDestination, 0.5f);
        }
    }
}
