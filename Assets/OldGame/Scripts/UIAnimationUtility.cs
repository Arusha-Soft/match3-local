using UnityEngine;
using DG.Tweening;

public static class UIAnimationUtility
{
    /// <summary>
    /// Moves a UI RectTransform up or down with a custom motion using DOTween.
    /// </summary>
    /// <param name="target">The RectTransform to move.</param>
    /// <param name="offsetY">How much to move in the Y direction (positive for up, negative for down).</param>
    /// <param name="duration">Duration of the animation.</param>
    /// <param name="easeType">Type of easing (e.g., Ease.OutQuad, Ease.InOutSine).</param>
    public static void MoveUIElement(RectTransform target, float offsetY, float duration, Ease easeType = Ease.OutBounce)
    {
        if (target == null) return;

        target.DOKill(true); // Kill any previous animation to avoid conflicts

        // Move to a new anchored position
        Vector2 targetPosition = target.anchoredPosition + new Vector2(0, offsetY);
        target.DOAnchorPos(targetPosition, duration).SetEase(easeType);
    }

    public static void ShakeUiElemnt(RectTransform target, float offsetY, float duration, Ease easeType = Ease.OutQuad)
    {
        if (target == null) return;

        target.DOKill(true); // Kill any previous animation to avoid conflicts

        // Move to a new anchored position
        Vector2 targetPosition = target.anchoredPosition + new Vector2(0, offsetY);
        target.DOShakePosition(duration,1f, 1,50,false,true).SetEase(easeType).SetLoops(-1, LoopType.Restart);
    }
        public static void ResetPosition(RectTransform rectTransform)
    {
        // First, kill any existing tweens to stop ongoing animations (like shaking or moving)
        rectTransform.DOKill();
        rectTransform.DORestart();
        //// Now, reset the position to its original position (assuming you want it to reset to (0, 0) or its original anchored position)
        //rectTransform.anchoredPosition = new Vector2(0f, 0f);  // Adjust this if your letters have a specific initial position
        //rectTransform.localScale = Vector3.one;  // Ensure scale is reset (optional)
    }
    public static void ShakeUiElemntZ(RectTransform target, float offsetZ, float duration, Ease easeType = Ease.OutQuad)
    {
        if (target == null) return;

        target.DOKill(true); // Kill any previous animation to avoid conflicts

        // Get the current local position
        Vector3 initialPosition = target.localPosition;

        // Create a shake animation on the Z-axis
        target.DOShakePosition(duration, new Vector3(1, 1, offsetZ), 10, 90, false, true)
            .SetEase(easeType) // Set easing type
            .SetLoops(-1, LoopType.Yoyo); // Yoyo loop for shaking effect
    }
    public static void PunchPosition(RectTransform target, Vector3 punchAmount, float duration, int vibrato = 10, float randomness = 90f, Ease easeType = Ease.InBounce)
    {
        if (target == null) return;

        target.DOKill(true); // Kill any previous animation to avoid conflicts

        // Punch the position with the given punchAmount, duration, vibrato, and randomness
        target.DOPunchPosition(punchAmount, duration, vibrato, randomness)
            .SetEase(easeType)  // Set the bounce ease type
            .SetLoops(1, LoopType.Restart);  // Infinite loop with bounce effect
    }
    public static void ShakeScale(RectTransform target, Vector3 shakeAmount, float duration, int vibrato = 10, float randomness = 90f, Ease easeType = Ease.InBounce)
    {
        if (target == null) return;

        target.DOKill(true); // Kill any previous animation to avoid conflicts

        // Apply the shake scale effect
        target.DOShakeScale(duration, shakeAmount, vibrato, randomness)
            .SetEase(easeType)  // Set the bounce ease type
            .SetLoops(1, LoopType.Restart);  // Infinite loop with bounce effect
    }
    public static void PunchRotation(RectTransform target, Vector3 punchAmount, float duration, int vibrato = 10, float randomness = 90f, Ease easeType = Ease.InBounce)
    {
        if (target == null) return;

        target.DOKill(true); // Kill any previous animation to avoid conflicts

        // Punch the position with the given punchAmount, duration, vibrato, and randomness
        target.DOPunchRotation(punchAmount, duration, vibrato, randomness)
            .SetEase(easeType)  // Set the bounce ease type
            .SetLoops(1, LoopType.Restart);  // Infinite loop with bounce effect
    }
    public static void ShakePosition(RectTransform target, Vector3 shakeAmount, float duration, int vibrato = 10, float randomness = 90f, Ease easeType = Ease.InBounce)
    {
        if (target == null) return;

        target.DOKill(true); // Kill any previous animation to avoid conflicts

        // Punch the position with the given punchAmount, duration, vibrato, and randomness
        target.DOShakePosition(duration,shakeAmount, vibrato, randomness,false,true)
            .SetEase(easeType)  // Set the bounce ease type
            .SetLoops(1, LoopType.Restart);  // Infinite loop with bounce effect
    }

}
