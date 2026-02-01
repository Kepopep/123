using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ScrollRectTimer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private float _autoScrollDelay = 5.0f;
    
    [SerializeField]
    private ScrollRectNavigator _navigator;
    
    private Coroutine _timerCoroutine;
    private bool _isPaused = false;

    private void OnEnable()
    {
        if (_navigator != null)
        {
            StartTimer();
        }
    }

    private void OnDisable()
    {
        StopTimer();
    }

    private void StartTimer()
    {
        StopTimer(); // Stop any existing timer
        _timerCoroutine = StartCoroutine(AutoScrollCoroutine());
    }

    private void StopTimer()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    public void PauseTimer()
    {
        _isPaused = true;
    }

    public void ResumeTimer()
    {
        _isPaused = false;
    }

    private IEnumerator AutoScrollCoroutine()
    {
        while (true)
        {
            // Wait for the delay, but check if paused each frame
            float waitedTime = 0f;
            while (waitedTime < _autoScrollDelay)
            {
                yield return null;
                if (!_isPaused)
                {
                    waitedTime += Time.unscaledDeltaTime;
                }
            }
            
            if (!_isPaused && _navigator != null)
            {
                _navigator.ScrollToNextElement();
            }
        }
    }

    // Implement interface methods to pause/resume when mouse enters/exits
    public void OnPointerEnter(PointerEventData eventData)
    {
        PauseTimer();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResumeTimer();
    }
}
