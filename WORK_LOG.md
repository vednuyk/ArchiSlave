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
| `Assets/2_Scripts/Architecture/Grid.cs` | 그리드 셀 (hover/activate 애니메이션, 빌딩 동기화, static events) |
| `Assets/2_Scripts/Architecture/GridSystem.cs` | 그리드 생성·관리 (rows×cols 배열) |
| `Assets/2_Scripts/Architecture/ArchitectureManager.cs` | 싱글톤, Grid↔ArchitectureNode 레지스트리 |
| `Assets/2_Scripts/Architecture/ArchitectureNode.cs` | 노드 베이스 클래스 (BaseScale, AnimateScale, SetSortingOrder) |
| `Assets/2_Scripts/Architecture/ConstructManager.cs` | 건설 실행, ArchitectureManager 자식으로 배치 |
| `Assets/2_Scripts/Architecture/BuildingDatabase.cs` | 건물 데이터 SO (id=0: CPU) |
| `Assets/2_Scripts/Architecture/CPU.cs` | CPU 노드 서브클래스 |
| `Assets/2_Scripts/UI/UIManager.cs` | B키/우클릭 입력, 패널 열기/닫기/SwitchTo 제어 |
| `Assets/2_Scripts/UI/UIPanelPopup.cs` | 팝업 패널 애니메이션 (Open/Close/SwitchTo, CanvasGroup) |
| `Assets/2_Scripts/UI/BuildButton.cs` | 건설 버튼 (static event, _buildingId) |
| `Assets/2_Scripts/Camera/RTSCameraController.cs` | 엣지 스크롤 + 줌 (New Input System) |

---

## 씬 구성 (GameScene)

| 오브젝트 | instanceID | 역할 |
|----------|-----------|------|
| `Main Camera` | 51730 | URP Base Camera, Orthographic, CinemachineBrain, Physics2DRaycaster |
| `RTSVirtualCamera` | 51712 | CinemachineCamera + CinemachineFollow (Z=-10) |
| `CameraTarget` | 51776 | RTSCameraController 부착 |
| `EventSystem` | 51720 | InputSystemUIInputModule |
| `GridSystem` | 51802 | 8×12 그리드 |
| `Canvas` | 51744 | UIManager 컴포넌트 직접 부착 |
| `BuildingPanel` | 51762 | UIPanelPopup + ConstructManager (_database=BuildingDatabase.asset) |
| `Button` | 51790 | BuildButton (_buildingId=0), 자식 Text |
| `ArchitectureManager` | -12408 | 빌딩 레지스트리, 설치된 빌딩들의 부모 |

---

## 리소스

| 경로 | 내용 |
|------|------|
| `Assets/1_Sources/IMG/GridFrame.png` | 290×290px 그리드 셀 스프라이트, PPU=100 |
| `Assets/1_Sources/PSB/CPU.psb` | CPU 스프라이트 PSB (리깅), documentPivot=(0.5,0.5) Center |
| `Assets/1_Sources/Prefabs/CPU.prefab` | CPU 프리팹 (root+Animator, child Model at (0,0,0)) |
| `Assets/1_Sources/BuildingDatabase.asset` | id=0: CPU |
| `Assets/GridMaterial.mat` | Sprites/Default, GPU Instancing |

---

## GridSystem 설정값

```
_rows = 8, _cols = 12
_cellSize = 2.9, _cellSpacing = 0.1
_gridSprite = GridFrame, _gridMaterial = GridMaterial
```

---

## 입력 체계

| 입력 | 동작 |
|------|------|
| 좌클릭 그리드 | 그리드 활성화 (`OnGridActivated` 발생) |
| B 키 | BuildingPanel 토글 (활성 그리드 있을 때 열기 / 열린 상태면 닫기) |
| 우클릭 | 뒤로가기 — `Grid.ActiveGrid?.Deactivate()` → `OnGridDeactivated` → 패널 자동 닫힘 |
| 패널 열린 상태 + 다른 그리드 좌클릭 | SwitchTo로 패널 이동 |
| 패널 밖 좌클릭 (그리드 위 아님) | 패널 닫기 |

---

## Grid static events / properties

```csharp
OnGridActivated(Grid)   // 좌클릭 활성화
OnGridDeactivated(Grid) // Deactivate() 호출
ActiveGrid              // 현재 활성화된 Grid (null=없음)
```

---

## ArchitectureManager API

```csharp
Register(Grid, ArchitectureNode)
Unregister(Grid)
GetBuilding(Grid) → ArchitectureNode or null
HasBuilding(Grid) → bool
```

---

## Grid↔Building 스케일/정렬 동기화

| 상태 | Grid sortingOrder | Building sortingOrder |
|------|------------------|-----------------------|
| 기본 | +0 | +1 |
| Hover | +2~+3 진행 | +1 (스케일만 1.06×) |
| Activated | +3 | +4 |
| Deactivated | +0 | +1 |

---

## 완료된 작업 이력

### 1~8. (2026-05-26까지)
그리드 시스템, MCP 씬 세팅, hover 이벤트, 카메라 버그, 화면 찢김, alpha/scale 애니메이션 완료.

### 9. Grid UX 개선 (2026-05-28)
- UIPanelPopup Open/Close/SwitchTo 코루틴 완성
- UIManager Grid.OnGridRightClicked 구독, 패널 외부 좌클릭 닫기
- ConstructManager BuildButton static event 기반 건설 시스템
- BuildingDatabase SO (id=0: CPU)

### 10. Grid UX 2차 개선 (2026-05-30)
- **Hover 미세 스케일** (`_hoverMicroScale=1.06`, SmoothStep 0.18s)
- **활성화 상태 Alpha/Scale 유지** (`_isHovered` 플래그, `OnPointerExit` early return)
- **다른 그리드 클릭 시 이전 그리드 alpha 복귀** (`Deactivate()`에서 `!_isHovered → AnimateAlpha(_baseAlpha)`)

### 11. BuildingPanel 동작 개선 (2026-05-30)
- **패널 SwitchTo 이동**: 다른 그리드 좌클릭 시 `OnGridActivated` → `OnAnyGridActivated` → SwitchTo
- **IsMouseOverGrid()**: Physics2D.OverlapPoint로 그리드 위 좌클릭 시 닫기 차단
- `OnGridRightClicked` 제거, `OnGridDeactivated` 추가

### 12. 입력 체계 개편 (2026-05-30)
- B키 = BuildingPanel 토글
- 우클릭 = 뒤로가기 (`Grid.ActiveGrid?.Deactivate()`)
- `Grid.ActiveGrid` public 프로퍼티 노출
- `UIManager.CloseBuildingPanel`: _currentGrid 먼저 null 처리 (이중 Close 방지)

### 13. ArchitectureManager + ConstructManager (2026-05-30)
- ArchitectureManager: 싱글톤, Dictionary<Grid, ArchitectureNode> 레지스트리
- ConstructManager: ArchitectureManager 자식으로 빌딩 배치, HasBuilding 중복 방지

### 14. CPU 위치 수정 (2026-05-30)
- CPU.prefab Model child position → (0, 0, 0) (기존 (-0.29, -0.62) 제거)
- CPU.psb.meta: `documentPivot → (0.5, 0.5)`, `documentAlignment → 0 (Center)`, `characterData.pivot → (0.5, 0.5)`

### 15. 빌딩 스케일/정렬 동기화 (2026-05-30)
- ArchitectureNode: `BaseScale`, `AnimateScale(target, dur, curve)`, `SetSortingOrder(order)`
- Grid: `GetBuilding()` 헬퍼, AnimateHoverScale/AnimateScale에서 빌딩 동기화 호출

---

## 알려진 이슈 / 주의사항

- `CameraTarget`이 Play 모드 전환 시 이전 위치를 기억함 → 카메라가 그리드 밖을 가리킬 수 있음
- `Grid` 클래스명이 `UnityEngine.Grid`(Tilemap)와 잠재적 충돌 → 향후 namespace `ArchiSlave` 추가 고려
- `CinemachineFollow.PositionDamping = (1,1,1)` → 카메라 이동에 댐핑 있음, RTS 스타일이면 (0,0,0) 고려
- CPU.png는 현재 CPU 프리팹에서 사용되지 않음 (CPU.psb가 실제 소스)

---

## 다음 작업 후보

- 설치된 노드 클릭 시 상세 정보/관리 UI
- 건물 추가 (RAM, GPU 등 id=1, 2...)
- 노드 간 연결(엣지) 시각화
- BuildingPanel UI 디자인 (버튼 레이아웃, 아이콘)
- 그리드 점유 시각화 (건물 있는 그리드 표시)

---

*마지막 업데이트: 2026-05-30*
