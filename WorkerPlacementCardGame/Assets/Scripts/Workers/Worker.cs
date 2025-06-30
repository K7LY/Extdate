using UnityEngine;

public class Worker : MonoBehaviour
{
    [Header("ワーカー情報")]
    public Player owner;
    public ActionSpace currentActionSpace;
    
    [Header("ビジュアル")]
    public SpriteRenderer spriteRenderer;
    public Sprite workerSprite;
    
    void Awake()
    {
        // SpriteRendererを追加
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
        // Collider2Dを追加（クリック検出用）
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<CircleCollider2D>();
    }
    
    void Start()
    {
        UpdateVisual();
    }
    
    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
        UpdateVisual();
    }
    
    public void SetActionSpace(ActionSpace actionSpace)
    {
        currentActionSpace = actionSpace;
    }
    
    public void UpdateVisual()
    {
        if (owner != null && spriteRenderer != null)
        {
            spriteRenderer.color = owner.playerColor;
            if (workerSprite != null)
                spriteRenderer.sprite = workerSprite;
        }
    }
    
    public void ReturnToPlayer()
    {
        if (currentActionSpace != null)
        {
            currentActionSpace.RemoveWorker();
            currentActionSpace = null;
        }
        
        // ワーカーを適切な位置に戻す処理
        // （プレイヤーエリアなど）
    }
    
    void OnMouseDown()
    {
        // ワーカーがクリックされた時の処理
        // 選択状態の切り替えなど
    }
}