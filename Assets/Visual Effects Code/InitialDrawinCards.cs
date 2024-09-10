





using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dra : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerDeck deck;
    public File playerHand;
    public Transform playerHandTransform;
    public GameObject cardPrefab;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitialDrawingCards());
    }

    IEnumerator InitialDrawingCards()
    {
        GameObject obj = new GameObject("NuevoGameObject");
        CardSpawner initialDrawing = obj.AddComponent<CardSpawner>();

        yield return null;

        initialDrawing.cardPrefab = cardPrefab; 
        initialDrawing.fileFile = playerHand;
        initialDrawing.fieldTransform = playerHandTransform;

        for (int i = 0; i < 10; i++)
        {
            initialDrawing.SpawnCard(deck.DrawCard());
            yield return new WaitForSecondsRealtime(0.3f);
        }

        gameManager.playerTurn = gameManager.initialPlayerTurn;
    }

}
