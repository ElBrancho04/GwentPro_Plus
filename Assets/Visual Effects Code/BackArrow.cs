using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackArrow : MonoBehaviour
{
    public void OnClick()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
