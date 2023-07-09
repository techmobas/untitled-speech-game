using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.Events;

public class QuitGame : MonoBehaviour {

    private KeywordRecognizer keywordRecognizer;

    public UnityEvent quitMenu;
    public UnityEvent quitGame;

    private void Start() {
        AudioClip audioClip = Microphone.Start(null, true, 120, 44100);
        while (!(Microphone.GetPosition(null) > 0)) { } // Wait until recording starts

        // Define the keywords to recognize
        string[] keywords = { "exit battle", "exit game"};
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
            case "exit battle":
                Debug.Log("Loading desired level");
                quitGame.Invoke();
                break;
            case "exit game":
                Debug.Log("Quitting Game");
                break;


        }
    }
}
