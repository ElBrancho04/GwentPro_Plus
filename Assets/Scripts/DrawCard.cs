using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DrawCard : MonoBehaviour
{
    public PlayerDeck deck;
    public File hand;
    public GameObject cardPrefab;
    public Transform handTransform;
    
    public void OnMouseClick(){
        CardData drawnCard = deck.DrawCard();
        hand.cards.Add(drawnCard);
        GameObject newCard = Instantiate(cardPrefab, handTransform);

        
        
        TextMeshProUGUI newCardName = newCard.transform.Find("Border/Name").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI newCardDescription = newCard.transform.Find("Border/Description").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI newCardPower = newCard.transform.Find("Border/AttackPower/AttackPowerText").GetComponent<TextMeshProUGUI>();

        newCardName.text = " " + drawnCard.cardName;
        newCardDescription.text = " " + drawnCard.cardDescription;
        newCardPower.text = " " + drawnCard.attackPower;
        

        
    }
}
