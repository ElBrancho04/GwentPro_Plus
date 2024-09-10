using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public static List<CardData> cards = new List<CardData>();
    public List<CardData> deck = new List<CardData>();
    public int deckSize;
    public GameObject cardBack;

     public CardData DrawCard() {
            if (deck.Count > 0) {
                CardData drawnCard = deck[0];
                deck.RemoveAt(0);
                return drawnCard;
            }
            else
            {
                return null;
            }
     }
    void Shuffle()
    {
        CardData temp;
        int randomIndex;
        int count = cards.Count;
        for (int i = 0; i < count; i++)
        {
            randomIndex = Random.Range(0, cards.Count);
            temp = Instantiate(cards[randomIndex]);
            deck.Add(temp);
            cards.RemoveAt(randomIndex);
        }
    }
    void Start()
    {
        cards = new List<CardData>(deck);
        deck.Clear();
        Shuffle();  
    }

    void Update()
    {
       deckSize = deck.Count; 
       cardBack.GetComponentInChildren<TextMeshProUGUI>().text = deck.Count.ToString();
    }
}
