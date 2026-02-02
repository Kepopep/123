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
        if(!_storage.ContainsKey(index))
        {
            _storage.Add(index, image);
        }
    }

    public Sprite Get(int index)
    {
        if(!_storage.ContainsKey(index))
        {
            throw new ArgumentOutOfRangeException(nameof(index), "No image at index");
        }
        return _storage[index];
    }

    public bool Contains(int index)
    {
        return _storage.ContainsKey(index);
    }
}
