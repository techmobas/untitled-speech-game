using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.Events;

public class LevelSelect : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;

    [Header("Levels")]
    public UnityEvent levelOne;
    public UnityEvent levelTwo;
    public UnityEvent levelThree;
    public UnityEvent levelFour;
    public UnityEvent levelBoss;
    public UnityEvent levelSecret;


    private void Start() {
        // Start recording audio from the microphone
        AudioClip audioClip = Microphone.Start(null, true, 120, 44100);
        while (!(Microphone.GetPosition(null) > 0)) { } // Wait until recording starts

        // Define the keywords to recognize
        string[] keywords = { "give me an eye", "give me mushroom", "give me goblin", "give me skeleton", "give me a boss", "give me a challenge" };
        keywordRecognizer = new KeywordRecognizer(keywords);
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args) {
        if (args.confidence == ConfidenceLevel.High || args.confidence == ConfidenceLevel.Medium || args.confidence == ConfidenceLevel.Low) {
            switch (args.confidence) {
                case ConfidenceLevel.High:
                    Debug.Log("High confidence");
                    DoSomething(args.text);
                    break;
                case ConfidenceLevel.Medium:
                    Debug.Log("Medium confidence");
                    DoSomething(args.text);
                    break;
                case ConfidenceLevel.Low:
                    Debug.Log("Low confidence");
                    break;
                default:
                    break;
            }
        }
    }

    private void DoSomething(string text) {
        switch (text) {
            case "give me an eye":
                Debug.Log("Loading desired level");
                levelOne.Invoke();
                break;
            case "give me mushroom":
                Debug.Log("Loading desired level");
                levelTwo.Invoke();
                break;
            case "give me goblin":
                Debug.Log("Loading desired level");
                levelThree.Invoke();
                break;
            case "give me skeleton":
                Debug.Log("Loading desired level");
                levelFour.Invoke();
                break;
            case "give me a boss":
                Debug.Log("Loading desired level");
                levelBoss.Invoke();
                break;
            case "give me a challenge":
                Debug.Log("Loading desired level");
                levelSecret.Invoke();
                break;
        }
    }
}
