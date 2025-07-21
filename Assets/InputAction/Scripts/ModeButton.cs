using UnityEngine;

public class ModeButton : MonoBehaviour
{
    private void Awake()
    {
        Player.ClickPlayerCursorEvent += OnClick;
    }
    private void OnDestroy()
    {
        Player.ClickPlayerCursorEvent -= OnClick;
    }

    void OnClick(GameObject gameObj, int PlayerNumber, bool isSelect = true)
    {
        if (gameObj != gameObject)
            return;
        BoardManager.Instance.ToggleButton();
    }
}
