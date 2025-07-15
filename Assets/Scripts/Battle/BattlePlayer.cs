using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePlayer : MonoBehaviour
{
    public Image TimeBar;
    public Text PlayerNameText;
    public int TimeLimit = 5;
    private float time=0;
    private bool startGame = false;
    public int matchCount;
    public GameObject FusePrefab;
    public Transform FuseParent;
    private List<GameObject> FuseList=new List<GameObject>();
    void Start()
    {
        //Init();
        StartCoroutine(test());
    }
    IEnumerator test()
    {
        yield return new WaitForSeconds(1);
        OnMatchCount();
        yield return new WaitForSeconds(1);
        StartCoroutine(test());
    }
    public void Init(string PlayerName)
    {
        PlayerNameText.text = PlayerName;
        TimeLimit = 5;
        matchCount = 0;

        foreach (var fuse in FuseList)
            Destroy(fuse);
        FuseList.Clear();

        startGame = true;
    }
    public void OnMatchCount()
    {
        matchCount++;
        TimeLimit = 5;
        time = 0;
        GameObject fuse = Instantiate(FusePrefab, FuseParent);
        FuseList.Add(fuse);
    }
    void Update()
    {
        if (!startGame)
            return;

        //Debug.Log(time);
        if (time < TimeLimit)
        {
            time += Time.deltaTime;
            TimeBar.fillAmount = 1 - (time / TimeLimit);
        }
        else
        {
            TimeBar.fillAmount = 0;
            // lose
        }
    }
}
