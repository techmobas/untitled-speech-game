using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsUIManager : MonoBehaviour
{
    private static StatsUIManager instance;


    [Header("Damage Text")]
    [SerializeField] GameObject textPrefab;
    [SerializeField] float speed;
    [SerializeField] Vector3 direction;
    [SerializeField] float fadeTime;


    [Header("Buff Icon")]
    [SerializeField] GameObject buffIconPrefab;
    private Dictionary<AbilitySO, Image> activeBuffsIcon = new Dictionary<AbilitySO, Image>();

    public static StatsUIManager Instance {
        get {
            if (instance == null) {
                instance = GameObject.FindObjectOfType<StatsUIManager>();
            }
            return instance;
        }
    }

    public void GenerateText(RectTransform canvasTransform, Vector3 position, string text, Color color, bool isCrit) {
        GameObject sct = (GameObject) Instantiate(textPrefab, position, Quaternion.identity);

        sct.transform.SetParent(canvasTransform);
        sct.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        sct.GetComponent<DamageUI>().InitializeText(speed, direction, fadeTime, isCrit);
        sct.GetComponent<TextMeshProUGUI>().text = text;
        sct.GetComponent<TextMeshProUGUI>().color = color;
    }

    public void AddBuffIcon(RectTransform buffIconGroup, Vector3 positon, AbilitySO buff) {

        GameObject go = (GameObject) Instantiate(buffIconPrefab, positon, Quaternion.identity);

        go.transform.SetParent(buffIconGroup);
        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        Image buffImage = go.GetComponent<Image>();
        buffImage.sprite = buff.icon;
        activeBuffsIcon[buff] = buffImage;
    }

    public void RemoveBuffIcon(AbilitySO buff) {

        if (activeBuffsIcon.TryGetValue(buff, out Image buffImage)) {
            Destroy(buffImage.gameObject);
            activeBuffsIcon.Remove(buff);
        }
    }
}
