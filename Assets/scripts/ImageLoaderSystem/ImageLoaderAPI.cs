using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Microsoft.Unity.VisualStudio.Editor;

namespace ImageLoaderSystem
{
    public class ImageLoaderAPI : MonoBehaviour
    {
        private static ImageLoaderAPI _instance;
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public static ImageLoaderAPI Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("ImageLoaderAPI");
                    _instance = obj.AddComponent<ImageLoaderAPI>();
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

        public async UniTask<Sprite> LoadImageDataAsync(int imageIndex)
        {
            if(ImageStorage.Instance.Contains(imageIndex))
            {
                return ImageStorage.Instance.Get(imageIndex);
            }

            Debug.Log("image try download");
            var bytes = await ImageDownloadManager.DownloadImageBytesAsync(imageIndex + 1);

            var sprite = ImageDownloadManager.ConvertBytesToSprite(bytes);
            Debug.Log("image added");


            ImageStorage.Instance.Add(imageIndex, sprite);

            return sprite;
        }
    }
}