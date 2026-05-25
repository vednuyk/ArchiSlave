using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class RTSCameraController : MonoBehaviour
{
    [Header("모드")]
    [Tooltip("2D 씬이면 체크 (XY 이동 + OrthoSize 줌)\n3D 씬이면 해제 (XZ 이동 + FOV 줌)")]
    public bool is2DMode = true;

    [Header("엣지 스크롤")]
    [Tooltip("마우스가 화면 가장자리 몇 픽셀 안에 들어올 때 이동 시작")]
    public float edgeScrollThickness = 20f;
    [Tooltip("카메라 이동 속도")]
    public float moveSpeed = 10f;

    [Header("줌")]
    [Tooltip("스크롤 줌 속도")]
    public float zoomSpeed = 2f;
    [Tooltip("최솟값 — 2D: OrthoSize 최소, 3D: FOV 최소")]
    public float minZoom = 2f;
    [Tooltip("최댓값 — 2D: OrthoSize 최대, 3D: FOV 최대")]
    public float maxZoom = 20f;

    [Header("커서")]
    [Tooltip("게임 실행 중 커서를 창 안에 가두기")]
    public bool confineCursor = true;

    [Header("참조")]
    [Tooltip("씬의 CinemachineCamera 오브젝트를 여기에 드래그하세요")]
    public CinemachineCamera virtualCamera;

    void OnEnable()
    {
        if (confineCursor)
            Cursor.lockState = CursorLockMode.Confined;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        HandleEdgeScroll();
        HandleZoom();
    }

    void HandleEdgeScroll()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        // 창 밖에 있으면 무시
        if (mousePos.x < 0f || mousePos.x > Screen.width ||
            mousePos.y < 0f || mousePos.y > Screen.height)
            return;

        float dx = 0f, dy = 0f, dz = 0f;

        if (mousePos.x <= edgeScrollThickness)
            dx = -1f;
        else if (mousePos.x >= Screen.width - edgeScrollThickness)
            dx = 1f;

        if (is2DMode)
        {
            if (mousePos.y <= edgeScrollThickness)
                dy = -1f;
            else if (mousePos.y >= Screen.height - edgeScrollThickness)
                dy = 1f;
        }
        else
        {
            if (mousePos.y <= edgeScrollThickness)
                dz = -1f;
            else if (mousePos.y >= Screen.height - edgeScrollThickness)
                dz = 1f;
        }

        Vector3 moveDir = new Vector3(dx, dy, dz);
        if (moveDir.sqrMagnitude < 0.01f) return;

        transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime, Space.World);
    }

    void HandleZoom()
    {
        if (virtualCamera == null || Mouse.current == null) return;

        // New Input System: scroll.y 는 노치당 ±120 단위 → 0.01 로 정규화
        float scroll = Mouse.current.scroll.ReadValue().y * 0.01f;
        if (Mathf.Approximately(scroll, 0f)) return;

        LensSettings lens = virtualCamera.Lens;

        if (lens.Orthographic)
        {
            lens.OrthographicSize = Mathf.Clamp(
                lens.OrthographicSize - scroll * zoomSpeed * 5f,
                minZoom, maxZoom);
        }
        else
        {
            lens.FieldOfView = Mathf.Clamp(
                lens.FieldOfView - scroll * zoomSpeed * 10f,
                minZoom, maxZoom);
        }

        virtualCamera.Lens = lens;
    }
}
