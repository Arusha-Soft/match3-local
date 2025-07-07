using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Board Setup")]
    public GameObject boardPrefab;
    public Transform boardParent;
    public int numberofBoards;
    private List<GameObject> boards = new List<GameObject>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CreateBoards(numberofBoards);
    }

    private void CreateBoards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var board = Instantiate(boardPrefab, boardParent);
            boards.Add(board);
        }
    }

    public List<GameObject> GetBoards()
    {
        return boards;
    }
    
}
