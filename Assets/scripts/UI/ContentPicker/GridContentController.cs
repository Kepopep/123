using System;
using ImageLoaderSystem;
using UnityEngine;

public class GridContentController : MonoBehaviour
{
    [SerializeField]
    private ElementSelectionMode _mode;
    
    [SerializeField]
    private PooledGridLayoutGroup _grid;

    void Awake()
    {
        _grid.OnElementAdd += OnElementAddAsync;
    }

    private async void OnElementAddAsync(GridElement element, int displayIndex)
    {
        var imageIndex = displayIndex + 1;
        switch (_mode)
        {
            case ElementSelectionMode.Odd:
                imageIndex = displayIndex * 2 - 1;
                break;
            case ElementSelectionMode.Even:
                imageIndex = displayIndex * 2;
                break;
        }
        element.SetData(imageIndex);
        
        await ImageLoaderAPI.Instance.LoadImageDataAsync(imageIndex);
    }
}


[Serializable]
public enum ElementSelectionMode
{
    All,    // All indexes from 0 to 66
    Odd,    // Only add indexes (odd numbers)
    Even    // Only even indexes
}