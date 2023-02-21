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

        // Play back the recorded audio to verify that it is working
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.Play();

        // Define the keywords to recognize
        string[] keywords = { "persona", "i got you", "attack" };
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
                break;
            case "i got you":
                Debug.Log("I got you too");
                break;
            case "attack":
                Debug.Log("Okay");
                break;
            default:
                Debug.Log("What did you say??");
                break;
        }
    }
}
