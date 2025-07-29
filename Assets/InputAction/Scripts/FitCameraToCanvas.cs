using UnityEngine;
using UnityEngine.UI;

public class FitCameraToCanvas : MonoBehaviour
{
    public Canvas canvas;
    public SpriteRenderer spriteRenderer;
    private void Start()
    {
    }

    private void Update()
    {
        if (canvas == null)
            return;
        Camera camera = GetComponent<Camera>();

        Vector3 canvasPos = canvas.transform.position;
        camera.transform.position = new Vector3(canvasPos.x, canvasPos.y, camera.transform.position.z);
        camera.orthographic = true;
        camera.orthographicSize = Screen.height / 2f;
        FitSpriteInCamera();
    }

    void FitSpriteInCamera()
    {
        if (canvas == null || spriteRenderer == null)
            return;
        Camera camera = GetComponent<Camera>();

        Sprite sprite = spriteRenderer.sprite;
        float spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
        float spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;
        float worldScreenHeight = camera.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * camera.aspect;
        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;
        spriteRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        spriteRenderer.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, spriteRenderer.transform.position.z);
    }
}
