using System;
using ImageLoaderSystem;
using UnityEngine;

public class GridContentProvider : MonoBehaviour
{
    [SerializeField]
    private ElementSelectionMode _mode;

    [SerializeField]
    private PooledGridLayoutGroup _grid;

    [SerializeField]
    private bool _useLoading;

    public int LastElementNumber => _mode == ElementSelectionMode.All ?
        ImageLoaderAPI.Instance.MaxImageIdex :
        ImageLoaderAPI.Instance.MaxImageIdex / 2;

    private void Awake()
    {
        _grid.OnElementVisualize += OnElementAddAsync;
    }

    private void OnDestroy()
    {
        _grid.OnElementVisualize -= OnElementAddAsync;
    }

    private async void OnElementAddAsync(GridElement element, int displayIndex)
    {
        var imageIndex = displayIndex + 1;
        switch (_mode)
        {
            case ElementSelectionMode.Odd:
                imageIndex = imageIndex * 2 - 1;
                break;
            case ElementSelectionMode.Even:
                imageIndex = imageIndex * 2;
                break;
        }

        if (ImageLoaderAPI.Instance.IsIdAvailabe(imageIndex))
        {
            if(!_useLoading)
            {
                return;
            }
            
            element.SetData(imageIndex);
            await ImageLoaderAPI.Instance.LoadImageDataAsync(imageIndex);
        }
        else
        {
            element.SetActive(false);
        }
    }



    // simple button onClick action...
    public void SetAllMode() => ChangeMode(ElementSelectionMode.All);
    public void SetEvenMode() => ChangeMode(ElementSelectionMode.Even);
    public void SetOddMode() => ChangeMode(ElementSelectionMode.Odd);

    private void ChangeMode(ElementSelectionMode newMode)
    {
        _mode = newMode;
        _grid.RecalculateVisibleIndex();
    }
}


[Serializable]
public enum ElementSelectionMode
{
    All,    // All indexes from 0 to 66
    Odd,    // Only add indexes (odd numbers)
    Even    // Only even indexes
}