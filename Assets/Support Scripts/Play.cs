using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Play : MonoBehaviour
{
    public bool play;
    public bool quit;
    public void OnClick()
    {
        if (play)
        SceneManager.LoadScene("SampleScene");
        else if (!quit)
        SceneManager.LoadScene("HelpScene");
        else
        Application.Quit();
    }
}
