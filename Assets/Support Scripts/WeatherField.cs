using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherField : MonoBehaviour
{
    public GameManager gameManager;
    public Aument aument;
    public List<CardData> cards = new List<CardData>();

    public void OnClick()
    {
        if (cards.Count < 3 && gameManager.selectedCard != null && !gameManager.selectedCard.card.isActive && gameManager.selectedCard.card.weather)
        {
            gameManager.playerTurn = 0;
            gameManager.selectedCard.transform.SetParent(transform);
            cards.Add(gameManager.selectedCard.card);
            gameManager.selectedCard.card.isActive = true;
            gameManager.selectedCard.gameObject.AddComponent<WtherSelCard>();
            gameManager.selectedCard.GetComponent<WtherSelCard>().decrs = gameManager.selectedCard.card.efectFactor;
            aument.card = gameManager.selectedCard.card;
            aument.wtherSelCard = gameManager.selectedCard.GetComponent<WtherSelCard>();
            aument.playEfectW = true;
        }
    }
}
