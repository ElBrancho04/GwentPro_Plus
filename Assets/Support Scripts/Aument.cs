using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Aument : MonoBehaviour
{
    public GameManager gameManager;
    public CardData card;
    public bool playEfectF;
    public bool playEfectC;
    public bool playEfectW;
    public File file;
    public CardData cardToPlayEfct;
    public int player;
    public WtherSelCard wtherSelCard;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        file = gameManager.fileToPlayEfct;
        if (card != null)
        player = card.player;
        if (card != null && card.efectFactor < 0)
        player = card.player%2 + 1;

        if (playEfectF && file != null && file.player == player && gameManager.confirm)
        {
            for (int i = 0; i < file.cards.Count; i++)
            {
                if (!file.cards[i].isGold)
                file.cards[i].powerVar += card.efectFactor;
            }
            playEfectF = false;
            gameManager.fileToPlayEfct = null;
            gameManager.confirm = false;
            gameManager.playerTurn = card.player%2 + 1;
        }

        cardToPlayEfct = gameManager.cardToPlayEfct;
        if (playEfectC && cardToPlayEfct != null && !cardToPlayEfct.isGold && cardToPlayEfct.player == player && gameManager.confirm) 
        {
           cardToPlayEfct.powerVar += gameManager.UIcardToPlayEfct.actPower;
           playEfectC = false;
           gameManager.cardToPlayEfct = null;
           gameManager.UIcardToPlayEfct = null;
           gameManager.confirm = false;
           gameManager.playerTurn = card.player%2 + 1;
        }

        if (playEfectW && file != null && file.player == player && gameManager.confirm)
        {
            wtherSelCard.file = file;

            wtherSelCard.file.decrs += card.efectFactor;
            playEfectW = false;
            gameManager.playerTurn = card.player%2 + 1;
            gameManager.fileToPlayEfct = null;
            gameManager.selectedCard = null; 
            gameManager.confirm = false;  
        }
    }
}
