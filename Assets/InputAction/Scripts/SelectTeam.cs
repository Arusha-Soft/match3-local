using UnityEngine;
using UnityEngine.UI;
public class SelectTeam : MonoBehaviour
{
    public Image myImg;
    public Team CurrentTeam;
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
        CurrentTeam=TeamManager.Instance.CurrentTeam();
        myImg.color = CurrentTeam.color;
    }
    void OnClick(GameObject gameObj, int PlayerNumber, bool isSelect)
    {
        if (gameObj != gameObject)
            return;

        CurrentTeam= TeamManager.Instance.NextTeam();
        myImg.color = CurrentTeam.color;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
