using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Cementary : MonoBehaviour
{
    public int player;
    public List<CardData> exitingCards;
    public TMP_Text text;

    void Start ()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        exitingCards = player == 1? new List<CardData>(gameManager.playerDeck1.deck): new List<CardData>(gameManager.playerDeck2.deck);
    }
    void LateUpdate()
    {
        List<CardData> deadCards = new List<CardData>(exitingCards);
        List<string> cardsInGame = new List<string>();
        if (player == 1)
        {
            foreach (var card in Context.HandOfPlayer1)
            {
                cardsInGame.Add(card.cardName);
            }
            foreach (var card in Context.DeckOfPlayer1)
            {
                cardsInGame.Add(card.cardName);
            }
            foreach (var card in Context.FieldOfPlayer1)
            {
                cardsInGame.Add(card.cardName);
            }
        }
        else if (player == 2)
        {
            foreach (var card in Context.HandOfPlayer2)
            {
                cardsInGame.Add(card.cardName);
            }
            foreach (var card in Context.DeckOfPlayer2)
            {
                cardsInGame.Add(card.cardName);
            }
            foreach (var card in Context.FieldOfPlayer2)
            {
                cardsInGame.Add(card.cardName);
            }
        }
        foreach (var card in FindObjectOfType<WeatherField>().cards)
        {
            cardsInGame.Add(card.cardName);
        }

        foreach (var name in cardsInGame)
        {
            for (int i = 0; i < deadCards.Count; i++)
            {
                if (deadCards[i].cardName == name)
                {
                    deadCards.RemoveAt(i);
                    break;
                }
            }
        }

        if (player == 1)
        {
            Context.GraveyardOfPlayer1 = new List<CardData>(deadCards);
        }
        else if (player == 2)
        {
            Context.GraveyardOfPlayer2 = new List<CardData>(deadCards);
        }
        text.text = deadCards.Count.ToString();
    }
}
