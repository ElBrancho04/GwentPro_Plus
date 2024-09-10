using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class File : MonoBehaviour
{
      public GameManager gameManager;
      public List<CardData> cards;   
      public bool melee = false;
      public bool ranged = false;
      public bool siege = false;
      public int player;
      public Transform fileTransform;
      public bool hand;
      public int decrs;
      public bool initialDevolution;
      public DelCard delCard;
      public GameObject initialDevolutionButton;
      public GameObject selected;
      public TextMeshProUGUI puntosEnFilaText;
      public int position;

      public void OnClick()
      {
            if (gameManager.playerTurn == 0 && !hand)
            gameManager.fileToPlayEfct = this;
            if (cards.Count < 10 && gameManager.selectedCard != null && !gameManager.selectedCard.card.isActive && gameManager.playerPass[player - 1] == false && player == gameManager.selectedCard.card.player && ((gameManager.selectedCard.card.melee == melee && melee == true )||(gameManager.selectedCard.card.ranged == ranged && ranged == true )||(gameManager.selectedCard.card.siege == siege && siege == true )))
            {
                  gameManager.selectedCard.transform.SetParent(fileTransform);
                  cards.Add(gameManager.selectedCard.card);
                  gameManager.selectedCard.card.isActive = gameManager.selectedCard.card.playEfect = true;
                  gameManager.selectedCard = null;
            }
      }
      void Update()
      {
            for (int i = 0; i < cards.Count; i++)
            {
                  if (cards[i].isActive == hand)
                  {
                        cards.RemoveAt(i);
                  }
            }

            if (hand && !initialDevolution && gameManager.playerTurn == player)
            {
                  StartCoroutine(Wait());
                  IEnumerator Wait()
                  {
                        yield return null;
                        gameManager.playerTurn = 0;
                        if (player == 1)
                        gameManager.pasar1.SetActive(false);
                        else
                        gameManager.pasar2.SetActive(false);
                  }
                  initialDevolutionButton.SetActive(true);
                  delCard.player = player;
                  delCard.initialDevolution = true;
                  initialDevolution = true;
            }

            if (gameManager.fileToPlayEfct == this)
            selected.SetActive(true);
            else
            selected.SetActive(false);

            if (!hand)
            {
                  int puntosEnFila = 0;
                  for (int i = 0; i < cards.Count; i++)
                  {
                        puntosEnFila += cards[i].actPower;
                  }
                  puntosEnFilaText.text = "" + puntosEnFila;
                  gameManager.puntos[position] = puntosEnFila;
            }
            if (gameManager.playerPass[player - 1] && hand)
            {
                  initialDevolution = true;
                  delCard.initialDevolution = false;
                  initialDevolutionButton.SetActive(false);
            }
      }
}
