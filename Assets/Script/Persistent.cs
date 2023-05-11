using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Persistent : MonoBehaviour
{
    private static Persistent instance;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Check if any scene is loaded
            if (SceneManager.sceneCount == 1 && SceneManager.GetActiveScene().buildIndex == 0) {
                SceneManager.LoadScene("Main Menu");
            }
        }
        else {
            Destroy(gameObject);
        }
    }
}
