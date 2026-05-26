# ArchiSlave — Work Log

> **규칙:** 작업 재시작 시 반드시 이 파일을 먼저 읽고 시작할 것.

---

## 프로젝트 개요

- **엔진:** Unity 6000.3.10f1 (2D Orthographic)
- **씬:** `Assets/0_Scenes/GameScene.unity`
- **패키지:** Cinemachine, New Input System (activeInputHandler: 1)
- **렌더 파이프라인:** URP (Universal Render Pipeline)
- **MCP 인스턴스:** `ArchiSlave@a56ad8f8c0c27548`

---

## 주요 스크립트

| 파일 | 역할 |
|------|------|
| `Assets/2_Scripts/Architecture/Grid.cs` | 그리드 셀 개별 동작 (호버 팝업, alpha, sorting) |
| `Assets/2_Scripts/Architecture/GridSystem.cs` | 그리드 생성·관리 (rows×cols 배열) |
| `Assets/2_Scripts/Camera/RTSCameraController.cs` | 엣지 스크롤 + 줌 (New Input System) |

---

## 씬 구성 (GameScene)

| 오브젝트 | 역할 |
|----------|------|
| `Main Camera` (instanceID: 51630) | URP Base Camera, Orthographic, CinemachineBrain |
| `RTSVirtualCamera` (instanceID: 51612) | CinemachineCamera + CinemachineFollow + CinemachineRotationComposer |
| `CameraTarget` (instanceID: 51658) | 카메라 추적 대상, RTSCameraController 컴포넌트 보유 |
| `EventSystem` (instanceID: 51622) | InputSystemUIInputModule (New Input System) |
| `GridSystem` (instanceID: -5778) | 8×12 그리드, GridSystem 컴포넌트 |

---

## 리소스

| 경로 | 내용 |
|------|------|
| `Assets/1_Sources/IMG/GridFrame.png` | 290×290px 그리드 셀 스프라이트, PPU=100 |
| `Assets/GridMaterial.mat` | Sprite/Default 셰이더, GPU Instancing 활성화 |

---

## GridSystem 설정값

```
_rows = 8, _cols = 12
_cellSize = 2.9, _cellSpacing = 0.1
_gridSprite = GridFrame, _gridMaterial = GridMaterial
```

---

## 완료된 작업 이력

### 1. 그리드 시스템 초기 구축
- `Grid.cs` — BoxCollider2D 자동 크기 설정, RequireComponent
- `GridSystem.cs` — rows/cols 설정 가능, Start() 1회 생성, `Grid[,]` 배열, `GetCell()` API

### 2. MCP 씬 세팅
- `GridFrame.png` 임포트 설정: Sprite, PPU=100
- `GridMaterial.mat` 생성: Sprites/Default, GPU Instancing
- `GridSystem` GameObject 생성 및 컴포넌트 프로퍼티 할당

### 3. 호버 이벤트 시스템 (New Input System 대응)
- **문제:** `activeInputHandler: 1` (New Input System 전용) → `OnMouseEnter/Exit` 완전 비작동
- **해결:**
  - `Grid.cs`: `OnMouseEnter/Exit` → `IPointerEnterHandler / IPointerExitHandler` 구현
  - `Main Camera`: `Physics2DRaycaster` 컴포넌트 추가
  - EventSystem에 이미 `InputSystemUIInputModule` 존재 확인

### 4. 카메라 회전 Damping 버그 수정
- **문제:** `CinemachineRotationComposer.Damping = (0.5, 0.5)` → 카메라 이동 시 회전 래그 발생 → 2D 스프라이트 압축돼 보임 (그리드 축소)
- **해결:** `CinemachineRotationComposer.Damping` → `(0, 0)`

### 5. 화면 찢김(Screen Tearing) 수정
- **문제:** `Main Camera.clearFlags = 2 (Depth Only)` → 배경 미클리어 → 카메라 이동 시 잔상/찢김
- **해결:** `clearFlags` → `1 (Solid Color)`
- **VSync:** Quality Level 0에서 `vSyncCount: 1` 이미 활성화 확인

### 6. 호버 애니메이션 강화
- `_hoverScaleMultiplier`: 1.15 → 1.35
- `_animDuration`: 0.25s → 0.42s
- `BuildDefaultCurve()` 재설계: 부드러운 수축(t=0.20, y=-0.06) → 큰 오버슈트(t=0.52, y=1.25) → 정착

### 7. Alpha 페이드 구현
- 기본 Alpha: `0.2f` (`Initialize`에서 `sr.color = Color(1,1,1,0.2)`)
- Enter: Alpha 빠르게 1.0 상승 (`Clamp01(t * 3.5)` — 약 0.12초)
- Exit: Alpha 바운싱 전체 시간에 걸쳐 서서히 0.2 감소 (`SmoothStep`)

### 8. Sorting Order 전환 로직
```
Enter: 0 →[애니 시작 +2]→ 3(정착)
Exit:  3 →[애니 시작 +1]→ 0(정착)
```
- 인접 셀 동시 전환 시 z-순서 충돌 방지
- 애니메이션 완료 시점에만 최종값 적용

---

## 현재 Grid.cs 핵심 로직 요약

```csharp
// 상태별 Sorting Order
Enter 진행: _baseSortingOrder + 2
Enter 정착: _baseSortingOrder + 3
Exit  진행: _baseSortingOrder + 1
Exit  정착: _baseSortingOrder + 0 (기본)

// Alpha
기본: 0.2f
Enter: Clamp01(t * 3.5f) → 빠른 상승
Exit:  SmoothStep(0,1,t)  → 서서히 감소

// Scale
_hoverScaleMultiplier = 1.35f, _animDuration = 0.42f
곡선: t=0→0, t=0.20→-0.06(수축), t=0.52→1.25(오버슈트), t=0.78→0.96, t=1→1
```

---

## 알려진 이슈 / 주의사항

- `CameraTarget`이 Play 모드 전환 시 이전 위치를 기억함 → 카메라가 그리드 밖을 가리킬 수 있음
- `Grid` 클래스명이 `UnityEngine.Grid`(Tilemap)와 잠재적 충돌 가능 → 향후 namespace `ArchiSlave` 추가 고려
- `sharedMaterial` 사용 중 → `sr.color` 변경은 per-instance로 처리되므로 배칭에 영향 없음

---

## 다음 작업 후보

- ArchitectureNode 시스템 구현 (노드 배치, 연결)
- CPU.cs 등 구체적인 노드 타입 구현
- 그리드 선택(클릭) 기능

---

*마지막 업데이트: 2026-05-26*
