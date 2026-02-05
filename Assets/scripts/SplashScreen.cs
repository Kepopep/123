using ImageLoaderSystem;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    void Awake()
    {
        gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        if(ImageLoaderAPI.Instance.LoadingCount <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
