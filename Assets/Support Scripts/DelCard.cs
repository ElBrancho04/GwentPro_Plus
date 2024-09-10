using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DelCard : MonoBehaviour
{
    public GameManager gameManager;
    public Efects efects;
    public CardData card;
    public List<CardUI> cardList;
    public List<File> fileList;
    public bool delCard;
    public bool delFile;
    public bool delWeather;
    public bool change;
    public bool fenrir;
    public bool initialDevolution;
    public bool draw;
    public CardData cardToPlayEfct;
    public File fileToPlayEfct;
    public int cardsToDraw;
    public int player;

    void Start()
    {
        cardsToDraw = 0;
    }


    void Update()
    {
        cardToPlayEfct = gameManager.cardToPlayEfct;

        if (delCard && cardToPlayEfct != null && gameManager.confirm) 
        {
            for (int i = 0; i < cardList.Count; i++)
            {
                if (cardList[i].card == cardToPlayEfct)
                {
                    cardList[i].transform.GetComponentInParent<File>().cards.Remove(cardList[i].card);
                    Destroy(cardList[i].gameObject);
                    delCard = false;
                    gameManager.cardToPlayEfct = null;
                    gameManager.UIcardToPlayEfct = null;
                    gameManager.playerTurn = card.player%2 + 1;
                    break;
                }
            }
            gameManager.confirm = false;
        }

        fileToPlayEfct = gameManager.fileToPlayEfct;

        if (delFile && fileToPlayEfct != null && gameManager.confirm)
        {
            for (int i = 0; i < fileList.Count; i++)
            {
                if (fileList[i] == fileToPlayEfct)
                {
                    for (int j = 0; j < fileList[i].cards.Count; j++)
                    {
                        StartCoroutine(Wait());
                        IEnumerator Wait(){yield return new WaitForSecondsRealtime(0.4f);}
                        Destroy(fileList[i].transform.GetChild(j).gameObject);
                    }
                    fileList[i].cards.Clear();
                    delFile = false;
                    gameManager.fileToPlayEfct = null;
                    gameManager.playerTurn = card.player%2 + 1;
                    break;
                }
            }
            gameManager.confirm = false;
        }

        if (delWeather && cardToPlayEfct != null && gameManager.confirm)
        {
            for (int i = 0; i < cardList.Count; i++)
            {
                if (cardList[i].card == cardToPlayEfct)
                {
                    cardList[i].transform.GetComponentInParent<WeatherField>().cards.Remove(cardList[i].card);
                    Destroy(cardList[i].gameObject);
                    delWeather = false;
                    gameManager.cardToPlayEfct = null;
                    gameManager.UIcardToPlayEfct = null;
                    gameManager.playerTurn = card.player%2 + 1;
                    break; 
                }
            }
            gameManager.confirm = false;
        }

        if (change && gameManager.cardToPlayEfct != null && gameManager.handCardToPlayEfct != null && card.player == gameManager.cardToPlayEfct.player && gameManager.handCardToPlayEfct.player == card.player && gameManager.confirm)
        {
            int file = 0;
            for (int i = 0; i < gameManager.tablero.Length; i++)
            {
                if (gameManager.tablero[i] == gameManager.UIcardToPlayEfct.GetComponentInParent<File>())
                {
                    file = i;
                    break;
                }
            }

            gameManager.tablero[file].cards.Remove(gameManager.cardToPlayEfct);
            gameManager.handUIcardToPlayEfct.GetComponentInParent<File>().cards.Add(gameManager.cardToPlayEfct);
            gameManager.cardToPlayEfct.isActive = false;
            gameManager.UIcardToPlayEfct.transform.SetParent(gameManager.handUIcardToPlayEfct.GetComponentInParent<File>().transform);

            gameManager.handUIcardToPlayEfct.GetComponentInParent<File>().cards.Remove(gameManager.handCardToPlayEfct);
            gameManager.tablero[file].cards.Add(gameManager.handCardToPlayEfct);
            gameManager.handCardToPlayEfct.isActive = true; 
            gameManager.handUIcardToPlayEfct.transform.SetParent(gameManager.tablero[file].transform);

            change = false;
            gameManager.cardToPlayEfct =  gameManager.handCardToPlayEfct = null;
            gameManager.UIcardToPlayEfct = gameManager.handUIcardToPlayEfct = null;
            gameManager.confirm = false;
            gameManager.playerTurn = card.player%2 + 1; 
        }

        if (fenrir && gameManager.cardToPlayEfct != null && gameManager.cardToPlayEfct.player == 2 && gameManager.confirm)
        {
            gameManager.UIcardToPlayEfct.GetComponentInParent<File>().cards.Remove(gameManager.cardToPlayEfct);
            Destroy(gameManager.UIcardToPlayEfct.gameObject);

            StartCoroutine(DrawCards(gameManager.playerDeck2));
            IEnumerator DrawCards(PlayerDeck playerDeck)
            {
                gameManager.cardSpawner.cardPrefab = gameManager.cardPrefab2;
                gameManager.cardSpawner.fileFile = gameManager.playerHand2;
                gameManager.cardSpawner.fieldTransform = gameManager.playerHand2.transform;

                yield return new WaitForSecondsRealtime(1f);
                gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());
                yield return new WaitForSecondsRealtime(1f);
                gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());
            }

            fenrir = false;
            gameManager.cardToPlayEfct = null;
            gameManager.UIcardToPlayEfct = null;
            gameManager.confirm = false;
            gameManager.playerTurn = 1;
        }
        if (cardsToDraw == 2)
        {
            initialDevolution = false;
            draw = true;
        }
        

        if (initialDevolution && gameManager.handCardToPlayEfct != null && gameManager.handCardToPlayEfct.player == player && gameManager.confirm)
        {
            if (player == 1)
            {
                gameManager.playerDeck1.deck.Add(gameManager.handCardToPlayEfct);
                gameManager.playerHand1.cards.Remove(gameManager.handCardToPlayEfct);
                Destroy(gameManager.handUIcardToPlayEfct.gameObject);
                gameManager.handUIcardToPlayEfct = null;
                gameManager.handCardToPlayEfct = null;
                cardsToDraw++;
                gameManager.confirm = false;
            }
            else
            {
                gameManager.playerDeck2.deck.Add(gameManager.handCardToPlayEfct);
                gameManager.playerHand2.cards.Remove(gameManager.handCardToPlayEfct);
                Destroy(gameManager.handUIcardToPlayEfct.gameObject);
                gameManager.handUIcardToPlayEfct = null;
                gameManager.handCardToPlayEfct = null;
                cardsToDraw++;
                gameManager.confirm = false;
            }
        }

        if (draw)
        {
            if (player == 1)
            {
                StartCoroutine(DrawCards(gameManager.playerDeck1));
                IEnumerator DrawCards(PlayerDeck playerDeck)
                {
                    gameManager.cardSpawner.cardPrefab = gameManager.cardPrefab1;
                    gameManager.cardSpawner.fileFile = gameManager.playerHand1;
                    gameManager.cardSpawner.fieldTransform = gameManager.playerHand1.transform;

                    if (cardsToDraw > 0)
                    {
                        int cardsToDrawV =  cardsToDraw;
                        yield return new WaitForSecondsRealtime(1f);
                        gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());
                        if ( cardsToDrawV == 2){
                        yield return new WaitForSecondsRealtime(1f);
                        gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());}
                    }
                }
                gameManager.playerHand1.initialDevolutionButton.SetActive(false);
            }

            if (player == 2)
            {
                StartCoroutine(DrawCards(gameManager.playerDeck2));
                IEnumerator DrawCards(PlayerDeck playerDeck)
                {
                    gameManager.cardSpawner.cardPrefab = gameManager.cardPrefab2;
                    gameManager.cardSpawner.fileFile = gameManager.playerHand2;
                    gameManager.cardSpawner.fieldTransform = gameManager.playerHand2.transform;

                    if (cardsToDraw > 0)
                    {
                        int cardsToDrawV = cardsToDraw;
                        yield return new WaitForSecondsRealtime(1f);
                        gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());
                        if (cardsToDrawV == 2){
                        yield return new WaitForSecondsRealtime(1f);
                        gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());}
                    }
                }
                gameManager.playerHand2.initialDevolutionButton.SetActive(false);
            }

            draw = false;
            initialDevolution = false;
            cardsToDraw = 0;
            gameManager.playerTurn = player;
            player = 0;
        }
    }

    public void Draw()
    {
        draw = true;
        initialDevolution = false;
    }
   
}
