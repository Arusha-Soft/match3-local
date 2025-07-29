using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
    public int PlayerID;
    public Image ArrowImage;
    public Image NumberImage;

    public Gamepad GamepadPlayer;
    public Mouse mouse;
    private float moveSpeed = 500f;
    private float moveMouseSpeed = 10;
   
    public void SetArrow(Sprite sprite) => ArrowImage.sprite = sprite;
    public void SetNumber(Sprite sprite) => NumberImage.sprite = sprite;
    public delegate void ClickPlayerCursor(GameObject gameobj, int PlayerNumber, bool isSelect = true);
    public static ClickPlayerCursor ClickPlayerCursorEvent;

    private RectTransform cursor;
    private Vector2 moveInput;

    void Start()
    {
        cursor=GetComponent<RectTransform>();
        SetArrow(PlayerManager.Instance.CursorSprites[PlayerID]);
        SetNumber(PlayerManager.Instance.NumberSprites[PlayerID]);
    }

    private void Update()
    {
        if (GamepadPlayer != null)
        {
            moveInput = GamepadPlayer.leftStick.ReadValue();

            if (GamepadPlayer.buttonSouth.wasPressedThisFrame || GamepadPlayer.crossButton.wasPressedThisFrame)
            {
                var theBtn = DetectButton();
               
                if (theBtn != null)
                {
                    if (theBtn.name == "ModeButton")
                        ModeButtonClick(theBtn.gameObject);
                    else if (theBtn.name == "SelectTeam")
                        ModeButtonClick(theBtn.gameObject);
                    else
                        SelectBoard(theBtn.gameObject);
                }
            }
            if (GamepadPlayer.buttonEast.wasPressedThisFrame || GamepadPlayer.circleButton.wasPressedThisFrame)
            {
                var theBtn = DetectButton();
                if (theBtn != null)
                {
                    UnselectBoard(theBtn.gameObject);
                }
            }
            moveCursor(moveSpeed);
        }
    }
    private void moveCursor(float speed)
    {
        cursor.anchoredPosition += moveInput * speed * Time.deltaTime;

        RectTransform canvasRect = cursor.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        Vector2 clampedPos = cursor.anchoredPosition;
        float halfWidth = cursor.sizeDelta.x * 0.5f;
        float halfHeight = cursor.sizeDelta.y * 0.5f;

        // Clamp within the canvas rect
        clampedPos.x = Mathf.Clamp(clampedPos.x, -canvasRect.rect.width / 2 + halfWidth, canvasRect.rect.width / 2 - halfWidth);
        clampedPos.y = Mathf.Clamp(clampedPos.y, -canvasRect.rect.height / 2 + halfHeight, canvasRect.rect.height / 2 - halfHeight);

        cursor.anchoredPosition = clampedPos;
    }
    private Button DetectButton()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = cursor.position;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        Button button = null;
        foreach (var result in results)
        {
            button = result.gameObject.GetComponent<Button>();
            if (button != null)
                break;
        }
        return button;
    }

    private void ModeButtonClick(GameObject btn)
    {
        ClickPlayerCursorEvent?.Invoke(btn, PlayerID);
    }
    private void SelectBoard(GameObject btn)
    {
        if (PlayerManager.Instance.CheckPlayerOnBoard(PlayerID))
            return;
        ClickPlayerCursorEvent?.Invoke(btn, PlayerID);
    }
    private void UnselectBoard(GameObject btn)
    {
        ClickPlayerCursorEvent?.Invoke(btn, PlayerID, false);
    }
}
