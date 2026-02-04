using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace ImageLoaderSystem
{
    public class ImageLoaderAPI : MonoBehaviour
    {
        [SerializeField]
        private int _maxImageIndex;

        public int MaxImageIdex => _maxImageIndex;

        private static ImageLoaderAPI _instance;
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public static ImageLoaderAPI Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
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

            var bytes = await ImageDownloadManager.DownloadImageBytesAsync(imageIndex);
            var sprite = ImageDownloadManager.ConvertBytesToSprite(bytes);

            ImageStorage.Instance.Add(imageIndex, sprite);

            return sprite;
        }

        public bool IsIdAvailabe(int imageIndex)
        {
            return imageIndex > 0 && imageIndex < _maxImageIndex;
        }
    }
}