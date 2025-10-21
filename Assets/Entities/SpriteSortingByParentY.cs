using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSortingByParentY : MonoBehaviour
{
    [SerializeField] private Transform reference;
    [SerializeField] private int baseSortingOrder = 0;
    [SerializeField] private float yToOrderMultiplier = 100f;

    private SpriteRenderer spriteRenderer;
    private float lastReferenceY;
    private bool hasLastReferenceY;
    private int lastAppliedOrder;
    private bool hasLastAppliedOrder;

    private void Awake()
    {
        CacheRenderer();
        UpdateSortingOrder();
    }

    private void OnEnable()
    {
        CacheRenderer();
        UpdateSortingOrder();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheRenderer();
        UpdateSortingOrder();
    }
#endif

    private void LateUpdate()
    {
        UpdateSortingOrder();
    }

    private void CacheRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private Transform ResolveReference()
    {
        if (reference != null)
        {
            return reference;
        }

        Transform parent = transform.parent;
        return parent != null ? parent : transform;
    }

    private void UpdateSortingOrder()
    {
        CacheRenderer();
        if (spriteRenderer == null)
        {
            return;
        }

        Transform target = ResolveReference();
        if (target == null)
        {
            return;
        }

        float currentY = target.position.y;
        if (hasLastReferenceY && Mathf.Approximately(currentY, lastReferenceY) && hasLastAppliedOrder)
        {
            return;
        }

        int order = baseSortingOrder + Mathf.RoundToInt(-currentY * yToOrderMultiplier);
        if (!hasLastAppliedOrder || order != lastAppliedOrder)
        {
            spriteRenderer.sortingOrder = order;
            lastAppliedOrder = order;
            hasLastAppliedOrder = true;
        }

        lastReferenceY = currentY;
        hasLastReferenceY = true;
    }
}