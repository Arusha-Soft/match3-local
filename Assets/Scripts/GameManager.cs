using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Action OninitializeComplete;

    [Header("Board Setup")]
    public GameObject boardPrefab;
    public Transform[] boardParents; // Assign 8 parents in the Inspector
    public Vector3[] prefabScales;       // Scale for each instantiated prefab (up to 8)

    public int numberofBoards;
    private List<GameObject> boards = new List<GameObject>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        //CreateBoards(numberofBoards);
        foreach (var board in boardParents) {board.gameObject.SetActive(false);}
    }
    private void InitializeBoard(int num)
    {
        CreateBoards(num);
    }

    //private void CreateBoards(int count)
    //{
    //    for (int i = 0; i < count; i++)
    //    {
    //        var board = Instantiate(boardPrefab, boardParent);
    //        boards.Add(board);
    //    }
    //    OninitializeComplete?.Invoke();
    //}
    //public void CreateBoards(int boardCount)
    //{
    //    if (boardCount < 1 || boardCount > boardParents.Length)
    //    {
    //        Debug.LogError("Invalid board count.");
    //        return;
    //    }

    //    Transform chosenParent = boardParents[boardCount - 1]; // boardCount=4 → index 3 = boardParent4
    //    boardParents[boardCount - 1].gameObject.SetActive(true);

    //    for (int i = 0; i < boardCount; i++)
    //    {
    //        GameObject board = Instantiate(boardPrefab, chosenParent);
    //        board.transform.localScale = Vector3.one;
    //        board.transform.localPosition = Vector3.zero; // Optional: Set position if needed
    //    }
    //}
    public void CreateBoards(int boardCount)
    {
        if (boardCount < 1 || boardCount > boardParents.Length)
        {
            Debug.LogError("Invalid board count.");
            return;
        }

        // Activate the selected parent (e.g. boardParents[3] for 4 boards)
        Transform chosenParent = boardParents[boardCount - 1];
        chosenParent.gameObject.SetActive(true);

        // Clear previous boards list
        boards.Clear();

        // Add children of the chosen parent to the boards list
        for (int i = 0; i < chosenParent.childCount; i++)
        {
            GameObject board = chosenParent.GetChild(i).gameObject;
            board.SetActive(true); // Just in case it's inactive
            boards.Add(board);
        }

        // Notify listeners (e.g. PlayerJoinManager)
        OninitializeComplete?.Invoke();
    }

    public List<GameObject> GetBoards()
    {
        return boards;
    }


    private void OnEnable()
    {
        PlayerCountSelector.OnCreateBoard += InitializeBoard;
    }
}
