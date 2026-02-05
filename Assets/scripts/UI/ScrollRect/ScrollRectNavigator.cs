using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ScrollRectNavigator : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField]
    private ScrollRect _scrollRect;
    [SerializeField]
    private RectTransform _contentPanel;

    private float _elementWidth = 2238f;
    private float scrollDuration = 0.5f;

    private int _currentElementIndex = 0;
    private int _totalElements = 0;
    private bool _isInitialized = false;
    private int _displayIndex = 0;

    public event Action OnNextElement;
    public event Action OnPreviousElement;

    void Awake()
    {
        InitializeScrollNavigation();
    }

    private void InitializeScrollNavigation()
    {
        if (_scrollRect == null)
            _scrollRect = GetComponent<ScrollRect>();

        if (_contentPanel == null)
            _contentPanel = _scrollRect.content;

        _totalElements = _contentPanel.childCount;

        if (_totalElements > 0)
        {
            UpdateCurrentElementIndex();
            _isInitialized = true;
        }
    }

    public void ScrollToNextElement()
    {
        var nextIndex = (_currentElementIndex + 1) % _totalElements;
        ScrollToElement(nextIndex);

        OnNextElement?.Invoke();
    }

    public void ScrollToPreviousElement()
    {
        var prevIndex = (_currentElementIndex - 1 + _totalElements) % _totalElements;
        ScrollToElement(prevIndex);

        OnPreviousElement?.Invoke();
    }

    public void ScrollToElement(int elementIndex)
    {
        if (elementIndex < 0 || elementIndex >= _totalElements || !_isInitialized)
            return;

        _displayIndex = elementIndex;

        if (_currentElementIndex == 0 || _currentElementIndex == _totalElements - 1
            && elementIndex == 0 || elementIndex == _totalElements - 1)
        {
            _displayIndex = RepositionForCarousel(_currentElementIndex, elementIndex);

        }

        var targetPosition = -_displayIndex * _elementWidth - _elementWidth / Screen.width - (_elementWidth - Screen.width) / 2f;
        StartCoroutine(SmoothScrollToPosition(targetPosition, _displayIndex));
    }

    private int RepositionForCarousel(int fromIndex, int toIndex)
    {
        if (_totalElements <= 1)
            return toIndex;

        if ((fromIndex == _totalElements - 1) && (toIndex == 0))
        {
            var targetPosition = toIndex * _elementWidth  - (_elementWidth / Screen.width) - (_elementWidth - Screen.width) / 2f;
            var anchoredPos = _contentPanel.anchoredPosition;
            _contentPanel.anchoredPosition = new Vector2(targetPosition, anchoredPos.y);
            return 1;
        }
        else if ((fromIndex == 0) && (toIndex == _totalElements - 1))
        {
            var targetPosition = -toIndex * _elementWidth - (_elementWidth / Screen.width) - (_elementWidth - Screen.width) / 2f;
            var anchoredPos = _contentPanel.anchoredPosition;
            _contentPanel.anchoredPosition = new Vector2(targetPosition, anchoredPos.y);
            return _totalElements - 2;
        }
        return toIndex;
    }

    private IEnumerator SmoothScrollToPosition(float targetPosition, int targetElementIndex)
    {
        var startPosition = _contentPanel.anchoredPosition;
        var endPosition = new Vector2(targetPosition, startPosition.y);

        var elapsedTime = 0f;

        while (elapsedTime < scrollDuration)
        {
            var t = elapsedTime / scrollDuration;
            t = 1f - Mathf.Pow(1f - t, 3);

            var newPosition = Vector2.Lerp(startPosition, endPosition, t);
            _contentPanel.anchoredPosition = newPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _contentPanel.anchoredPosition = endPosition;

        _currentElementIndex = targetElementIndex;
    }

    private void UpdateCurrentElementIndex()
    {
        var currentPosition = -_contentPanel.anchoredPosition.x;

        var normalizedPosition = currentPosition + _elementWidth / 6f;
        var calculatedIndex = Mathf.RoundToInt(normalizedPosition / _elementWidth);

        _currentElementIndex = Mathf.Clamp(calculatedIndex, 0, _totalElements - 1);
    }

    public int GetCurrentDisplayIndex()
    {
        return _displayIndex % GetTotalDisplayElements();
    }

    public int GetTotalDisplayElements()
    {
        return _totalElements - 1;
    }

    void Update()
    {
        if (!_isInitialized) return;

        var currentPosition = -_contentPanel.anchoredPosition.x;
        var normalizedPosition = currentPosition + _elementWidth / 6f;
        var calculatedIndex = Mathf.RoundToInt(normalizedPosition / _elementWidth);

        if (calculatedIndex != _currentElementIndex &&
            calculatedIndex >= 0 &&
            calculatedIndex < _totalElements)
        {
            _currentElementIndex = calculatedIndex;
        }
    }
}