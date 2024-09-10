using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;
using System.IO;

public class CardSpawner : MonoBehaviour
{
    public File fileFile;
    public GameObject cardPrefab; // Prefab de la carta
    public Transform fieldTransform; // Transform del área de juego donde se mostrarán las cartas
    public GameManager gameManager;

    // Método para instanciar una carta en el campo
    public void SpawnCard(CardData cardData)
    {
        GameObject cardObject = Instantiate(cardPrefab, fieldTransform);
        CardUI cardUI = cardObject.GetComponent<CardUI>();
        cardUI.card = cardData;
        fileFile.cards.Add(cardData);
        if (fileFile.cards.Count > 10)
        {
            gameManager.RemoveCard(cardObject);
        }       
    }

    public void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
}
