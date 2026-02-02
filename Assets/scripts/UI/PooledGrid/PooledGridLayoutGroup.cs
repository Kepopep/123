using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PooledGridLayoutGroup : LayoutGroup
{
    [Header("Grid Settings")]
    [SerializeField] 
    private int _columns = 2;

    [SerializeField] 
    private int _rows = 3;
   
    [SerializeField] 
    private Vector2 _cellSize = new Vector2(100, 100);
   
    [SerializeField] 
    private Vector2 _spacing = new Vector2(10, 10);
   
    [SerializeField] 
    private Vector2 _padding = new Vector2(10, 10);

    [Header("Virtualization Settings")]
    [SerializeField] 
    private GameObject _itemPrefab;
    
    [SerializeField] 
    private int _bufferSize = 2; 

    [Header("Scroll Settings")]
    [SerializeField] 
    private ScrollRect scrollRect;

    private List<GameObject> pooledItems = new List<GameObject>();
    private List<GridElement> gridElements = new List<GridElement>();
    private Dictionary<int, GridElement> visibleItems = new Dictionary<int, GridElement>();

    private int totalItemCount = 0;
    private int startIndex = 0;
    private int endIndex = 0;

    // Properties
    public int Columns { get { return _columns; } set { _columns = value; SetDirty(); } }
    public int Rows { get { return _rows; } set { _rows = value; SetDirty(); } }
    public int TotalItemCount { get { return totalItemCount; } }

    protected override void Start()
    {
        base.Start();
        InitializePool();
        SetupContentRect();
    }

    protected override void OnDestroy()
    {
        if (scrollRect == null)
        {
            Debug.LogWarning("ScrollRect is not assigned!", this);
            return;
        }

        DeinitializePool();

    }

    private void LateUpdate()
    {
        if(scrollRect != null && scrollRect.content != null &&
            scrollRect.content.anchoredPosition.y < 0)
        {
            scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0);
            scrollRect.velocity = Vector2.zero;
        }
    }

    private void SetupContentRect()
    {
        var width = (_cellSize.x * _columns) + (_spacing.x * (_columns - 1)) + (_padding.x * 2);
        var actualRowsNeeded = Mathf.CeilToInt((float)totalItemCount / _columns);
        var height = (_cellSize.y * actualRowsNeeded) + (_spacing.y * (actualRowsNeeded - 1)) + (_padding.y * 2);

        scrollRect.content.sizeDelta = new Vector2(width, height);
        scrollRect.content.anchoredPosition = new Vector2(-width / 2f, 0);
        scrollRect.content.pivot = new Vector2(0, 1);
    }

    private void InitializePool()
    {
        if (_itemPrefab == null)
        {
            Debug.LogError("Item prefab is not assigned!", this);
            return;
        }

        if (scrollRect == null)
        {
            Debug.LogWarning("ScrollRect is not assigned!", this);
            return;
        }

        totalItemCount = _columns * _rows;
        scrollRect.onValueChanged.AddListener(OnScrollChanged);

        CreateItemPool();
        UpdateVisibleItems();
    }

    private void DeinitializePool()
    {
        pooledItems.Clear();
        gridElements.Clear();
        visibleItems.Clear();

        var poolSize = (_columns * _rows) + (_bufferSize * 2);

        for (int i = scrollRect.content.childCount - 1; i >= 0; i--)
        {
            var child = scrollRect.content.GetChild(i);

            Destroy(child.gameObject);
        }

        scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }

    private void CreateItemPool()
    {
        if(!Application.isPlaying)
        {
            return;
        }
        
        pooledItems.Clear();
        gridElements.Clear();
        visibleItems.Clear();
        
        var poolSize = (_columns * _rows) + (_bufferSize * 2);
        for (int i = 0; i < poolSize; i++)
        {
            GameObject item = Instantiate(_itemPrefab, scrollRect.content);
            item.SetActive(false);
            
            GridElement gridElement = item.GetComponent<GridElement>();
            if (gridElement == null)
            {
                gridElement = item.AddComponent<GridElement>();
            }
            
            pooledItems.Add(item);
            gridElements.Add(gridElement);
        }
    }

    private void OnScrollChanged(Vector2 scrollPosition)
    {
        UpdateVisibleItems();
    }
    

    private void UpdateVisibleItems()
    {
        if (pooledItems.Count == 0 || _itemPrefab == null)
        {
            return;
        }

        CalculateVisibleRange();

        var keysToRemove = new List<int>();
        foreach (KeyValuePair<int, GridElement> pair in visibleItems)
        {
            if (pair.Key < startIndex || pair.Key > endIndex)
            {
                ReturnItemToPool(pair.Value);
                keysToRemove.Add(pair.Key);
            }
        }

        foreach (int key in keysToRemove)
        {
            visibleItems.Remove(key);
        }

        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i >= 0 && !visibleItems.ContainsKey(i))
            {
                GridElement item = GetPooledItem();
                if (item != null)
                {
                    item.SetData(i);
                    item.SetActive(true);
                    
                    var row = i / _columns;
                    var col = i % _columns;
                    RectTransform rectTransform = item.GetComponent<RectTransform>();
                    
                    var xPos = _padding.x + (_cellSize.x + _spacing.x) * col + (_cellSize.x * 0.5f);
                    var yPos = -_padding.y - (_cellSize.y + _spacing.y) * row - (_cellSize.y * 0.5f);
                    
                    rectTransform.anchoredPosition = new Vector2(xPos, yPos);
                    rectTransform.sizeDelta = _cellSize;
                    
                    visibleItems[i] = item;
                }
            }
        }
    }

    private void CalculateVisibleRange()
    {
        var viewportRect = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;

        var viewportCorners = new Vector3[4];
        viewportRect.GetWorldCorners(viewportCorners);

        for (int i = 0; i < 4; i++)
        {
            viewportCorners[i] = scrollRect.content.InverseTransformPoint(viewportCorners[i]);
        }

        var minX = Mathf.Min(viewportCorners[0].x, viewportCorners[2].x);
        var maxX = Mathf.Max(viewportCorners[0].x, viewportCorners[2].x);
        var minY = Mathf.Min(viewportCorners[0].y, viewportCorners[2].y);
        var maxY = Mathf.Max(viewportCorners[0].y, viewportCorners[2].y);

        var minCol = Mathf.Min(0, Mathf.FloorToInt(Mathf.Max(0, minX - _padding.x) / (_cellSize.x + _spacing.x)));
        var maxCol = Mathf.Max(_columns - 1, Mathf.FloorToInt((maxX - _padding.x) / (_cellSize.x + _spacing.x)));
        
        var minRow = Mathf.Max(0, Mathf.FloorToInt(Mathf.Max(0, -maxY - _padding.y) / (_cellSize.y + _spacing.y)));
        var maxRow = Mathf.Max(_rows - 1, Mathf.FloorToInt((-minY - _padding.y) / (_cellSize.y + _spacing.y)));

        startIndex = Mathf.Max(0, (minRow * _columns) + minCol); 
        endIndex = (maxRow * _columns) + maxCol;
    }

    private GridElement GetPooledItem()
    {
        foreach (GridElement element in gridElements)
        {
            if (!element.IsActive())
            {
                return element;
            }
        }

        var newItem = Instantiate(_itemPrefab, scrollRect.content);
        var newElement = newItem.GetComponent<GridElement>();

        pooledItems.Add(newItem);
        gridElements.Add(newElement);

        return newElement;
    }

    private void ReturnItemToPool(GridElement item)
    {
        if (item != null)
        {
            item.SetActive(false);
            item.SetData(-1); 
        }

    }

    public void SetTotalItemCount(int count)
    {
        totalItemCount = count;
        var newRows = Mathf.CeilToInt((float)count / _columns);
        _rows = newRows;
        UpdateVisibleItems();
    }



    private void CalculateLayout()
    {
        if (!IsActive())
            return;

        SetLayoutInputForChildren();
    }

    private float GetGreatestMinimumChildWidth()
    {
        var maxMinWidth = 0f;
        for (int i = 0; i < rectChildren.Count; i++)
        {
            maxMinWidth = Mathf.Max(maxMinWidth, LayoutUtility.GetMinWidth(rectChildren[i]));
        }
        return maxMinWidth;
    }

    private float GetGreatestMinimumChildHeight()
    {
        var maxMinHeight = 0f;
        for (int i = 0; i < rectChildren.Count; i++)
        {
            maxMinHeight = Mathf.Max(maxMinHeight, LayoutUtility.GetMinHeight(rectChildren[i]));
        }
        return maxMinHeight;
    }

    private void SetLayoutInputForChildren()
    {
        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform rect = rectChildren[i];

            m_Tracker.Add(this, rect,
                DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.SizeDeltaY);

            rect.sizeDelta = _cellSize;
        }
    }


    #region LayoutGroup
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if (IsActive())
        {
            UpdateVisibleItems();
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!Application.isPlaying && transform.childCount == 0)
        {
            _columns = Mathf.Max(1, _columns);
            _rows = Mathf.Max(1, _rows);
        }
    }
#endif

    public override void CalculateLayoutInputHorizontal()
    {
        CalculateLayout();
    }

    public override void CalculateLayoutInputVertical()
    {
        CalculateLayout();
    }

    public override float minWidth { get { return GetGreatestMinimumChildWidth() + _padding.x * 2; } }
    public override float preferredWidth { get { return (_cellSize.x * _columns) + (_spacing.x * (_columns - 1)) + (_padding.x * 2); } }
    public override float flexibleWidth { get { return -1; } }

    public override float minHeight { get { return GetGreatestMinimumChildHeight() + _padding.y * 2; } }
    public override float preferredHeight { get { return (_cellSize.y * _rows) + (_spacing.y * (_rows - 1)) + (_padding.y * 2); } }
    public override float flexibleHeight { get { return -1; } }

    public override int layoutPriority { get { return 0; } }

    public override void SetLayoutHorizontal()
    {
        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform rect = rectChildren[i];
        }
    }

    public override void SetLayoutVertical()
    {
        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform rect = rectChildren[i];
        }
    }

    #endregion
}