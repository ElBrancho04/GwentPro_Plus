using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Context : MonoBehaviour
{
    public GameManager gameManager;
    public static int TriggerPlayer;
    public static List<CardData> Board;
    public static List<CardData> HandOfPlayer1;
    public static List<CardData> HandOfPlayer2;
    public static List<CardData> FieldOfPlayer1;
    public static List<CardData> FieldOfPlayer2;
    public static List<CardData> GraveyardOfPlayer1;
    public static List<CardData> GraveyardOfPlayer2;
    public static List<CardData> DeckOfPlayer1;
    public static List<CardData> DeckOfPlayer2;
    public List<CreatedEffect> CreatedEffects;
    public List<string> cardNames;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        TriggerPlayer = gameManager.realTurn;
        HandOfPlayer1 = gameManager.playerHand1.cards;
        HandOfPlayer2 = gameManager.playerHand2.cards;
        DeckOfPlayer1 = gameManager.playerDeck1.deck;
        DeckOfPlayer2 = gameManager.playerDeck2.deck;
        CreatedEffects = new List<CreatedEffect>();
        FieldOfPlayer1 = new List<CardData>();
        FieldOfPlayer2 = new List<CardData>();
        Board = new List<CardData>();
    }

    void Update()
    {
        var tablero = gameManager.tablero;
        for (int i = 0; i < 3; i++)
        {
            foreach (var item in tablero[i].cards)
            {
                FieldOfPlayer1.Add(item);
                Board.Add(item);
            }
        }
        for (int i = 3; i < 6; i++)
        {
            foreach (var item in tablero[i].cards)
            {
                FieldOfPlayer2.Add(item);
                Board.Add(item);
            }
        }
    }
}
