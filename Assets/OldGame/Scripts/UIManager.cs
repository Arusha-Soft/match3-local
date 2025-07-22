using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI Ingame_text;
    public TextMeshProUGUI inGameTimer;
    private void Awake()
    {
        if(Instance == null) {Instance = this;}
    }

    public void SetText(string txt)
    {
        
        StartCoroutine(ShowAndHide(txt));
    }
    IEnumerator ShowAndHide(string message)
    {
        Ingame_text.text = message;
        yield return new WaitForSeconds(1.5f);
        Ingame_text.text = null;
    }

    public void SetTimer(string time)
    {
        inGameTimer.text = time;
    }
    public void HideTimer()
    {
        inGameTimer.text = null;
    }
}
