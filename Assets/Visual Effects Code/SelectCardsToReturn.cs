using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCardsToReturn : MonoBehaviour
{
    public GameObject model1;
    public GameObject model2;
    public GameObject follower;
    public GameManager gameManager;
    public bool efectUI_Text;

    void Update()
    {
        if (model1 != null && !model1.activeSelf && !model2.activeSelf)
        gameObject.SetActive(false);
        if (follower != null)
        follower.SetActive(true);
    }
    void LateUpdate()
    {
        if (efectUI_Text && (gameManager.playerTurn == 1 || gameManager.playerTurn == 2))
        gameObject.SetActive(false);
    }
}
