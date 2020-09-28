using UnityEngine;
using UnityEngine.UI;

public class GridLayout : LayoutGroup
{
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }
    [Tooltip("Row amount of grid.")]
    [SerializeField] private int _rows;
    [Tooltip("Column amount of grid.")]
    [SerializeField] private int _columns;
    [Tooltip("FitTypes Uniform same size,Width: Custom Width,Height: Custom Height,FixedRows:Fix the amout of rows of grid,FixedColumns:Fix amount of columns")]
    [SerializeField] private FitType _fitType=FitType.FixedColumns;
    [Tooltip("Fit the amount of width of Cells in the grid.")]
    [SerializeField] private bool _fitX;
    [Tooltip("Fit the amount of height of Cells in the grid.")]
    [SerializeField] private bool _fitY;
    [Tooltip("Enable Unique X value of width columns.")]
    [SerializeField] private bool _uniqueXSize;
    [Tooltip("Enable Unique Y value of Row Height.")]
    [SerializeField] private bool _uniqueYSize;
    [Tooltip("Cell Size of grid.")]
    [SerializeField] private Vector2 _cellSize = Vector2.zero;
    [Tooltip("Spacing of grid.")]
    [SerializeField] private Vector2 _spacing = Vector2.zero;
    [SerializeField] private readonly uint _xGridInitialPosition;
    [SerializeField] private readonly uint _yGridInitialPosition;
    public Vector2 CellSize { get => _cellSize; set => _cellSize = value; }
    public Vector2 Spacing { get => _spacing; set => _spacing = value; }
    public int Rows { get => _rows; set => _rows = value; }
    public int Columns { get => _columns; set => _columns = value; }
    public int GridElementsCount { get => rectChildren.Count; }

    public bool FitX { get => _fitX; set => _fitX = value; }
    public bool FitY { get => _fitY; set => _fitY = value; }

    public bool UniqueX { get => _uniqueXSize; set => _uniqueXSize = value; }

    public bool UniqueY { get => _uniqueYSize; set => _uniqueYSize = value; }
    /// <summary>
    /// Return the x,y rect transform of the grid.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public RectTransform childRectransform(int x, int y)
    {
        if (y > 0 && x > 0)
        {
            return rectChildren[(x * _columns) + y];
        }
        else if (y > 0 && x == 0)
        {
            return rectChildren[y];
        }
        else if (y == 0 && x > 0)
        {
            return rectChildren[x * _columns];
        }
        else if (y == 0 && x == 0)
        {
            return rectChildren[0];
        }
        else
        {
            Debug.LogError("cant find rect children");
            return null;
        }

    }
    /// <summary>
    /// Enabled transforms childs of Grid.
    /// </summary>
    /// <returns></returns>
    private int enableChild() {
        int ret=0;
        foreach (RectTransform rect in rectChildren) {
            if (rect.gameObject.activeSelf) {
                ret++;
            }
        }
        return ret;
    }
    /// <summary>
    /// Re organize the grid dinamically.
    /// </summary>
    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        if (_fitType == FitType.Width || _fitType == FitType.Height || _fitType == FitType.Uniform)
        {
            _fitX = true;
            _fitY = true;
            //float sqrRt = Mathf.Sqrt(transform.childCount);
            float sqrRt = Mathf.Sqrt(enableChild());
            _rows = Mathf.CeilToInt(sqrRt);
            _columns = Mathf.CeilToInt(sqrRt);
        }

        if (_fitType == FitType.Width || _fitType == FitType.FixedColumns)
        {
            if (_columns != 0) {
                _rows = Mathf.CeilToInt(enableChild() / (float)_columns);
            }
            
        }
        if (_fitType == FitType.Height || _fitType == FitType.FixedRows)
        {
            if (_rows != 0) {
                _columns = Mathf.CeilToInt(enableChild() / (float)_rows);
            }
            
        }
        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;
        float cellWidth=0;
        float cellHeight = 0;
        if (_columns != 0)
        {
            cellWidth = _fitX ? ((parentWidth / _columns) - ((_spacing.x / _columns) * (_columns - 1)) - (padding.left / _columns) - (padding.right / _columns)) : _cellSize.x;

        }

        if (_rows != 0) {
            cellHeight = _fitY ? ((parentHeight / _rows) - ((_spacing.y / _rows) * (_rows - 1)) - (padding.bottom / _rows)) : _cellSize.y;
        }

        _cellSize = new Vector2(cellWidth, cellHeight);
        var xPos = 0f;
        var yPos = 0f;
        int rowCount = 0;
        int columnCount = 0;
        for (int i = 0; i < enableChild(); i++)
        {
            if (_columns != 0)
            {
                rowCount = i / _columns;
                columnCount = i % _columns;
            }
            
           
            var item = rectChildren[i];
           
            if (!UniqueX)
            {
                xPos = _cellSize.x * columnCount + (_spacing.x * columnCount) + padding.left;
            }

            if (!UniqueY) {
                yPos = _cellSize.y * rowCount + (_spacing.y * rowCount) + padding.top;
            }

           
            if (!UniqueX)
            {
                SetChildAlongAxis(item, 0, xPos, _cellSize.x);
            }
            else {
                SetChildAlongAxis(item, 0, xPos);
                xPos += rectChildren[i].rect.width + (_spacing.x * columnCount) + padding.left;
            }

            if (!UniqueY)
            {
                SetChildAlongAxis(item, 1, yPos, _cellSize.y);
            }
            else {
                SetChildAlongAxis(item, 1, yPos);
                yPos += rectChildren[i].rect.height + (_spacing.y * rowCount) + padding.top;
            }
        }

    }


    public override void CalculateLayoutInputVertical()
    {

    }

    public override void SetLayoutHorizontal()
    {

    }

    public override void SetLayoutVertical()
    {

    }

}
