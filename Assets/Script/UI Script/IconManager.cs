using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconManager : MonoBehaviour
{
    private static IconManager instance;

    public GameObject buffImage;
    public RectTransform buffImageGroup;

    public static IconManager Instance {
        get {
            if (instance == null) {
                instance = GameObject.FindObjectOfType<IconManager>();
            }
            return instance;
        }
    }


}
