using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridElement : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    public RectTransform Rect => _rect;
    private RectTransform _rect;

    private int _dataIndex = -1;
    private bool _isEnabled = false;

    private WaitForSeconds _timeoutRoutine = new WaitForSeconds(0.1f);
    private Coroutine _loadeRoutine;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void SetData(int index)
    {
        _dataIndex = index;

        if (_dataIndex != -1 && index != -1)
        {
            UpdateImageData();
        }
    }

    public void SetActive(bool active)
    {
        if (!active)
        {
            _image.sprite = null;
        }

        gameObject.SetActive(active);
        _isEnabled = active;
    }

    void OnEnable()
    {
        UpdateImageData();
    }

    private void UpdateImageData()
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