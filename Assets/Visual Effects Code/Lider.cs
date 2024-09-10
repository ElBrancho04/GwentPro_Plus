using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lider : MonoBehaviour
{
    public int player;
    public GameManager gameManager;
    public DelCard delCard;
    public GameObject ayuda;

    public void OnClick()
    {
        if (player == 1 && gameManager.playerTurn == 1)
        {
            CardUI max1 = null;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    if (max1 == null || gameManager.tablero[i].transform.GetChild(j).GetComponent<CardUI>().actPower > max1.actPower)
                    {
                        max1 = gameManager.tablero[i].transform.GetChild(j).GetComponent<CardUI>();
                    }
                    if (gameManager.tablero[i].transform.GetChild(j).GetComponent<CardUI>().actPower == max1.actPower)
                    {
                        int w = Random.Range(1,3);
                        if (w == 2)
                        max1 = gameManager.tablero[i].transform.GetChild(j).GetComponent<CardUI>();
                    }
                }
            }

            CardUI max2 = null;
            for (int i = 3; i < 6; i++)
            {
                for (int j = 0; j < gameManager.tablero[i].cards.Count; j++)
                {
                    if (max2 == null || gameManager.tablero[i].transform.GetChild(j).GetComponent<CardUI>().actPower > max2.actPower)
                    {
                        max2 = gameManager.tablero[i].transform.GetChild(j).GetComponent<CardUI>();
                    }
                    if (gameManager.tablero[i].transform.GetChild(j).GetComponent<CardUI>().actPower == max2.actPower)
                    {
                        int w = Random.Range(1,3);
                        if (w == 2)
                        max2 = gameManager.tablero[i].transform.GetChild(j).GetComponent<CardUI>();
                    }
                }
            }
            if (max1 != null && max2 != null)
            {
                StartCoroutine(Wait1());
                IEnumerator Wait1()
                {
                    yield return new WaitForSecondsRealtime(0.5f);
                    max1.GetComponentInParent<File>().cards.Remove(max1.card);
                    Destroy(max1.gameObject);
                }
                StartCoroutine(Wait2());
                IEnumerator Wait2()
                {
                    yield return new WaitForSecondsRealtime(0.8f);
                    max2.GetComponentInParent<File>().cards.Remove(max2.card);
                    Destroy(max2.gameObject); 
                }
            }

        }

        if (player == 2 && gameManager.playerTurn == 2) 
        {
            bool thereAreCards = false;
            for (int i = 3; i < 6; i++)
            {
                if (gameManager.tablero[i].cards.Count > 0)
                thereAreCards = true;
            }
            if (thereAreCards)
            {
                gameManager.playerTurn = 0;
                delCard.fenrir = true;
            }
        }
    }

    public void ShowHelp()
    {
        ayuda.SetActive(!ayuda.activeSelf);
    }
}
