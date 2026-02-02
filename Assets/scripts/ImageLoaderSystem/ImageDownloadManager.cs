using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ImageLoaderSystem
{
    public static class ImageDownloadManager
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(12, 12);
        
        public static async UniTask<byte[]> DownloadImageBytesAsync(int imageIndex)
        {
            if (imageIndex < 1 || imageIndex > 66)
            {
                throw new ArgumentOutOfRangeException(nameof(imageIndex), "Image index must be between 1 and 66");
            }

            string imageUrl = $"http://data.ikppbb.com/test-task-unity-data/pics/{imageIndex}.jpg";

            await _semaphore.WaitAsync();

            try
            {
                return  await _httpClient.GetByteArrayAsync(imageUrl);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static Sprite ConvertBytesToSprite(byte[] imageData)
        {
            Texture2D texture = new Texture2D(2, 2);
            
            texture.LoadImage(imageData);
            
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            
            return sprite;
        }
    }
}