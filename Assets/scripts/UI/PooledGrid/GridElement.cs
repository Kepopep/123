using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class GridElement : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    [SerializeField]
    private GameObject _vipBadge;

    [SerializeField]
    private Button _button;

    public RectTransform Rect => _rect;
    private RectTransform _rect;

    public int DataIndex => _dataIndex;
    private int _dataIndex = -1;
    private bool _isEnabled = false;

    private bool _isVip = false;

    private WaitForSeconds _timeoutRoutine = new WaitForSeconds(0.1f);
    private Coroutine _loadeRoutine;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _button.onClick.AddListener(ShowContent);
    }

    void OnDestroy()
    {
        _button.onClick.RemoveListener(ShowContent);
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
        if(!_isEnabled)
        {
            return;
        }

        _loadeRoutine = null;
        _loadeRoutine = StartCoroutine(WaitRoutine());
        _isVip = _dataIndex % 4 == 0;
        _vipBadge.SetActive(_isVip);
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

    private void ShowContent()
    {
        if (_isVip)
        {
            PopUpManager.Instance.ShowVipOffer();
        }
        else
        {
            PopUpManager.Instance.ShowImage(_image.sprite);
        }
    }
}