using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AyudaText : MonoBehaviour
{
    public GameObject ayudaButton;

    void Update()
    {
        if (!ayudaButton.activeSelf)
        gameObject.SetActive(false);
    }
}
