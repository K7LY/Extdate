using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Card Deck", menuName = "Card Game/Card Deck")]
public class CardDeck : ScriptableObject
{
    [Header("デッキ設定")]
    public List<Card> allCards = new List<Card>();
    public bool shuffleOnStart = true;
    
    [Header("ランタイムデータ")]
    [SerializeField] private List<Card> drawPile = new List<Card>();
    [SerializeField] private List<Card> discardPile = new List<Card>();
    
    // イベント
    public System.Action<Card> OnCardDrawn;
    public System.Action OnDeckShuffled;
    public System.Action OnDeckEmpty;
    
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public int TotalCards => DrawPileCount + DiscardPileCount;
    
    public void InitializeDeck()
    {
        // デッキをリセット
        drawPile.Clear();
        discardPile.Clear();
        
        // 全カードをドローパイルに追加
        drawPile.AddRange(allCards);
        
        // シャッフル
        if (shuffleOnStart)
        {
            ShuffleDeck();
        }
        
        Debug.Log($"デッキを初期化しました。カード数: {drawPile.Count}");
    }
    
    public Card DrawCard()
    {
        // ドローパイルが空の場合、捨て札をシャッフルして戻す
        if (drawPile.Count == 0)
        {
            ReshuffleDiscardPile();
        }
        
        // それでも空の場合
        if (drawPile.Count == 0)
        {
            OnDeckEmpty?.Invoke();
            return null;
        }
        
        // カードをドロー
        Card drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        
        OnCardDrawn?.Invoke(drawnCard);
        return drawnCard;
    }
    
    public List<Card> DrawCards(int count)
    {
        List<Card> drawnCards = new List<Card>();
        
        for (int i = 0; i < count; i++)
        {
            Card card = DrawCard();
            if (card != null)
            {
                drawnCards.Add(card);
            }
            else
            {
                break; // デッキが空になった
            }
        }
        
        return drawnCards;
    }
    
    public void DiscardCard(Card card)
    {
        if (card != null && !discardPile.Contains(card))
        {
            discardPile.Add(card);
        }
    }
    
    public void DiscardCards(List<Card> cards)
    {
        foreach (Card card in cards)
        {
            DiscardCard(card);
        }
    }
    
    public void ShuffleDeck()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            Card temp = drawPile[i];
            int randomIndex = Random.Range(i, drawPile.Count);
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
        
        OnDeckShuffled?.Invoke();
        Debug.Log("デッキをシャッフルしました");
    }
    
    private void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0)
            return;
            
        // 捨て札をドローパイルに戻す
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        
        // シャッフル
        ShuffleDeck();
        
        Debug.Log("捨て札をシャッフルしてドローパイルに戻しました");
    }
    
    public void AddCardToDeck(Card card)
    {
        if (card != null && !allCards.Contains(card))
        {
            allCards.Add(card);
            drawPile.Add(card);
        }
    }
    
    public void RemoveCardFromDeck(Card card)
    {
        allCards.Remove(card);
        drawPile.Remove(card);
        discardPile.Remove(card);
    }
    
    public Card PeekTopCard()
    {
        if (drawPile.Count > 0)
            return drawPile[0];
        return null;
    }
    
    public List<Card> PeekTopCards(int count)
    {
        List<Card> peekedCards = new List<Card>();
        int actualCount = Mathf.Min(count, drawPile.Count);
        
        for (int i = 0; i < actualCount; i++)
        {
            peekedCards.Add(drawPile[i]);
        }
        
        return peekedCards;
    }
    
    // デバッグ用
    public void PrintDeckState()
    {
        Debug.Log($"ドローパイル: {drawPile.Count}枚, 捨て札: {discardPile.Count}枚");
    }
}