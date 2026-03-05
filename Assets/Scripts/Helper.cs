using UnityEngine;

public static class Helper {
    /// <summary>
    /// Returns true if a world position is inside the specified camera's viewport.
    /// Pivot-based check. Good for small objects.
    /// </summary>
    public static bool IsInsideViewport(Vector3 worldPosition, Camera cam = null) {
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Vector3 viewportPos = cam.WorldToViewportPoint(worldPosition);

        return viewportPos.z > 0f &&
               viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0f && viewportPos.y <= 1f;
    }

    /// <summary>
    /// Returns true if ANY part of the bounds overlaps the camera view.
    /// Best for large sprites or bosses.
    /// </summary>
    public static bool IsBoundsVisible(Bounds bounds, Camera cam = null) {
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        return !(bounds.max.x < min.x ||
                 bounds.min.x > max.x ||
                 bounds.max.y < min.y ||
                 bounds.min.y > max.y);
    }
}