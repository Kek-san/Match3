using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Gem : MonoBehaviour {
    public GemTypeSO type;

    public void SetType(GemTypeSO type) {
        this.type = type;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = type.sprite;
        spriteRenderer.color = type.color;
        
    }

    public GemTypeSO GetType() => type;

    public void DestroyGem() => Destroy(gameObject);

}
