using UnityEngine;
using UnityEngine.UI;

public class PlayerBoard : MonoBehaviour
{
    public int boardIndex;
    public Image iconImage;
    public Button btn;
    private void Awake()
    {
        PlayerCursor.ClickPlayerCursorEvent += OnClick;
    }
    private void OnDestroy()
    {
        PlayerCursor.ClickPlayerCursorEvent -= OnClick;
    }
    void Start()
    {
    }
    void OnClick(GameObject gameObj, int PlayerNumber, bool isSelect)
    {
        if (gameObj != gameObject)
            return;

        if (isSelect)
        {
            if (JoinManager.Instance.CheckBoardDontUse(boardIndex))
                return;
            iconImage.sprite = JoinManager.Instance.playerSprites[PlayerNumber];
            JoinManager.Instance.BindPlayerOnBoard(PlayerNumber, boardIndex);
        }
        else
        {
            if (!JoinManager.Instance.CheckPlayerAndBoard(PlayerNumber, boardIndex))
                return;

            iconImage.sprite = JoinManager.Instance.DefaultSprite;
            JoinManager.Instance.UnBindPlayerOnBoard(PlayerNumber, boardIndex);
        }
        
    }
    void Update()
    {
       
    }

   
}
