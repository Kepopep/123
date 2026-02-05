using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ScrollRectNavigator : MonoBehaviour
{
    [Header("Scroll Settings")]
    public ScrollRect scrollRect;
    public RectTransform contentPanel;
    
    [Header("Element Settings")]
    public float elementWidth = 2238f; // Width of each element
    
    [Header("Animation Settings")]
    public float scrollDuration = 0.5f; // Duration of the scroll animation
    
    private int currentElementIndex = 0;
    private int totalElements = 0;
    private bool isInitialized = false;
    private int _displayIndex = 0;

    public event Action OnNextElement;
    public event Action OnPreviousElement;

    void Awake()
    {
        InitializeScrollNavigation();
    }
    
    private void InitializeScrollNavigation()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();
            
        if (contentPanel == null)
            contentPanel = scrollRect.content;
            
        // Count the number of child elements
        totalElements = contentPanel.childCount;
        
        if (totalElements > 0)
        {
            UpdateCurrentElementIndex();
            isInitialized = true;
        }
    }
    
    /// <summary>
    /// Scrolls to the next element in the scroll rect with carousel functionality
    /// </summary>
    public void ScrollToNextElement()
    {
        if (totalElements <= 1) return; // Need at least 2 elements for carousel effect
        
        int nextIndex = (currentElementIndex + 1) % totalElements;
        ScrollToElement(nextIndex);

        OnNextElement?.Invoke();
    }
    
    /// <summary>
    /// Scrolls to the previous element in the scroll rect with carousel functionality
    /// </summary>
    public void ScrollToPreviousElement()
    {
        if (totalElements <= 1) return; // Need at least 2 elements for carousel effect
        
        int prevIndex = (currentElementIndex - 1 + totalElements) % totalElements;
        ScrollToElement(prevIndex);
        
        OnPreviousElement?.Invoke();
    }
    
    /// <summary>
    /// Scrolls to a specific element by index with carousel functionality
    /// </summary>
    /// <param name="elementIndex">Index of the element to scroll to</param>
    public void ScrollToElement(int elementIndex)
    {
        if (elementIndex < 0 || elementIndex >= totalElements || !isInitialized)   
            return;
        
        _displayIndex = elementIndex;

        if (currentElementIndex==0||currentElementIndex==totalElements-1
            && elementIndex==0||elementIndex==totalElements-1)
        {
            _displayIndex = RepositionForCarousel(currentElementIndex, elementIndex);

        }
        
        var targetPosition = -_displayIndex * elementWidth - elementWidth/6f;
        StartCoroutine(SmoothScrollToPosition(targetPosition, _displayIndex));
    }
    
    /// <summary>
    /// Repositions elements for seamless carousel transition
    /// </summary>
    /// <param name="fromIndex">Starting index</param>
    /// <param name="toIndex">Target index</param>
    private int RepositionForCarousel(int fromIndex, int toIndex)
    {
        if (totalElements <= 1) 
            return toIndex;
        
        if ((fromIndex == totalElements - 1) && (toIndex == 0))
        {
            float targetPosition = toIndex * elementWidth - elementWidth/6f;
            Vector2 anchoredPos = contentPanel.anchoredPosition;
            contentPanel.anchoredPosition = new Vector2(targetPosition, anchoredPos.y);
            return 1;
        }
        else if ((fromIndex == 0) && (toIndex == totalElements - 1))
        {
            float targetPosition = -toIndex * elementWidth - elementWidth/6f;
            Vector2 anchoredPos = contentPanel.anchoredPosition;
            contentPanel.anchoredPosition = new Vector2(targetPosition, anchoredPos.y);
            return totalElements-2;
        }
        return toIndex;
    }
    
    /// <summary>
    /// Smoothly animates the scroll rect to the target position
    /// </summary>
    /// <param name="targetPosition">Target X position to scroll to</param>
    /// <param name="targetElementIndex">Target element index</param>
    private IEnumerator SmoothScrollToPosition(float targetPosition, int targetElementIndex)
    {
        Vector2 startPosition = contentPanel.anchoredPosition;
        Vector2 endPosition = new Vector2(targetPosition, startPosition.y);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < scrollDuration)
        {
            float t = elapsedTime / scrollDuration;
            t = 1f - Mathf.Pow(1f - t, 3); // Cubic ease-out
            
            Vector2 newPosition = Vector2.Lerp(startPosition, endPosition, t);
            contentPanel.anchoredPosition = newPosition;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we reach the exact target position
        contentPanel.anchoredPosition = endPosition;
        
        // Update the current element index
        currentElementIndex = targetElementIndex;
    }
    
    /// <summary>
    /// Updates the current element index based on the current scroll position
    /// </summary>
    private void UpdateCurrentElementIndex()
    {
        if (totalElements <= 0) return;
        
        // Get the current horizontal position
        float currentPosition = -contentPanel.anchoredPosition.x;
        
        // Calculate which element is currently visible based on position
        float normalizedPosition = currentPosition + elementWidth/6f;
        int calculatedIndex = Mathf.RoundToInt(normalizedPosition / elementWidth);
        
        // Ensure the calculated index is within valid range
        currentElementIndex = Mathf.Clamp(calculatedIndex, 0, totalElements - 1);
    }
    
    /// <summary>
    /// Gets the current element index
    /// </summary>
    public int GetCurrentDisplayIndex()
    {
        return _displayIndex % GetTotalDisplayElements();
    }
    
    /// <summary>
    /// Gets the total number of elements
    /// </summary>
    public int GetTotalDisplayElements()
    {
        return totalElements - 1;
    }
    
    // Optional: Handle manual scrolling to update the current element index
    void Update()
    {
        if (!isInitialized) return;
        
        // Check if the user manually scrolled to a different position
        float currentPosition = -contentPanel.anchoredPosition.x;
        float normalizedPosition = currentPosition + elementWidth/6f;
        int calculatedIndex = Mathf.RoundToInt(normalizedPosition / elementWidth);
        
        if (calculatedIndex != currentElementIndex && 
            calculatedIndex >= 0 && 
            calculatedIndex < totalElements)
        {
            currentElementIndex = calculatedIndex;
        }
    }
}