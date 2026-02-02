using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridElement : MonoBehaviour
{
    [SerializeField]
    private Image _image;
    
    private int _dataIndex = -1;
    private bool _isEnabled = false;

    private WaitForSeconds _timeoutRoutine = new WaitForSeconds(0.1f);
    private Coroutine _loadeRoutine;

    public void SetData(int index)
    {
        _dataIndex = index;
    }

    public void SetActive(bool active)
    {
        if(!active)
        {
            _image.sprite = null;
        }
        
        gameObject.SetActive(active);
        _isEnabled = active;
    }

    async void OnEnable()
    {
        _loadeRoutine = null;
        _loadeRoutine = StartCoroutine(WaitRoutine());
    }

    void OnDisable()
    {
        _loadeRoutine = null;
    }

    public bool IsActive()
    {
        return _isEnabled;
    }

    private IEnumerator WaitRoutine()
    {
        do
        {
            yield return _timeoutRoutine;
        } while (!ImageStorage.Instance.Contains(_dataIndex));

        _image.sprite = ImageStorage.Instance.Get(_dataIndex);
    }
}