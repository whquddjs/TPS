using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public Image aimPointReticle;
    public Image hitPointReticle;

    public float smoothTime = 0.2f;//지연시간
    
    private Camera screenCamera;//카메라가 바라보는 방향으로 총을 쏠 것임
    private RectTransform crossHairRectTransform;

    private Vector3 currentHitPointVelocity;
    private Vector2 targetPoint;

    private void Awake()
    {
        screenCamera = Camera.main;
        crossHairRectTransform = hitPointReticle.GetComponent<RectTransform>();
    }

    public void SetActiveCrosshair(bool active)
    {
        hitPointReticle.enabled = active;
        aimPointReticle.enabled = active;
    }

    public void UpdatePosition(Vector3 worldPoint)
    {
        targetPoint = screenCamera.WorldToScreenPoint(worldPoint);
    }

    private void Update()
    {
        if (!hitPointReticle.enabled)
            return;

        crossHairRectTransform.position = Vector3.SmoothDamp(crossHairRectTransform.position, targetPoint,
            ref currentHitPointVelocity, smoothTime);
    }
}