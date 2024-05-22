using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public TextMeshProUGUI text;
    public static int score;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        Debug.Log("text: " + text.text);
    }

    // Update is called once per frame
    void Update()
    {
        text.text = score.ToString();   
    }
}
