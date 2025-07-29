using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public int BoardID;
    public Image BoardImage;
    public Image SelectImage;
    public GameObject BoardOfImage;
    public Button btn;

    public SelectTeam selectTeam;
    private void Awake()
    {
        Player.ClickPlayerCursorEvent += OnClick;
    }
    private void OnDestroy()
    {
        Player.ClickPlayerCursorEvent -= OnClick;
    }
    private void OnEnable()
    {
    }
    void Start()
    {
    }
    public void SetSelectTeamActive(bool isFreeToAll)
    {
        selectTeam.gameObject.SetActive(!isFreeToAll);
    }
    public void SetBoardState(bool isSelect)
    {
        BoardOfImage.SetActive(!isSelect);
    }
    public void OnClick(GameObject gameObj, int PlayerNumber, bool isSelect)
    {
        if (gameObj != gameObject)
            return;

        if (BoardManager.Instance.isFreeToAll)
        {
            if (isSelect)
            {
                if (PlayerManager.Instance.CheckBoardDontUse(BoardID))
                    return;

                SetBoardState(true);

                BoardImage.sprite = BoardManager.Instance.BoardSprites[PlayerNumber];
                SelectImage.sprite = BoardManager.Instance.SelectSprites[PlayerNumber];
                PlayerManager.Instance.BindPlayerOnBoard(PlayerNumber, BoardID, PlayerNumber, PlayerNumber);
            }
            else
            {
                if (!PlayerManager.Instance.CheckPlayerAndBoard(PlayerNumber, BoardID))
                    return;

                SetBoardState(false);
                PlayerManager.Instance.UnBindPlayerOnBoard(PlayerNumber, BoardID);
            }
        }
        else
        {
            if (isSelect)
            {
                if (PlayerManager.Instance.CheckBoardDontUse(BoardID))
                    return;
                SetBoardState(true);
                BoardImage.sprite = BoardManager.Instance.BoardSprites[selectTeam.CurrentTeam.TeamID];
                SelectImage.sprite = BoardManager.Instance.SelectSprites[selectTeam.CurrentTeam.TeamID];
                PlayerManager.Instance.BindPlayerOnBoard(PlayerNumber, BoardID, selectTeam.CurrentTeam.TeamID, selectTeam.CurrentTeam.TeamID);
            }
            else
            {
                if (!PlayerManager.Instance.CheckPlayerAndBoard(PlayerNumber, BoardID))
                    return;

                SetBoardState(false);
                PlayerManager.Instance.UnBindPlayerOnBoard(PlayerNumber, BoardID);
            }

        }
        
    }
}
