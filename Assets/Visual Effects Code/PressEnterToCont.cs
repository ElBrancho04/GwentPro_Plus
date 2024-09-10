using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PressEnterToCont : MonoBehaviour
{
    public bool turnScreen;
    public GameManager gameManager;
    public int player;

    void OnEnable()
    {
        StartCoroutine(Wait());
        IEnumerator Wait()
        {
            yield return null;
            if (gameManager.realTurn == player%2 + 1)
            gameObject.SetActive(false);
        }
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            if (turnScreen)
            {
                gameObject.SetActive(false);
            }
            else
            {
                SceneManager.LoadScene("MenuScene");
            }
        }
    }
}
