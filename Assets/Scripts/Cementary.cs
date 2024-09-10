using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Cementary : MonoBehaviour
{
    public GameManager gameManager;
    public int player;
    public AumentZone m;
    public AumentZone r;
    public AumentZone s;
    public WeatherField weatherField;
    public TextMeshProUGUI cant;

    void Update()
    {
        int cardsInField = 0;
        int cardsInCementary = 0;

        if (m.card != null)
        cardsInField++;
        if (r.card != null)
        cardsInField++;
        if (s.card != null)
        cardsInField++;

        for (int i = 0; i < weatherField.cards.Count; i++)
        {
            if (weatherField.cards[i].player == player)
            cardsInField++;
        }

        if (player == 1)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    cardsInField++;
                }
            }
            for (int i = 0; i < gameManager.playerHand1.cards.Count; i++)
            {
                cardsInField++;
            }

            gameManager.roundWinnerCalculator.cardsInField1 = cardsInField - gameManager.playerHand1.cards.Count;
            cardsInCementary = 31 - gameManager.playerDeck1.deck.Count - cardsInField;
        }

        if (player == 2)
        {
            for (int i = 3; i < 6; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    cardsInField++;
                }
            }
            for (int i = 0; i < gameManager.playerHand2.cards.Count; i++)
            {
                cardsInField++;
            }

            gameManager.roundWinnerCalculator.cardsInField2 = cardsInField - gameManager.playerHand2.cards.Count;
            cardsInCementary = 31 - gameManager.playerDeck2.deck.Count - cardsInField;
        }

        cant.text = "" + cardsInCementary;
    }
}
