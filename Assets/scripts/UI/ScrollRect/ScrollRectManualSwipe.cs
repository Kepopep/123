using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollRectManualSwipe : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] 
    private ScrollRectNavigator _navigator;

    [Header("Swipe Settings")]
    [SerializeField] 
    private float _minSwipeDistance = 50f;

    [SerializeField] 
    private float _maxSwipeTime = 1f;
    
    
    private Vector2 _startTouchPosition;
    private float _startTime;
    private bool _isTrackingSwipe = false;
    
    public System.Action _onLeftSwipe;
    public System.Action _onRightSwipe;

    void Start()
    {
        if (_onLeftSwipe == null)
            _onLeftSwipe = _navigator.ScrollToNextElement;
        if (_onRightSwipe == null)
            _onRightSwipe = _navigator.ScrollToPreviousElement;
    }

    void OnDestroy()
    {
        _onLeftSwipe = null;
        _onRightSwipe = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startTouchPosition = eventData.position;
        _startTime = Time.time;
        _isTrackingSwipe = true;
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isTrackingSwipe)
        {
            ProcessSwipe(eventData);
            _isTrackingSwipe = false;
        }
    }
    
    private void ProcessSwipe(PointerEventData eventData)
    {
        var endTouchPosition = eventData.position;
        var deltaTime = Time.time - _startTime;
        
        if (deltaTime <= _maxSwipeTime)
        {
            Vector2 swipeVector = endTouchPosition - _startTouchPosition;
            
            if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
            {
                if (swipeVector.x > _minSwipeDistance)
                {
                    TriggerRightSwipe();
                }
                else if (-swipeVector.x > _minSwipeDistance)
                {
                    TriggerLeftSwipe();
                }
            }
        }
    }
    
    private void TriggerLeftSwipe()
    {
        if (_onLeftSwipe != null)
        {
            _onLeftSwipe.Invoke();
        }
    }
    
    private void TriggerRightSwipe()
    {
        if (_onRightSwipe != null)
        {
            _onRightSwipe.Invoke();
        }
    }
}
