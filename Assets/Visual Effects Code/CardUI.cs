using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public GameManager gameManager;
    public CardData card;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI attackPowerText;
    public bool isSelected;
    public GameObject melee;
    public GameObject ranged;
    public GameObject siege;
    public GameObject gold;
    public int actPower;
    public GameObject back;
    public GameObject selected;
    public Image image;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (card.cardImage != null)
        image.sprite = card.cardImage;
        nameText.text = "" + card.cardName;
        descriptionText.text = "" + card.cardDescription;
        if (card.cardType == CardType.Unit)
        {
            
        melee.SetActive(card.melee);
        ranged.SetActive(card.ranged);
        siege.SetActive(card.siege);
        card.actPower = actPower = card.attackPower + card.powerVar + card.aumPower + card.decrs;
        attackPowerText.text = "" + actPower;
        }
        
        if (gameManager.selectedCard != this)
        {
            isSelected = false;
        }
        gold.SetActive(card.isGold);  


        if(card.playEfect)
        {
            Efects efects = FindAnyObjectByType<Efects>();
            efects.PlayEfect(card);
            card.playEfect = false;
        }

        if (gameManager.playerTurn == card.player && !card.isActive)
        back.SetActive(false);
        if (gameManager.playerTurn == card.player%2 + 1 && !card.isActive)
        back.SetActive(true);

        if (card.isActive && gameObject.GetComponent<IsActive>() == null)
        gameObject.AddComponent<IsActive>();
        else if (!card.isActive)
        Destroy(gameObject.GetComponent<IsActive>());

        if (gameManager.selectedCard == this || gameManager.cardToPlayEfct == card || gameManager.handCardToPlayEfct == card)
        selected.SetActive(true);
        else
        selected.SetActive(false);
    }
    public void OnClick()
    {
        if (gameManager.playerTurn == 0 && card.isActive)
        gameManager.UIcardToPlayEfct = this;
        if (gameManager.playerTurn == 0 && !card.isActive)
        gameManager.handUIcardToPlayEfct = this;
        if (card.player == gameManager.playerTurn && gameManager.playerPass[card.player - 1] == false)
        {

        isSelected = true;
        gameManager.selectedCard = this;
        }
        if (card.player == gameManager.realTurn || card.isActive)
        {
            gameManager.showedCard = this; 
        }    
    }

    void LateUpdate()
    {
        if (!card.isActive)
        {
            card.aumPower = card.powerVar = card.decrs = 0;
        }
        if (card.cardType == CardType.Special)
        actPower = 0;
    }
}
        
    

