using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ImageLoaderSystem
{
    public class ImageLoaderAPI : MonoBehaviour
    {
        [SerializeField]
        private int _maxImageIndex;

        public int MaxImageIdex => _maxImageIndex;

        private static ImageLoaderAPI _instance;

        public static ImageLoaderAPI Instance => _instance;

        public int LoadingCount => _loadingCout;

        private int _maxLoadedIndex;
        private int _loadingCout;

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
            if (ImageStorage.Instance.Contains(imageIndex))
            {
                return ImageStorage.Instance.Get(imageIndex);
            }

            var sprite = await SaveImage(imageIndex);
            return sprite;
        }

        private async System.Threading.Tasks.Task<Sprite> SaveImage(int imageIndex)
        {
            _loadingCout++;
            var bytes = await ImageDownloadManager.DownloadImageBytesAsync(imageIndex);
            var sprite = ImageDownloadManager.ConvertBytesToSprite(bytes);

            ImageStorage.Instance.Add(imageIndex, sprite);

            _loadingCout--;
            return sprite;
        }

        public bool IsIdAvailabe(int imageIndex)
        {
            return imageIndex > 0 && imageIndex <= _maxImageIndex;
        }
    }
}