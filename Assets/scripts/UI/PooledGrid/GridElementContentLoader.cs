using ImageLoaderSystem;
using UnityEngine;

public class GridElementContentLoader : MonoBehaviour
{
    [SerializeField]
    private PooledGridLayoutGroup _group;
    
    void Awake()
    {
        _group.OnElementAdd += OnElementAdd;
    }

    void OnDestroy()
    {
        _group.OnElementAdd -= OnElementAdd;
    }

    private async void OnElementAdd(int index)
    {
        await ImageLoaderAPI.Instance.LoadImageDataAsync(index);
    }
}
