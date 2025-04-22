using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Text winText;
    [Tooltip("Drag all your BoxController objects here")]
    public List<BoxController> boxes = new List<BoxController>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        winText.gameObject.SetActive(false);
    }

    public void RemoveBox(BoxController box)
    {
        boxes.Remove(box);

        if (boxes.Count == 0)
            WinGame();
    }

    void WinGame()
    {
        winText.text = winText.text;
        winText.gameObject.SetActive(true);
        Handheld.Vibrate();
    }
}
