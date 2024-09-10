using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AumentZone : MonoBehaviour
{
    public GameManager gameManager;
    public File file;
    public CardData card;
    public int player;

    void Update()
    {
        for (int i = 0; i < file.cards.Count; i++)
        {
            if (card != null && !file.cards[i].isGold)
            file.cards[i].aumPower = card.efectFactor;
            else 
            file.cards[i].aumPower = 0;
        }
        
    }

    public void OnClick()
    {
        if (card == null && gameManager.playerTurn == player && gameManager.selectedCard != null && !gameManager.selectedCard.card.isActive && gameManager.selectedCard.card.player == player && !gameManager.playerPass[player - 1] && gameManager.selectedCard.card.aument)
        {
            gameManager.selectedCard.transform.SetParent(transform);
            card = gameManager.selectedCard.card;
            gameManager.selectedCard.card.isActive = true;
            gameManager.selectedCard = null;
            gameManager.playerTurn = card.player%2 + 1; 
        }
    }
}
