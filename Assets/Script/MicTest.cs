using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class MicTest : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    private void Start()
    {
        // Start recording audio from the microphone
        AudioClip audioClip = Microphone.Start(null, true, 120, 44100);
        while (!(Microphone.GetPosition(null) > 0)) { } // Wait until recording starts

        // Define the keywords to recognize
        string[] keywords = { "persona" };
        keywordRecognizer = new KeywordRecognizer(keywords);
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (args.confidence == ConfidenceLevel.High || args.confidence == ConfidenceLevel.Medium || args.confidence == ConfidenceLevel.Low)
        {
            switch (args.confidence)
            {
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

    private void DoSomething(string text)
    {
        switch (text)
        {
            case "persona":
                Debug.Log("You say Persona");
                gameObject.SetActive(false);
                keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
                keywordRecognizer.Stop();
                break;
        }
    }
}
