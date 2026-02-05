using UnityEngine;

public class PopUpManager : MonoBehaviour
{
    public static PopUpManager Instance => _instance;
    private static PopUpManager _instance;

    [SerializeField]
    private VipPopUp _vipWindow;

    [SerializeField]
    private ImagePopUp _imagePopUp;

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void ShowImage(Sprite sprite)
    {
        _imagePopUp.Show(sprite);
    }

    public void ShowVipOffer()
    {
        _vipWindow.Show();
    }
}