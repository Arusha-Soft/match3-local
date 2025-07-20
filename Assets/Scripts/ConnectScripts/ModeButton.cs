using UnityEngine;

public class ModeButton : MonoBehaviour
{
    private void Awake()
    {
        PlayerCursor.ClickPlayerCursorEvent += OnClick;
    }
    private void OnDestroy()
    {
        PlayerCursor.ClickPlayerCursorEvent -= OnClick;
    }

    void OnClick(GameObject gameObj, int PlayerNumber, bool isSelect = true)
    {
        if (gameObj != gameObject)
            return;
        JoinManager.Instance.ToggleButton();
    }
}
