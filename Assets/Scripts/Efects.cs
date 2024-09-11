using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Efects : MonoBehaviour
{
    public GameManager gameManager;
    public Aument aument;
    public DelCard delCard;
    public WeatherField weatherField;
    public GameObject selectACard;
    public GameObject selectAFile;
    public GameObject selectAnAllyCard;
    public GameObject selectAnEnemyCard;
    public GameObject selectAnAllyFile;
    public GameObject selectAnEnemyFile;
    public GameObject selectAWeather;
    public GameObject decoy;

    void Start()
    {
        aument = FindAnyObjectByType<Aument>();
        delCard = FindAnyObjectByType<DelCard>();
        weatherField = FindAnyObjectByType<WeatherField>();
    }

    public void PlayEfect(CardData card)
    {
        StartCoroutine(Wait());
        IEnumerator Wait(){ yield return new WaitForSecondsRealtime(0.2f);

        int efectFactor = card.efectFactor;
        int cardID = card.id;
        gameManager.fileToPlayEfct = null;
        gameManager.UIcardToPlayEfct = null;
        gameManager.cardToPlayEfct = null;
        aument.card = card;
        delCard.card = card;
        if (cardID == -1)
        {
            Evaluator evaluator = new Evaluator((CreatedCard)card, FindAnyObjectByType<Context>());
            evaluator.Evaluate();
            gameManager.playerTurn = 2;
        }
        if (cardID == 3 || cardID == 8 || cardID == 18 || cardID == 23 || cardID == 6 || cardID == 22) //Aumentar una fila
        {
            gameManager.playerTurn = 0;
            selectAnAllyFile.SetActive(true);
            aument.playEfectF = true; 
        }

        if (cardID == 1 || cardID == 17) //Duplicar el poder de una carta
        {
            bool thereAreSilver = false;
            for (int i = 0; i < gameManager.tablero.Length; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    if (gameManager.tablero[i].player == card.player && !gameManager.tablero[i].cards[j].isGold)
                    {
                        thereAreSilver = true;
                        break;
                    }
                    if (thereAreSilver)
                    break;
                }
            }
            if (thereAreSilver)
            {
                gameManager.playerTurn = 0;
                selectAnAllyCard.SetActive(true);
                aument.playEfectC = true;
            }
            else
            gameManager.playerTurn = card.player%2 + 1;
        }

        if (cardID == 2 || cardID == 16) //Eliminar la carta con mas poder del campo
        {
            CardUI cardUI = null;
            List<CardUI> list = new List<CardUI>(){cardUI};
            
            for (int i = 0; i < gameManager.tablero.Length; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    if (list[0] == null || gameManager.tablero[i].cards[j].actPower > list[0].actPower)
                    {
                        list = new List<CardUI>(){gameManager.tablero[i].transform.GetChild(j).gameObject.GetComponent<CardUI>()};
                    }
                    else if (gameManager.tablero[i].cards[j].actPower == list[0].actPower)
                    {
                        list.Add(gameManager.tablero[i].transform.GetChild(j).gameObject.GetComponent<CardUI>());
                    }
                }
            }
            if (list.Count > 1)
            {
                gameManager.playerTurn = 0;
                selectACard.SetActive(true);
                delCard.cardList = list;
                delCard.delCard = true;
            }
            else if (list.Count == 1)
            {
                list[0].transform.GetComponentInParent<File>().cards.Remove(list[0].card);
                Destroy(list[0].gameObject);
                gameManager.playerTurn = card.player%2 + 1;  
            } 
        }

        if (cardID == 4 || cardID == 19) //Igualar el poder de las cartas al promedio
        {
            int totalPower = 0;
            int cards = 0;
            int average = 0;
            for (int i = 0; i < gameManager.tablero.Length; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    totalPower += gameManager.tablero[i].cards[j].actPower;
                    cards++;
                    average = totalPower/cards;
                }
            }

            for (int i = 0; i < gameManager.tablero.Length; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    if(!gameManager.tablero[i].cards[j].isGold)
                    gameManager.tablero[i].cards[j].powerVar = average - gameManager.tablero[i].cards[j].attackPower;
                }
            }
            gameManager.playerTurn = card.player%2 + 1;
        }

        if (cardID == 5 || cardID == 20 || cardID == 25) //Robar dos cartas
        {
            if (card.player == 1)
            {
                StartCoroutine(DrawCards(gameManager.playerDeck1));
                IEnumerator DrawCards(PlayerDeck playerDeck)
                {
                    gameManager.cardSpawner.cardPrefab = gameManager.cardPrefab1;
                    gameManager.cardSpawner.fileFile = gameManager.playerHand1;
                    gameManager.cardSpawner.fieldTransform = gameManager.playerHand1.transform;

                    yield return new WaitForSecondsRealtime(1f);
                    gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());
                    yield return new WaitForSecondsRealtime(1f);
                    gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());
                }
            }

            if (card.player == 2)
            {
                StartCoroutine(DrawCards(gameManager.playerDeck2));
                IEnumerator DrawCards(PlayerDeck playerDeck)
                {
                    gameManager.cardSpawner.cardPrefab = gameManager.cardPrefab2;
                    gameManager.cardSpawner.fileFile = gameManager.playerHand2;
                    gameManager.cardSpawner.fieldTransform = gameManager.playerHand2.transform;

                    yield return new WaitForSecondsRealtime(1f);
                    gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());
                    if (card.efectFactor == 2){
                    yield return new WaitForSecondsRealtime(1f);
                    gameManager.cardSpawner.SpawnCard(playerDeck.DrawCard());}
                }
            }
            gameManager.playerTurn = card.player%2 + 1;
        }

        if (cardID == 7 || cardID == 21) //Eliminar la carta con menos poder del campo rival
        {
            CardUI cardUI = null;
            List<CardUI> list = new List<CardUI>(){cardUI};
            
            for (int i = 0; i < gameManager.tablero.Length; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    if (gameManager.tablero[i].player == card.player%2 + 1 && (list[0] == null || gameManager.tablero[i].cards[j].actPower < list[0].actPower))
                    {
                        list = new List<CardUI>(){gameManager.tablero[i].transform.GetChild(j).gameObject.GetComponent<CardUI>()};
                    }
                    else if (gameManager.tablero[i].player == card.player%2 + 1 && gameManager.tablero[i].cards[j].actPower == list[0].actPower)
                    {
                        list.Add(gameManager.tablero[i].transform.GetChild(j).gameObject.GetComponent<CardUI>());
                    }
                }
            }
            if (list.Count > 1)
            {
                gameManager.playerTurn = 0;
                selectAnEnemyCard.SetActive(true);
                delCard.cardList = list;
                delCard.delCard = true;
            }
            else if (list.Count == 1 && list[0] != null)
            {
                list[0].transform.GetComponentInParent<File>().cards.Remove(list[0].card);
                Destroy(list[0].gameObject);
                gameManager.playerTurn = card.player%2 + 1;  
            }
            else
            gameManager.playerTurn = card.player%2 + 1;
        }

        if (cardID == 9 || cardID == 24) //Limpiar la fila con menos unidades
        {
            File file = null;
            List<File> list = new List<File>(){file};
            for (int i = 0; i < gameManager.tablero.Length; i++)
            {  
                if (gameManager.tablero[i].cards.Count > 0 && (list[0] == null || gameManager.tablero[i].cards.Count < list[0].cards.Count))
                {
                    list = new List<File>(){gameManager.tablero[i]};
                }
                else if (list[0] != null && gameManager.tablero[i].cards.Count == list[0].cards.Count)
                {
                    list.Add(gameManager.tablero[i]);
                }
            }
            if (list.Count > 1)
            {
                gameManager.playerTurn = 0;
                selectAFile.SetActive(true);
                delCard.fileList = list;
                delCard.delFile = true;
            }
            else if (list.Count == 1)
            {
                int count = list[0].cards.Count;
                list[0].cards.Clear();
                gameManager.playerTurn = card.player%2 + 1;
                for (int i = 0; i < count; i++)
                {
                    Destroy(list[0].transform.GetChild(i).gameObject);
                }
            }
            
        }

         if (cardID == 10) //Multiplicar power por cant de cartas iguales
        {
            int aument = -1;
            for (int i = 0; i < gameManager.tablero[0].cards.Count; i++)
            {
                if (gameManager.tablero[0].cards[i].id == 10)
                {
                    aument++;
                }
            }

            card.powerVar += 4*aument;
            gameManager.playerTurn = card.player%2 + 1;
        }

        if (cardID == 11 || cardID == 26) //Eliminar un clima enemigo
        {
            CardUI cardUI = null; 
            List<CardUI> list = new List<CardUI>(){cardUI};
            for (int i = 0; i < weatherField.cards.Count; i++)
            {
                if (weatherField.cards[i].player != card.player)
                {
                    if (list[0] == null)
                    list[0] = weatherField.transform.GetChild(i).gameObject.GetComponent<CardUI>();
                    else
                    list.Add(weatherField.transform.GetChild(i).gameObject.GetComponent<CardUI>());
                }
            }

            if (list.Count > 1)
            {
                gameManager.playerTurn = 0;
                selectAWeather.SetActive(true);
                delCard.cardList = list;
                delCard.delWeather = true;
            }
            else if (list[0] != null)
            {
                list[0].transform.GetComponentInParent<WeatherField>().cards.Remove(list[0].card);
                Destroy(list[0].gameObject);
                gameManager.playerTurn = card.player%2 + 1;
            }
            else
            gameManager.playerTurn = card.player%2 + 1;
        }

        if (cardID == 15 || cardID == 30) //Señuelo
        {
            bool thereAreUnits = false;
            bool thereAreCardsInHand = false;
            for (int i = 0; i < gameManager.tablero.Length; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    if (gameManager.tablero[i].player == card.player && gameManager.tablero[i].cards[j].cardType == CardType.Unit)
                    {
                        thereAreUnits = true;
                        break;
                    }
                    if (thereAreUnits)
                    break;
                }
            }

            if ((card.player == 1 && gameManager.playerHand1.cards.Count > 0)||(card.player == 2 && gameManager.playerHand2.cards.Count > 0))
            {
                thereAreCardsInHand = true;
            }

            if (thereAreUnits && thereAreCardsInHand)
            {
                gameManager.playerTurn = 0;
                decoy.SetActive(true);
                delCard.card = card;
                delCard.change = true;
            }
            else
            gameManager.playerTurn = card.player%2 + 1;
        }

    }} 
}
