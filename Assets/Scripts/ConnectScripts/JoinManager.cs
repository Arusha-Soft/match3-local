using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JoinManager : MonoBehaviour
{
    public GameObject CursorPrefab;
    public Transform CursorParent;

    public GameObject FreeToAllPanel,TeamPanel;
    public GameObject PlayerBoardPrefabFreeToAll;
    public Transform PlayerBoardParentFreeToAll;
    public GameObject PlayerBoardPrefabTeam;
    public Transform PlayerBoardParentTeam;
    private List<GameObject> PlayerList=new List<GameObject>();
    private List<GameObject> CursorList = new List<GameObject>();
    public Text ButtonText;
    private bool isFreeToAll = true;

    public static JoinManager Instance;
    public Color[] playerColors;
    public Sprite[] playerSprites;
    public Sprite[] CursorSprites;
   //public List<Gamepad> gamepadList;
    //public Text gamePadError;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // var gamepads = Gamepad.all;
        //gamepadList = new List<Gamepad>();
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            // gamepadList.Add(gamepad);
            var cursor = SimplePool.Spawn(CursorPrefab, Vector3.zero, CursorPrefab.transform.rotation);
            cursor.transform.SetParent(CursorParent, false);
            CursorList.Add(cursor);
            cursor.GetComponent<PlayerCursor>().playerNumber = i;
            cursor.GetComponent<PlayerCursor>().GamepadPlayer = Gamepad.all[i];
        }

        SetModeButoon(isFreeToAll);
        for (int i = 0; i < 2; i++)
        {
            GeneratePlayerBoard(PlayerBoardPrefabFreeToAll, PlayerBoardParentFreeToAll,i);
        }
        for (int i = 0; i < 8; i++)
        {
            GeneratePlayerBoard(PlayerBoardPrefabTeam, PlayerBoardParentTeam,i);
        }
    }

    private void GeneratePlayerBoard(GameObject PlayerBoardPrefab, Transform PlayerBoardParent,int id)
    {
        var player = SimplePool.Spawn(PlayerBoardPrefab, Vector3.zero, PlayerBoardPrefab.transform.rotation);
        player.transform.SetParent(PlayerBoardParent,false);
        player.name = id.ToString();
        PlayerList.Add(player);
    }
    public void ToggleButton()
    {
        isFreeToAll = !isFreeToAll;
        SetModeButoon(isFreeToAll);
    }
    private void SetModeButoon(bool isFreeToAll)
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
        //var gamepads = Gamepad.all;
        //if (gamepads.Count == 2)
        //{
        //    Gamepad player1 = gamepads[0];
        //    Gamepad player2 = gamepads[1];
        //}
        //else
        //{
        //    Debug.Log("Need 2 controllers. Found: " + gamepads.Count);
        //}
    }
}
