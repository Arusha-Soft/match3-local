using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{

    public InputField Player1NameInputField;
    public InputField Player2NameInputField;
    private string PlayerName1, PlayerName2;
    public GameObject SelectPlayerPanel;
    public GameObject BattlePlayerPanel;
    public BattlePlayer BattlePlayer1, BattlePlayer2;

    void Start()
    {
        BattlePlayerPanel.SetActive(false);
    }

    public void OnGetPlayerName(int NoPlayer)
    {
        if (NoPlayer == 0)
            PlayerName1= Player1NameInputField.text;
        else 
            PlayerName2 = Player2NameInputField.text;
    }

    public void StartBattle()
    {
        Debug.Log(PlayerName1);
        Debug.Log(PlayerName2);
        BattlePlayerPanel.SetActive(true);
        BattlePlayer1.Init(PlayerName1);
        BattlePlayer2.Init(PlayerName2);
    }

    void Update()
    {
        
    }
}
