using System;
using System.Collections.Generic;
using UnityEngine;

public class ImageStorage : MonoBehaviour
{
    private static ImageStorage _instance;

    public static ImageStorage Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("ImageLoaderAPI");
                _instance = obj.AddComponent<ImageStorage>();
            }
            return _instance;
        }
    }


    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Dictionary<int, Sprite> _storage = new Dictionary<int, Sprite>();

    public void Add(int index, Sprite image)
    {
        var saveIndex = index + 1;
        if(!_storage.ContainsKey(saveIndex))
        {
            _storage.Add(saveIndex, image);
        }
    }

    public Sprite Get(int index)
    {
        var saveIndex = index + 1;
        if(!_storage.ContainsKey(saveIndex))
        {
            throw new ArgumentOutOfRangeException(nameof(saveIndex), "No image at index");
        }
        return _storage[saveIndex];
    }

    public bool Contains(int index)
    {
        return _storage.ContainsKey(index + 1);
    }
}
