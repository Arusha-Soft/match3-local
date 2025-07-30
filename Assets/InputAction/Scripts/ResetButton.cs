using UnityEngine;

public class ResetButton : MonoBehaviour
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
        GameController.Instance.InitGame();
    }
}
