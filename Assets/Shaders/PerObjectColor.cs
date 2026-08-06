using UnityEngine;

public class PerObjectColor : MonoBehaviour
{
    public Color customColor = Color.white;
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        UpdateColor();
    }
    [ContextMenu("ChangeCOlor")]
    void UpdateColor()
    {
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_BaseColor", customColor);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateColor();
        }
    }
#endif
}
