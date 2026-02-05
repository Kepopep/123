using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

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

    [SerializeField]
    private GridContentProvider _provider;

    [Header("Scroll Settings")]
    [SerializeField]
    private ScrollRect _scrollRect;

    private List<GameObject> _pooledItems = new List<GameObject>();
    private List<GridElement> _gridElements = new List<GridElement>();
    private Dictionary<int, GridElement> _visibleItems = new Dictionary<int, GridElement>();

    private (float, float, float, float) _viewPortConrers;

    private int _totalItemCount = 0;
    private int _startIndex = 0;
    private int _endIndex = 0;

    private int _lastElementPosition;
    private Vector3[] _viewportCorners = new Vector3[4];

    public event Action<GridElement, int> OnElementVisualize;

    protected override void Awake()
    {
        if (Screen.width > _cellSize.x * _columns
            && Screen.width > _cellSize.x * (_columns + 1))
        {
            _columns++;
        }
    }

    protected override void Start()
    {
        base.Start();
        InitializePool();
        SetupContentRect();
    }

    protected override void OnDestroy()
    {
        DeinitializePool();
    }

    private void LateUpdate()
    {
        if (_scrollRect.content.anchoredPosition.y < 0)
        {
            _scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, 0);
            _scrollRect.velocity = Vector2.zero;
        }
        else if (_scrollRect.content.anchoredPosition.y > _lastElementPosition)
        {
            _scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, _lastElementPosition);
            _scrollRect.velocity = Vector2.zero;
        }
    }

    private void SetupContentRect()
    {
        var width = (_cellSize.x * _columns) + (_spacing.x * (_columns - 1)) + (_padding.x * 2);
        var actualRowsNeeded = Mathf.CeilToInt((float)_totalItemCount / _columns);
        var height = (_cellSize.y * actualRowsNeeded) + (_spacing.y * (actualRowsNeeded - 1)) + (_padding.y * 2);

        var lastElementRow = (_provider.LastElementNumber - (_rows * _columns)) / _columns;
        _lastElementPosition = (int)(_cellSize.y * lastElementRow + _padding.y + (_spacing.y * lastElementRow));
        _scrollRect.content.sizeDelta = new Vector2(width, height);
        _scrollRect.content.anchoredPosition = new Vector2(-width / 2f, 0);
        _scrollRect.content.pivot = new Vector2(0, 1);

    }

    private void InitializePool()
    {
        _totalItemCount = _columns * _rows;
        _scrollRect.onValueChanged.AddListener(OnScrollChanged);

        CreateItemPool();
        UpdateVisibleItems();
    }

    private void DeinitializePool()
    {
        _pooledItems.Clear();
        _gridElements.Clear();
        _visibleItems.Clear();

        var poolSize = (_columns * _rows) + (_bufferSize * 2);

        for (int i = _scrollRect.content.childCount - 1; i >= 0; i--)
        {
            var child = _scrollRect.content.GetChild(i);

            Destroy(child.gameObject);
        }

        _scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }

    private void CreateItemPool()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        _pooledItems.Clear();
        _gridElements.Clear();
        _visibleItems.Clear();

        var poolSize = (_columns * _rows) + (_bufferSize * 2);
        for (int i = 0; i < poolSize; i++)
        {
            GameObject item = Instantiate(_itemPrefab, _scrollRect.content);
            item.SetActive(false);

            GridElement gridElement = item.GetComponent<GridElement>();
            if (gridElement == null)
            {
                gridElement = item.AddComponent<GridElement>();
            }

            _pooledItems.Add(item);
            _gridElements.Add(gridElement);
        }
    }

    private void OnScrollChanged(Vector2 scrollPosition)
    {
        UpdateVisibleItems();
    }

    private void UpdateVisibleItems()
    {
        if (_pooledItems.Count == 0 || _itemPrefab == null)
        {
            return;
        }

        CalculateVisibleRange();

        var keysToRemove = new List<int>();
        foreach (KeyValuePair<int, GridElement> pair in _visibleItems)
        {
            if (pair.Key < _startIndex || pair.Key > _endIndex)
            {
                ReturnItemToPool(pair.Value);
                keysToRemove.Add(pair.Key);
            }
        }

        foreach (int key in keysToRemove)
        {
            _visibleItems.Remove(key);
        }

        for (int i = _startIndex; i <= _endIndex && i < _provider.LastElementNumber; i++)
        {
            if (_visibleItems.ContainsKey(i))
            {
                continue;
            }

            var item = PositionElement(i);
            OnElementVisualize?.Invoke(item, i);
        }
    }

    private GridElement PositionElement(int i)
    {
        var item = GetPooledItem();

        var row = i / _columns;
        var col = i % _columns;

        var xPos = _padding.x + (_cellSize.x + _spacing.x) * col + (_cellSize.x * 0.5f);
        var yPos = -_padding.y - (_cellSize.y + _spacing.y) * row - (_cellSize.y * 0.5f);

        item.Rect.anchoredPosition = new Vector2(xPos, yPos);
        item.Rect.sizeDelta = _cellSize;

        _visibleItems[i] = item;

        item.SetActive(true);
        return item;
    }

    public void RecalculateVisibleIndex()
    {
        SetupContentRect();
        CalculateVisibleRange();

        for (int i = _startIndex; i <= _endIndex && i < _provider.LastElementNumber; i++)
        {
            if (!_visibleItems.ContainsKey(i))
            {
                var item = PositionElement(i);
                OnElementVisualize?.Invoke(item, i);
                continue;
            }

            OnElementVisualize?.Invoke(_visibleItems[i], i);
        }

        foreach (var item in _visibleItems.Values)
        {
            if (!item.IsActive())
            {
                continue;
            }

            if (!_provider.IsAllow(item.DataIndex))
            {
                item.SetActive(false);
            }
        }
    }


    private void CalculateVisibleRange()
    {
        _scrollRect.viewport.GetWorldCorners(_viewportCorners);

        for (int i = 0; i < 4; i++)
        {
            _viewportCorners[i] = _scrollRect.content.InverseTransformPoint(_viewportCorners[i]);
        }

        _viewPortConrers.Item1 = Mathf.Min(_viewportCorners[0].x, _viewportCorners[2].x); // minx
        _viewPortConrers.Item2 = Mathf.Max(_viewportCorners[0].x, _viewportCorners[2].x); // maxx
        _viewPortConrers.Item3 = Mathf.Min(_viewportCorners[0].y, _viewportCorners[2].y); // minY
        _viewPortConrers.Item4 = Mathf.Max(_viewportCorners[0].y, _viewportCorners[2].y); // maxY

        var minCol = Mathf.Min(0, Mathf.FloorToInt(Mathf.Max(0, _viewPortConrers.Item1 - _padding.x) / (_cellSize.x + _spacing.x)));
        var maxCol = Mathf.Max(_columns - 1, Mathf.FloorToInt((_viewPortConrers.Item2 - _padding.x) / (_cellSize.x + _spacing.x)));

        var minRow = Mathf.Max(0, Mathf.FloorToInt(Mathf.Max(0, -_viewPortConrers.Item4 - _padding.y) / (_cellSize.y + _spacing.y)));
        var maxRow = Mathf.Max(_rows - 1, Mathf.FloorToInt((-_viewPortConrers.Item3 - _padding.y) / (_cellSize.y + _spacing.y)));

        _startIndex = Mathf.Max(0, (minRow * _columns) + minCol);
        _endIndex = Mathf.Clamp((maxRow * _columns) + maxCol, 0, _provider.LastElementNumber);
    }

    private GridElement GetPooledItem()
    {
        foreach (GridElement element in _gridElements)
        {
            if (!element.IsActive())
            {
                return element;
            }
        }

        var newItem = Instantiate(_itemPrefab, _scrollRect.content);
        var newElement = newItem.GetComponent<GridElement>();

        _pooledItems.Add(newItem);
        _gridElements.Add(newElement);

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

    #endregion
}