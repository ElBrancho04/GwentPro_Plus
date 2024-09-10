using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CardUI selectedCard;
    public int playerTurn;
    public bool[] playerPass = new bool[2];
    public PlayerDeck playerDeck1;
    public PlayerDeck playerDeck2;
    public CardSpawner cardSpawner;
    public GameObject cardPrefab1;
    public GameObject cardPrefab2;
    public File playerHand1;
    public File playerHand2;
    public ShowCard showCard1;
    public ShowCard showCard2;
    public CardUI showedCard;
    public int roundsWon1;
    public int roundsWon2;
    public GameObject punto1P1;
    public GameObject punto2P1;
    public GameObject punto1P2;
    public GameObject punto2P2;
    public RoundWinnerCalculator roundWinnerCalculator;
    public File fileToPlayEfct;
    public CardUI UIcardToPlayEfct;
    public CardData cardToPlayEfct;
    public CardUI handUIcardToPlayEfct;
    public CardData handCardToPlayEfct;
    public File[] tablero;
    public GameObject pasar1;
    public GameObject pasar2;
    public GameObject lider1;
    public GameObject lider2;
    public int initialPlayerTurn;
    public int realTurn;
    public bool confirm;
    public GameObject confirmButton1;
    public GameObject confirmButton2;
    public GameObject ayuda1;
    public GameObject ayuda2;
    public int[] puntos;
    public TextMeshProUGUI puntosTotales1;
    public TextMeshProUGUI puntosTotales2;
    public GameObject turn1;
    public GameObject turn2;
    public GameObject winner1;
    public GameObject winner2;

    void Start()
    {
        puntos = new int[6];
    }

    void LateUpdate()
    {
        if (playerPass[0] == true && playerPass[1] == true)
        {
            roundWinnerCalculator.RoundWinnerCal();
            playerPass[0] = false;
            playerPass[1] = false;
            StartCoroutine(DrawCards(playerDeck1,playerDeck2));
        }
        if (playerTurn == 1)
        realTurn = 1;
        if (playerTurn == 2)
        realTurn = 2;
        StartCoroutine(Wait());
        IEnumerator Wait()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            if (playerTurn == 1)
            {
                pasar1.SetActive(true);
                lider1.SetActive(true);
                ayuda1.SetActive(true);
                pasar2.SetActive(false);
                lider2.SetActive(false);
                ayuda2.SetActive(false);
                realTurn = 1;
            }
            
            if (playerTurn == 2)
            {
                pasar2.SetActive(true);
                lider2.SetActive(true);
                ayuda2.SetActive(true);
                pasar1.SetActive(false);
                lider1.SetActive(false);
                ayuda1.SetActive(false);
                realTurn = 2;
            }
        }

        for (int i = 0; i < tablero.Length; i++)
        {
            for (int j = 0; j < tablero[i].cards.Count; j++)
            {
                if (tablero[i].cards[j].cardType == CardType.Special && playerTurn == tablero[i].cards[j].player%2 + 1)
                {
                    tablero[i].cards.Remove(tablero[i].cards[j]);
                    Destroy(tablero[i].transform.GetChild(j).gameObject);
                }
            }
        }

        if (playerTurn == 1 || playerTurn == 2)
        {
            cardToPlayEfct = handCardToPlayEfct = null;
            UIcardToPlayEfct = handUIcardToPlayEfct = null;
            fileToPlayEfct = null;
        }
        else
        selectedCard = null;
    }

        IEnumerator DrawCards(PlayerDeck playerDeck1, PlayerDeck playerDeck2)
    {
        cardSpawner.cardPrefab = cardPrefab1;
        cardSpawner.fileFile = playerHand1;
        cardSpawner.fieldTransform = playerHand1.transform;

        cardSpawner.SpawnCard(playerDeck1.DrawCard());

        cardSpawner.cardPrefab = cardPrefab2;
        cardSpawner.fileFile = playerHand2;
        cardSpawner.fieldTransform = playerHand2.transform;

        cardSpawner.SpawnCard(playerDeck2.DrawCard());
        yield return new WaitForSecondsRealtime(1f);
        cardSpawner.SpawnCard(playerDeck2.DrawCard());

        cardSpawner.cardPrefab = cardPrefab1;
        cardSpawner.fileFile = playerHand1;
        cardSpawner.fieldTransform = playerHand1.transform;

        cardSpawner.SpawnCard(playerDeck1.DrawCard());
    }
    
    void Update()
    {
        if (UIcardToPlayEfct != null)
        cardToPlayEfct = UIcardToPlayEfct.card;
        if (handUIcardToPlayEfct != null)
        handCardToPlayEfct = handUIcardToPlayEfct.card;
        if (showedCard != null && showedCard.card.player != showCard1.player)
        {
            showCard1.gameObject.SetActive(false);
            showCard2.gameObject.SetActive(true);
        }
        if (showedCard != null && showedCard.card.player != showCard2.player)
        {
            showCard2.gameObject.SetActive(false);
            showCard1.gameObject.SetActive(true);
        }
        if (showedCard == null)
        {
            showCard1.gameObject.SetActive(false);
            showCard2.gameObject.SetActive(false);
        }

        if(roundsWon1 == 1)
        punto1P1.SetActive(true);

        if(roundsWon1 == 2)
        punto2P1.SetActive(true);

        if(roundsWon2 == 1)
        punto1P2.SetActive(true);

        if(roundsWon2 == 2)
        punto2P2.SetActive(true);

        if (playerPass[0] && playerTurn == 1)
        playerTurn = 2;

        if (playerPass[1] && playerTurn == 2)
        playerTurn = 1;

        if (playerTurn == 0)
        {
            if (lider1.activeSelf)
            confirmButton1.SetActive(true);
            if (lider2.activeSelf)
            confirmButton2.SetActive(true);
        }
        else
        {
            confirmButton1.SetActive(false);
            confirmButton2.SetActive(false);
        }

        puntosTotales1.text = "" + (puntos[0] + puntos[1] + puntos[2]);
        puntosTotales2.text = "" + (puntos[3] + puntos[4] + puntos[5]);

        if (roundsWon1 == 2)
        {
            turn1.SetActive(false);
            turn2.SetActive(false);
            playerTurn = -3;
            winner1.SetActive(true);
        }
        if (roundsWon2 == 2)
        {
            turn1.SetActive(false);
            turn2.SetActive(false);
            playerTurn = -3;
            winner2.SetActive(true);
        }

        if (playerPass[0] == true && playerPass[1] == true)
        {
            roundWinnerCalculator.RoundWinnerCal();
            playerPass[0] = false;
            playerPass[1] = false;
            StartCoroutine(DrawCards(playerDeck1,playerDeck2));
        }
    }

    public void RemoveCard(GameObject card)
    {
        List<CardData> cardsFile = card.transform.parent.GetComponent<File>().cards;
        cardsFile.Remove(card.GetComponent<CardUI>().card);
        StartCoroutine(Wait(card));
    }

    IEnumerator Wait(GameObject card)
    {
        Destroy(card.GetComponent<CardUI>());
        yield return new WaitForSecondsRealtime(0.3f);
        Destroy(card);
    }

    public void Confirm(GameObject confirmButton)
    {
        confirm = true;
        confirmButton.SetActive(false);
    }

}
