using UnityEngine;

public class NoduleController : MonoBehaviour
{
    [Header("Nodule Properties")]
    public float collectionTime = 2f;
    public int mineralValue = 10;
    public bool isCollected = false;

    [Header("Visuals")]
    public Color collectedColor = new Color(0.2f, 0.2f, 0.2f);

    private Renderer noduleRenderer;
    private Color originalColor;
    private float collectionProgress = 0f;

    private void Start()
    {
        noduleRenderer = GetComponent<Renderer>();
        if (noduleRenderer != null)
        {
            originalColor = noduleRenderer.material.color;
        }
    }

    public void StartCollection()
    {
        if (isCollected) return;
        collectionProgress += Time.deltaTime;

        if (noduleRenderer != null)
        {
            float colorLerp = collectionProgress / collectionTime;
            noduleRenderer.material.color = Color.Lerp(originalColor, collectedColor, colorLerp);
        }

        if (collectionProgress >= collectionTime)
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        if (noduleRenderer != null)
        {
            noduleRenderer.material.color = collectedColor;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        MiningManager.Instance?.AddCollectedNodule(mineralValue);

        Destroy(gameObject, 0.5f);
    }

    public void ResetCollection()
    {
        collectionProgress = 0f;
        if (noduleRenderer != null)
        {
            noduleRenderer.material.color = originalColor;
        }
    }
}
