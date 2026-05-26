using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("그리드 크기")]
    [SerializeField] private int _rows = 5;
    [SerializeField] private int _cols = 5;

    [Header("셀 설정")]
    [Tooltip("월드 단위 셀 크기. GridFrame PPU=100 기준 2.9 권장")]
    [SerializeField] private float _cellSize = 2.9f;
    [Tooltip("셀 간 간격")]
    [SerializeField] private float _cellSpacing = 0.1f;

    [Header("리소스 — GridFrame 스프라이트와 공유 Material 할당")]
    [SerializeField] private Sprite _gridSprite;
    [SerializeField] private Material _gridMaterial;

    // 공개 접근자
    public int Rows => _rows;
    public int Cols => _cols;

    private Grid[,] _grids;

    private void Start() => BuildGrid();

    private void BuildGrid()
    {
        _grids = new Grid[_rows, _cols];

        float step = _cellSize + _cellSpacing;
        // 전체 그리드를 GridSystem 오브젝트 중심 기준으로 배치
        Vector3 origin = transform.position
            - new Vector3(step * (_cols - 1) * 0.5f,
                          step * (_rows - 1) * 0.5f, 0f);

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                Vector3 pos = origin + new Vector3(c * step, r * step, 0f);
                _grids[r, c] = CreateCell(r, c, pos);
            }
        }
    }

    private Grid CreateCell(int row, int col, Vector3 worldPos)
    {
        var go = new GameObject($"Grid_{row}_{col}");
        go.transform.SetParent(transform, false);
        go.transform.position = worldPos;

        // SpriteRenderer + BoxCollider2D 는 Grid 의 RequireComponent 로 자동 추가됨
        var cell = go.AddComponent<Grid>();
        cell.Initialize(row, col, _gridSprite, _gridMaterial);
        return cell;
    }

    /// <summary>행·열로 셀 참조 반환. 범위 밖이면 null.</summary>
    public Grid GetCell(int row, int col)
    {
        if (row < 0 || row >= _rows || col < 0 || col >= _cols) return null;
        return _grids[row, col];
    }
}
