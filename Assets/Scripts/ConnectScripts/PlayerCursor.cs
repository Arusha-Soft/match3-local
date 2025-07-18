using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class PlayerCursor : MonoBehaviour
{
    public int playerNumber;
    public RectTransform cursor;
    public float moveSpeed = 100f;
    private Vector2 moveInput;

    public Image iconImage;
    public void SetColor(Sprite CursorSprites) => iconImage.sprite = CursorSprites;
    public delegate void ClickPlayerCursor(GameObject gameobj,int PlayerNumber);
    public static ClickPlayerCursor ClickPlayerCursorEvent;
    void Start()
    {
        SetColor(JoinManager.Instance.CursorSprites[playerNumber]);
    }

    private void Update()
    {
        if (Gamepad.current == null)
            return;

        moveInput = Gamepad.current.leftStick.ReadValue();

        cursor.anchoredPosition += moveInput * moveSpeed * Time.deltaTime;

        //Vector2 clampedPos = cursor.anchoredPosition;
        //clampedPos.x = Mathf.Clamp(clampedPos.x, 0, Screen.width);
        //clampedPos.y = Mathf.Clamp(clampedPos.y, 0, Screen.height);
        //cursor.anchoredPosition = clampedPos;

        if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
           Keyboard.current.enterKey.wasPressedThisFrame)
        {
            TryClick();
        }

    }
    void TryClick()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = cursor.position;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                ClickPlayerCursorEvent(button.gameObject, playerNumber);
                break;
            }
        }
    }
}
