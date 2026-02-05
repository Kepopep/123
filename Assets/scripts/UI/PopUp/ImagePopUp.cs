using UnityEngine;
using UnityEngine.UI;

public class ImagePopUp : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    public void Show(Sprite sprite)
    {
        gameObject.SetActive(true);
        _image.sprite = sprite;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}