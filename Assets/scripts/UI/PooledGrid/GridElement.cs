using UnityEngine;
using UnityEngine.UI;

public class GridElement : MonoBehaviour
{
    [SerializeField]
    private Text indexText; // Text component to show the index
    
    private int dataIndex = -1;
    private bool isEnabled = false;
    
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        isEnabled = active;
    }
    
    public bool IsActive()
    {
        return isEnabled;
    }
    
    public void SetData(int index)
    {
        dataIndex = index;
        
        if (indexText != null)
        {
            indexText.text = index.ToString();
        }
    }
    
    public int GetDataIndex()
    {
        return dataIndex;
    }
}