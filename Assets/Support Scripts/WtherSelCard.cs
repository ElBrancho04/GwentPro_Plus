using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WtherSelCard : MonoBehaviour
{
    public File file;
    public int decrs;


    void Update()
    {
        if (file != null){
        for (int i = 0; i < file.cards.Count; i++)
        {
            if (!file.cards[i].isGold)
            file.cards[i].decrs = file.decrs;
        }}
    }

    void OnDestroy()
    {
        if(file != null){
        for (int i = 0; i < file.cards.Count; i++)
        {
            file.decrs -= decrs;
        }}
    }
}
