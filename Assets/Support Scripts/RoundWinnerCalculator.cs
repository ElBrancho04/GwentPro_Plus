using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundWinnerCalculator : MonoBehaviour
{
    public File sField1;
    public File rField1;
    public File mField1;
    public File sField2;
    public File rField2;
    public File mField2;
    public int totalPoints1;
    public int totalPoints2;
    public GameManager gameManager;
    public int cardsInField1;
    public int cardsInField2;

    public void RoundWinnerCal()
    {
        totalPoints1 = FilePowerCalculator(sField1) + FilePowerCalculator(rField1) + FilePowerCalculator(mField1);
        totalPoints2 = FilePowerCalculator(sField2) + FilePowerCalculator(rField2) + FilePowerCalculator(mField2);

        if (totalPoints1 > totalPoints2)
        {
            gameManager.roundsWon1++;
            gameManager.playerTurn = 1;
            StartCoroutine(EndOfRoundRemoving());
            return;
        }
        if (totalPoints1 < totalPoints2)
        {
            gameManager.roundsWon2++;
            gameManager.playerTurn = 2;
            StartCoroutine(EndOfRoundRemoving());
            return;
        }
        StartCoroutine(EndOfRoundRemoving());
        return;
    }

    public int FilePowerCalculator(File file)
    {
        int result = 0;
        for (int i = 0; i < file.cards.Count; i++)
        {
           result += file.cards[i].attackPower; 
        }
        file.cards.Clear();
        return result;
    }

    IEnumerator EndOfRoundRemoving()
    {
        int totalCardsInField = cardsInField1 + cardsInField2;
        for (int i = 0; i < totalCardsInField; i++)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            Destroy(FindObjectOfType<IsActive>().gameObject);
        }
    }
}
