using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class JoinManager : MonoBehaviour
{
    public GameObject FreeToAllPanel,TeamPanel;
    public GameObject PlayerBoardPrefabFreeToAll;
    public Transform PlayerBoardParentFreeToAll;
    public GameObject PlayerBoardPrefabTeam;
    public Transform PlayerBoardParentTeam;
    private List<GameObject> PlayerList=new List<GameObject>();
    public Text ButtonText;

    void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            GeneratePlayerBoard(PlayerBoardPrefabFreeToAll, PlayerBoardParentFreeToAll);
        }
        for (int i = 0; i < 8; i++)
        {
            GeneratePlayerBoard(PlayerBoardPrefabTeam, PlayerBoardParentTeam);
        }
    }

    private void GeneratePlayerBoard(GameObject PlayerBoardPrefab, Transform PlayerBoardParent)
    {
        var player = SimplePool.Spawn(PlayerBoardPrefab, Vector3.zero, PlayerBoardPrefab.transform.rotation);
        player.transform.SetParent(PlayerBoardParent);
        PlayerList.Add(player);
    }
    public void ToggleButton(bool isFreeToAll)
    {
        FreeToAllPanel.SetActive(isFreeToAll);
        TeamPanel.SetActive(!isFreeToAll);
        if (isFreeToAll)
        {
            ButtonText.text = "FreeToAll";
        }
        else
        {
            ButtonText.text = "Team";
        }
    }
    void Update()
    {

    }
}
