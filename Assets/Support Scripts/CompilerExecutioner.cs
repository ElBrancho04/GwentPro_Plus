using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

class CompilerExecutioner: MonoBehaviour
{
    public TMP_InputField input;
    public TMP_Text displayText;
    public string CompilationErrors {get; private set;}

    void Start()
    {
        Invoke("Activate", 3f);
    }
    void Activate()
    {
        input.gameObject.SetActive(true);
    }
    public void Compile()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        SemanticAnalyzer analyzer = new SemanticAnalyzer(FindAnyObjectByType<Context>());
        CompilationErrors = analyzer.Analyze(input.text);
        if (CompilationErrors != "")
        {
            displayText.text = CompilationErrors;
        }
        else
        {
            foreach (var item in analyzer.Cards)
            {
                gameManager.playerDeck1.deck.Insert(0, item);
            }
            Destroy(input.gameObject);
        }
    }
}