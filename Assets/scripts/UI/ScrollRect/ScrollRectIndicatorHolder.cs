using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectIndicatorHolder : MonoBehaviour
{
    [Header("References")]
    public ScrollRectNavigator scrollRectNavigator;
    
    [Header("Indicator Settings")]
    public GameObject indicatorPrefab; 
    public Transform indicatorsParent; 
    public Sprite activeSprite;
    public Sprite inactiveSprite;
    
    private List<Image> indicators = new List<Image>(); // List to store indicator image components
    private int currentSelectedIndex = 0;
    private int totalElements = 0;

    void Start()
    {       
        totalElements = scrollRectNavigator.GetTotalDisplayElements();

        InitializeIndicators();
        SetupEventListeners();
    }

    void OnDestroy()
    {
        RemoveEventListeners();
    }

    private void InitializeIndicators()
    {
        totalElements = scrollRectNavigator.GetTotalDisplayElements();
        currentSelectedIndex = scrollRectNavigator.GetCurrentDisplayIndex();

        CreateIndicators();
        UpdateIndicators();
    }

    private void CreateIndicators()
    {
        foreach (Transform child in indicatorsParent)
        {
            DestroyImmediate(child.gameObject);
        }
        indicators.Clear();

        for (int i = 0; i < totalElements; i++)
        {
            GameObject indicatorObj = Instantiate(indicatorPrefab, indicatorsParent);
            
            var img = indicatorObj.GetComponent<Image>();
            img.sprite = i == currentSelectedIndex ? activeSprite : inactiveSprite;
            indicators.Add(img);
        }
    }

    private void SetupEventListeners()
    {
        if (scrollRectNavigator != null)
        {
            scrollRectNavigator.OnNextElement += OnElementChanged;
            scrollRectNavigator.OnPreviousElement += OnElementChanged;
        }
    }

    private void RemoveEventListeners()
    {
        if (scrollRectNavigator != null)
        {
            scrollRectNavigator.OnNextElement -= OnElementChanged;
            scrollRectNavigator.OnPreviousElement -= OnElementChanged;
        }
    }

    private void OnElementChanged()
    {
        currentSelectedIndex = scrollRectNavigator.GetCurrentDisplayIndex();
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        for (int i = 0; i < indicators.Count; i++)
        {
            if (indicators[i] != null)
            {
                indicators[i].sprite = i == currentSelectedIndex ? activeSprite : inactiveSprite;
            }
        }
    }
}
