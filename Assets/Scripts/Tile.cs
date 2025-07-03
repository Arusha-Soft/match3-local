using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int tileID; // index 0-5 for cookie, chocolate, etc.
    public Image tileImage; // assign in prefab

    public void SetTile(int id, Sprite sprite)
    {
        tileID = id;
        tileImage.sprite = sprite;
    }
}
