using UnityEngine;

public class MiningMachineController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public float hoverHeight = 2f;
    public LayerMask terrainLayer;

    [Header("Mining Settings")]
    public float miningRange = 5f;
    public float collectionRadius = 3f;
    public Transform collectionPoint;

    [Header("References")]
    public ParticleSystem dustTrail;
    public ParticleSystem miningEffect;
    public GameObject lights;

    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isMining = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        MiningManager.Instance?.RegisterMiningMachine(this);
    }

    private void Update()
    {
        HandleInput();
        UpdateTerrainFollow();
        UpdateMining();
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        if (movement.magnitude > 0.01f)
        {
            isMoving = true;
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 0.1f);
        }
        else
        {
            isMoving = false;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            isMining = true;
        }
        else
        {
            isMining = false;
        }

        UpdateParticleEffects();
    }

    private void UpdateTerrainFollow()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 10f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f, terrainLayer))
        {
            Vector3 targetPos = hit.point + Vector3.up * hoverHeight;
            transform.position = Vector3.Lerp(transform.position, targetPos, 2f * Time.deltaTime);

            Vector3 surfaceNormal = hit.normal;
            Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, surfaceNormal) * Quaternion.Euler(0, transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 2f * Time.deltaTime);
        }
    }

    private void UpdateMining()
    {
        if (!isMining || collectionPoint == null) return;

        float adjustedMiningRange = miningRange * GameManager.Instance.miningRate;
        Collider[] hitColliders = Physics.OverlapSphere(collectionPoint.position, adjustedMiningRange);

        foreach (Collider collider in hitColliders)
        {
            NoduleController nodule = collider.GetComponent<NoduleController>();
            if (nodule != null && !nodule.isCollected)
            {
                float distance = Vector3.Distance(collectionPoint.position, nodule.transform.position);
                if (distance < collectionRadius)
                {
                    nodule.StartCollection();
                }
            }
        }

        if (miningEffect != null)
        {
            var emission = miningEffect.emission;
            emission.rateOverTime = adjustedMiningRange * 20f;
        }
    }

    private void UpdateParticleEffects()
    {
        if (dustTrail != null)
        {
            var emission = dustTrail.emission;
            emission.rateOverTime = isMoving ? 10f : 2f;
        }
    }

    public bool IsMining()
    {
        return isMining;
    }

    public float GetMiningIntensity()
    {
        return isMining ? GameManager.Instance.miningRate : 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (collectionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(collectionPoint.position, miningRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(collectionPoint.position, collectionRadius);
        }
    }
}
