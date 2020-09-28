using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public enum WidthMode { Homogeneous, Fix, BestFit,Empty }
public enum HeightMode { Homogeneous, Fix, BestFit,Empty }
public class JSONGrid : MonoBehaviour
{


    private const string HEADERS_STRING = "ColumnHeaders";
    private const string DATA_STRING = "Data";
    private const string TITLE_STRING = "Title";

    [Tooltip("Path of the JSON to load grid from.")]
    [SerializeField] private string _jsonPath = "";
    [SerializeField] private GridLayout _horizontalGridPrefab = null;
    [SerializeField] private TMP_Text _titleText = null;

    [SerializeField] private GameObject _headerPrefab = null;
    [SerializeField] private GameObject _dataPrefab = null;
    [SerializeField] private GameObject _verticalGridPrefab = null;

    [Space()]
    [Tooltip("Number of Columns To fetch from JSON.")]
    [SerializeField] public int FetchColumns;
    private int _showingColumns;
    [Tooltip("Number of Rows To fetch from JSON.")]
    [SerializeField] public int FetchRows;
    private int _showingRows;

    [Tooltip("Width fit mode, Homogeneous: Same Width using size of grid,Fixed:Same Width input by user,BestFit: Proportional to words on column.")]
    [SerializeField] WidthMode WidthMode = WidthMode.Homogeneous;
    [Tooltip("Height fit mode, Homogeneous: Same Width using size of grid,Fixed:Same Width input by user,BestFit: Proportional to words on row.")]
    [SerializeField] HeightMode HeightMode = HeightMode.Homogeneous;
    [Tooltip("Spacing of rows and columns.")]
    [SerializeField] Vector2 Spacing = Vector2.zero;
    private Vector2 _Spacing;

    [Tooltip("Fixed Size of rows height, needs to be in FixedMode.")]
    [SerializeField] float FixedHeight;
    private float _fixedHeight;
    [Tooltip("Fixed Size of columns width, needs to be in FixedMode.")]
    [SerializeField] float FixedWidth;
    private float _fixedWidth;





    private WidthMode _widthMode;
    private HeightMode _heightMode;
    /// <summary>
    /// node of Json data.
    /// </summary>
    private JSONNode node;

    private List<List<GameObject>> _gridOfTextElements;
    private List<GameObject> _verticalGrids;
    private List<float> _avgLenghtWords;
    private List<float> _avgLenghtRow;



    #region MonoBehaviour
    private void Awake()
    {
        _verticalGrids = new List<GameObject>();
        _gridOfTextElements = new List<List<GameObject>>();
        _avgLenghtWords = new List<float>();
        _avgLenghtRow = new List<float>();
        BuildGrid();
    }

    public void LateUpdate()
    {
        if (Spacing != _Spacing) {
            AdjustRowSpacing();
        }
        if (FetchColumns != _showingColumns)
        {
            ReAdjustShowingColumns();
        }
        if (FetchRows != _showingRows)
        {
           ReAdjustShowingRows();
        }
        if (HeightMode != _heightMode)
        {
            ReSizeHeight();
        }
        if (WidthMode != _widthMode)
        {
            ReSizeWidth();
        }

        if (FixedHeight != _fixedHeight && HeightMode == HeightMode.Fix)
        {
            ReSizeHeight();
        }

        if (FixedWidth != _fixedWidth && WidthMode == WidthMode.Fix)
        {
            ReSizeWidth();
        }
    }
    #endregion


    #region Class Methods

    /// <summary>
    /// Parse a JSON file in the path _jsonPath to a JSONNode Object.
    /// </summary>
    /// <returns></returns>


    public JSONNode SerializeJson()
    {
        using (StreamReader stream = new StreamReader(Application.dataPath + _jsonPath))
        {
            string json = stream.ReadToEnd();
            return JSON.Parse(json);
        }
    }
  
    /// <summary>
    /// Build Grid Creates the node from json and creates a grid of text elements from that, its linked to a button on screen.
    /// </summary>
    public void BuildGrid() {
        node = null;
        node = SerializeJson();
        int totalColumns = node[HEADERS_STRING].Count;
        FetchColumns = totalColumns;
        int totalRows = node[DATA_STRING].Count;
        FetchRows = totalRows;
        CleanGrid();
        CreateGrid(totalRows,totalColumns,node[TITLE_STRING]);

        HeightMode = HeightMode.Homogeneous;
        WidthMode = WidthMode.Homogeneous;
    }
    
    /// <summary>
    /// Cleans the last Grid.
    /// </summary>
    public void CleanGrid() {
        if (_verticalGrids!=null) {
            foreach (GameObject go in _verticalGrids) {
                Destroy(go);
            }
        }
        _horizontalGridPrefab.CalculateLayoutInputHorizontal();
        _heightMode = HeightMode.Empty;
        _widthMode = WidthMode.Empty;
    }
    /// <summary>
    /// Creates a grid from the node of json.
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="columns"></param>
    /// <param name="title"></param>
    private void CreateGrid(int rows,int columns,string title) {
        LoadTitle(title);
        CreateVerticalGrids(columns,rows);
        CreateTextElements(columns, rows);
    }
    /// <summary>
    /// Load title of the grid.
    /// </summary>
    /// <param name="title"></param>
    public void LoadTitle(string title)
    {
        if (_titleText != null) {
            _titleText.text = title;
        }
        else {
            Debug.LogError("No title text");
        }
    }
    /// <summary>
    /// Creates Vertical Components of grid.
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="rows"></param>
    private void CreateVerticalGrids(int columns,int rows) {
        _horizontalGridPrefab.Columns = columns;
        _horizontalGridPrefab.Rows = rows;
        _verticalGrids.Clear();
        _avgLenghtWords.Clear();
        for (int i = 0; i<columns; i++) {
            GameObject go = Instantiate(_verticalGridPrefab, _horizontalGridPrefab.transform);
            _avgLenghtWords.Add(GetAVGLenghtWords(i));
            _verticalGrids.Add(go);
            GridLayout grid = go.GetComponent<GridLayout>();
            grid.Rows = rows;
            grid.Columns = 1;
        }
        if (WidthMode==WidthMode.Homogeneous) {
            SetHomogeneousColumnWidth(true);
        }
        if (HeightMode==HeightMode.Homogeneous) {
            SetHomogeneousRowHeight(true);
        }
    }
    /// <summary>
    /// Create Text elements that hold the grid text information.
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="rows"></param>
    private void CreateTextElements(int columns, int rows) {
        _gridOfTextElements.Clear();
        _avgLenghtRow.Clear();
        for (int i = 0; i <= rows; i++) {
            _gridOfTextElements.Add(new List<GameObject>());
            for (int j = 0; j < columns; j++) {
                if (i == 0)
                {
                    _gridOfTextElements[i].Add(Instantiate(_headerPrefab, _verticalGrids[j].transform));
                }
                else {
                    _gridOfTextElements[i].Add(Instantiate(_dataPrefab, _verticalGrids[j].transform));
                }
                
                TMP_Text text = _gridOfTextElements[i][j].GetComponentInChildren<TMP_Text>();

                if (text != null) {
                    if (i == 0) {
                        text.text = node[HEADERS_STRING][j];
                    }
                    else {
                        text.text = node[DATA_STRING][i-1][j];
                    }    
                }
                else {
                    Debug.LogError("Data prefab without text");
                }
            }
            _avgLenghtRow.Add(GetAvgLengthRow(i)); 
        }
    }


    /// <summary>
    /// Resize the height of the grid, depends on the mode of resize.
    /// </summary>
    private void ReSizeHeight() {
        _fixedHeight = FixedHeight;
        _fixedWidth = FixedWidth;
        _heightMode = HeightMode;
        switch (HeightMode) {
            case HeightMode.Homogeneous:
                SetHomogeneousRowHeight(true);
                break;
            case HeightMode.Fix:
                SetFixedValues();
                break;
            case HeightMode.BestFit:
                BestFitRowsHeight();
                break;
        }
    }
    /// <summary>
    /// Resize Width of grid, depends on the mode of resize.
    /// </summary>
    public void ReSizeWidth() {
        _fixedWidth = FixedWidth;
        _fixedHeight = FixedHeight;
        _widthMode = WidthMode;
        switch (WidthMode) {
            case WidthMode.Homogeneous:
                SetHomogeneousColumnWidth(true);
                break;
            case WidthMode.Fix:
                SetFixedValues();
                break;

            case WidthMode.BestFit:
                BestFitColumnWidth();
                break;
        }
    }
    /// <summary>
    /// Adjust the amount of rows to show on the grid.
    /// </summary>
    private void ReAdjustShowingRows() {
        _showingRows = FetchRows;
        if (FetchRows > 20) {
            FetchRows = 20;
        }
        _horizontalGridPrefab.Rows = _showingRows;
        FetchRows = (FetchRows > _gridOfTextElements.Count) ? _gridOfTextElements.Count : FetchRows;
        FetchRows = (FetchRows < 0) ?  0 :FetchRows;
        if (_showingRows >= 0 && _showingRows <= _gridOfTextElements.Count) {
            for (int i = 1; i < _gridOfTextElements.Count; i++) {
                List<GameObject> row = _gridOfTextElements[i];
                if (i > _showingRows) {
                    foreach (GameObject go in row) {
                        go.SetActive(false);
                    }
                }
                else {
                    foreach (GameObject go in row) {
                        go.SetActive(true);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Adjust the amount of columns to show on the grid.
    /// </summary>
    private void ReAdjustShowingColumns() {
        _showingColumns = FetchColumns;
        if (FetchColumns > 15) {
            FetchColumns = 15;
        }
        _horizontalGridPrefab.Columns = _showingColumns;
        FetchColumns = (FetchColumns > _verticalGrids.Count) ? _verticalGrids.Count : FetchColumns;
        FetchColumns = (FetchColumns < 0) ? 0 : FetchColumns;

        if (_showingColumns >= 0 &&_showingColumns<= _verticalGrids.Count) {
            for (int j = 0; j < _verticalGrids.Count; j++) {
                if (j >= _showingColumns) {
                    _verticalGrids[j].SetActive(false);
                }
                else {
                    _verticalGrids[j].SetActive(true);
                }
            }
            _horizontalGridPrefab.CalculateLayoutInputHorizontal();
        }
    }
    /// <summary>
    /// Set Columns width to a homogeneous value.
    /// </summary>
    /// <param name="val"></param>
    private void SetHomogeneousColumnWidth(bool val) {
        _horizontalGridPrefab.UniqueX = false;
        FixedWidth = _horizontalGridPrefab.CellSize.x;
 
        for (int j = 0; j < _verticalGrids.Count; j++) {
             GridLayout verticalGrid = _verticalGrids[j].GetComponent<GridLayout>();
          verticalGrid.FitX = val;
        }
        _horizontalGridPrefab.FitX = val;
        _horizontalGridPrefab.CalculateLayoutInputHorizontal();
    }
    /// <summary>
    /// Sets the rows Heights to a homogeneous value.
    /// </summary>
    /// <param name="val"></param>
    private void SetHomogeneousRowHeight(bool val) {
        
        foreach (GameObject go in _verticalGrids)
        {
            GridLayout grid = go.GetComponent<GridLayout>();
            grid.UniqueY = false;
        }
        _horizontalGridPrefab.UniqueX = false;
        FixedHeight = _horizontalGridPrefab.CellSize.y;
        for (int j = 0; j < _verticalGrids.Count; j++) {
            GridLayout verticalGrid = _verticalGrids[j].GetComponent<GridLayout>();
            verticalGrid.FitY = val;
            verticalGrid.CalculateLayoutInputHorizontal();
        }
        _horizontalGridPrefab.FitY = val;
        _horizontalGridPrefab.CalculateLayoutInputHorizontal();
    }
    /// <summary>
    /// Sets the columns and/or height to a Fixed Value exposed in the editor.
    /// </summary>
    private void SetFixedValues() {
        foreach (GameObject go in _verticalGrids)
        {
            GridLayout grid = go.GetComponent<GridLayout>();
            grid.UniqueY = false;
        } 
            _horizontalGridPrefab.UniqueX = false;
        if (HeightMode == HeightMode.Fix) {
            _horizontalGridPrefab.FitY = false;
            if (WidthMode==WidthMode.Fix) {
                _horizontalGridPrefab.FitX = false;
                _horizontalGridPrefab.CellSize = new Vector2(FixedWidth, FixedHeight);
            }
            else {
                _horizontalGridPrefab.FitX = true;
                _horizontalGridPrefab.CellSize = new Vector2(_horizontalGridPrefab.CellSize.x, FixedHeight);
               
            }
            foreach (GameObject go in _verticalGrids) {
                GridLayout grid = go.GetComponent<GridLayout>();
                grid.CalculateLayoutInputHorizontal();
            }
        }
        else if (WidthMode == WidthMode.Fix) {
            _horizontalGridPrefab.FitX = false;
            _horizontalGridPrefab.FitY = true;
            _horizontalGridPrefab.CellSize = new Vector2(FixedWidth, _horizontalGridPrefab.CellSize.y);
            
        }
        _horizontalGridPrefab.CalculateLayoutInputHorizontal();
        Canvas.ForceUpdateCanvases();
        _horizontalGridPrefab.SetLayoutVertical();
    }
    /// <summary>
    /// Avg word amount of row.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private float GetAvgLengthRow(int row) {
        float t = 0;
        if (_gridOfTextElements[row] != null) {
            foreach (GameObject go in _gridOfTextElements[row]) {
                TMP_Text text = go.GetComponentInChildren<TMP_Text>();
                t += text.text.Length;
            }
            if (_gridOfTextElements[row].Count != 0) {
                t = t / _gridOfTextElements[row].Count;
            }
            else {
                t = 0;
            }
        }
        return t;
    }
    /// <summary>
    /// Avg word count of column
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    private float GetAVGLenghtWords(int column)
    {
        string v = node[HEADERS_STRING][column];
        int k = v.Length;
        for (int i = 0; i < _gridOfTextElements.Count-1; i++)
        {
            v = node[DATA_STRING][i][column];
            k += v.Length;
        }
        if (_gridOfTextElements.Count != 0)
        {
            k = k / _gridOfTextElements.Count;
        }
        else {
            k = v.Length;
        }
        return k;
    }
    /// <summary>
    /// Best Fit of column width its proportional to amount of text in that particular column.
    /// </summary>
    private void BestFitColumnWidth() {
        _horizontalGridPrefab.FitX = false;
        _horizontalGridPrefab.UniqueX = true;
        int i = 0;
        foreach (GameObject go in _verticalGrids)
        {
            GridLayout grid = go.GetComponent<GridLayout>();
            RectTransform rect = grid.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(Mathf.Min(_avgLenghtWords[i] * 20f + 50, 350f),grid.CellSize.y);
            grid.CalculateLayoutInputHorizontal();
            i++;
        }
        _horizontalGridPrefab.CalculateLayoutInputHorizontal();
    }
    /// <summary>
    /// Best Fit Height its proportional to amount of thext in that particular row.
    /// </summary>
    private void BestFitRowsHeight() {
        _horizontalGridPrefab.FitY = false;
        foreach (GameObject go in _verticalGrids) {
            GridLayout grid = go.GetComponent<GridLayout>();         
            grid.UniqueY = true;
            if (_showingRows >= 0 && _showingRows <= _gridOfTextElements.Count)
            {
                for (int i = 1; i < _gridOfTextElements.Count; i++)
                {
                    List<GameObject> row = _gridOfTextElements[i];
                    foreach (GameObject rowElement in row)
                    {
                        RectTransform rect = rowElement.GetComponent<RectTransform>();
                        rect.sizeDelta = new Vector2(grid.CellSize.x, Mathf.Min(_avgLenghtRow[i] * 13f, 200f));
                    }
                }
            }
            grid.CalculateLayoutInputHorizontal();
        }
        _horizontalGridPrefab.CalculateLayoutInputHorizontal();
    }

    /// <summary>
    /// Adjust Spacing of grid.
    /// </summary>
    private void AdjustRowSpacing() {
        _Spacing = Spacing;
        foreach (GameObject go in _verticalGrids)
        {
            GridLayout grid = go.GetComponent<GridLayout>();
            grid.FitY = false;
            grid.Spacing =new Vector2(grid.Spacing.x ,Spacing.y);
            grid.CalculateLayoutInputHorizontal();
        }
        if (Spacing.x < 0)
        {
            Spacing.x = 0;
        }
        _horizontalGridPrefab.Spacing = new Vector2(Spacing.x, _horizontalGridPrefab.Spacing.y);
        _horizontalGridPrefab.CalculateLayoutInputHorizontal();
    }

    #endregion
}
