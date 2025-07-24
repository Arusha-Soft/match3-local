using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public int BoardID;
    public Image iconImage;
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
        if (BoardManager.Instance.isFreeToAll)
            selectTeam.gameObject.SetActive(false);
        else
            selectTeam.gameObject.SetActive(true);
    }
    void Start()
    {
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
                BoardOfImage.SetActive(false);

                iconImage.sprite = BoardManager.Instance.BoardSprites[PlayerNumber];
                PlayerManager.Instance.BindPlayerOnBoard(PlayerNumber, BoardID, PlayerNumber);
            }
            else
            {
                if (!PlayerManager.Instance.CheckPlayerAndBoard(PlayerNumber, BoardID))
                    return;

                iconImage.sprite = BoardManager.Instance.DefaultSprite;
                PlayerManager.Instance.UnBindPlayerOnBoard(PlayerNumber, BoardID);
            }
        }
        else
        {
            if (isSelect)
            {
                if (PlayerManager.Instance.CheckBoardDontUse(BoardID))
                    return;
                BoardOfImage.SetActive(false);
                iconImage.sprite = BoardManager.Instance.BoardSprites[selectTeam.CurrentTeam.TeamID];
                PlayerManager.Instance.BindPlayerOnBoard(PlayerNumber, BoardID, selectTeam.CurrentTeam.TeamID);
            }
            else
            {
                if (!PlayerManager.Instance.CheckPlayerAndBoard(PlayerNumber, BoardID))
                    return;

                iconImage.sprite = BoardManager.Instance.DefaultSprite;
                PlayerManager.Instance.UnBindPlayerOnBoard(PlayerNumber, BoardID);
            }

        }
        
    }
}
