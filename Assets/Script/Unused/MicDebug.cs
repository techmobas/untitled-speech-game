using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicDebug : MonoBehaviour
{
    void Start()
    {
        AudioClip audioClip = Microphone.Start(null, true, 120, 44100);
        while (!(Microphone.GetPosition(null) > 0)) { } // Wait until recording starts

        // Play back the recorded audio to verify that it is working
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
