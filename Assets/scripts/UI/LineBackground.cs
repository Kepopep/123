using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LineBackground : MonoBehaviour
{
    [SerializeField]
    private Sprite _lineSprite;
    
    [SerializeField]
    private Color _lienColor = Color.white;
    
    
    public float LineDistance = 50f;
    
    public float LineWidth = 2f;
    
    public Vector2 Offset = Vector2.zero;
    
    // Private variables starting with _
    private RectTransform _rectTransform;
    private GameObject[] _lineObjects;

    public void UpdateLines()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_rectTransform != null)
        {
            var totalWidth = _rectTransform.rect.width;
            var requiredLineCount = Mathf.Max(0, Mathf.CeilToInt((totalWidth + LineDistance) / LineDistance));
            
            Debug.Log($"rea {requiredLineCount}");
            if (_lineObjects == null || _lineObjects.Length != requiredLineCount)
            {
                if (_lineObjects != null)
                {
                    for (int i = 0; i < _lineObjects.Length; i++)
                    {
                        if (_lineObjects[i] != null)
                        {
#if UNITY_EDITOR
                            if (!Application.isPlaying)
                                DestroyImmediate(_lineObjects[i]);
                            else
                                Destroy(_lineObjects[i]);
#endif
                        }
                    }
                }
                
                _lineObjects = new GameObject[requiredLineCount];
            }
            
            for (int i = 0; i < requiredLineCount; i++)
            {
                var lineObj = _lineObjects[i];
                
                if (lineObj == null)
                {
                    lineObj = new GameObject($"VerticalLine_{i}");
                    lineObj.transform.SetParent(transform, false);
                    
                    RectTransform lineRect = lineObj.AddComponent<RectTransform>();
                    lineRect.anchorMin = Vector2.zero;
                    lineRect.anchorMax = Vector2.zero;
                    lineRect.pivot = new Vector2(0.0f, 0.0f);
                    
                    _lineObjects[i] = lineObj;
                }
                
                RectTransform lineRectTrans = _lineObjects[i].GetComponent<RectTransform>();
                lineRectTrans.anchoredPosition = new Vector2((i * LineDistance) + (LineWidth / 2), 0) + Offset;
                lineRectTrans.sizeDelta = new Vector2(LineWidth, _rectTransform.rect.height);
                
                UnityEngine.UI.Image lineImage = _lineObjects[i].GetComponent<UnityEngine.UI.Image>();
                if (lineImage == null)
                {
                    lineImage = _lineObjects[i].AddComponent<UnityEngine.UI.Image>();
                    
                    var texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    lineImage.sprite = _lineSprite;
                }
                
                lineImage.color = _lienColor;
            }
        }
    }
    
    private void OnDestroy()
    {
        if (_lineObjects != null)
        {
            for (int i = 0; i < _lineObjects.Length; i++)
            {
                if (_lineObjects[i] != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(_lineObjects[i]);
                    else
                        Destroy(_lineObjects[i]);
#endif
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LineBackground))]
public class LineBackgroundEditor : Editor
{
    private void OnSceneGUI()
    {
        var lineBackground = (LineBackground)target;
        
        if (lineBackground.LineDistance > 0)
        {
            Transform transform = lineBackground.transform;
            RectTransform rectTransform = lineBackground.GetComponent<RectTransform>();
            
            if (rectTransform != null)
            {
                var lineCount = Mathf.Max(0, Mathf.CeilToInt((rectTransform.rect.width + lineBackground.LineDistance) / lineBackground.LineDistance));
                
                for (var i = 0; i < lineCount; i++)
                {
                    var localX = (-rectTransform.rect.width / 2) + (i * lineBackground.LineDistance) + (lineBackground.LineWidth / 2);
                    
                    var worldStart = transform.TransformPoint(new Vector3(localX, rectTransform.rect.height / 2, 0) + new Vector3(lineBackground.Offset.x, lineBackground.Offset.y, 0));
                    var worldEnd = transform.TransformPoint(new Vector3(localX, -rectTransform.rect.height / 2, 0) + new Vector3(lineBackground.Offset.x, lineBackground.Offset.y, 0));
                    Handles.color = Color.green;
                    Handles.DrawLine(worldStart, worldEnd);

                    worldStart = transform.TransformPoint(new Vector3(localX+lineBackground.LineWidth, rectTransform.rect.height / 2, 0) + new Vector3(lineBackground.Offset.x, lineBackground.Offset.y, 0));
                    worldEnd = transform.TransformPoint(new Vector3(localX+lineBackground.LineWidth, -rectTransform.rect.height / 2, 0) + new Vector3(lineBackground.Offset.x, lineBackground.Offset.y, 0));
                    Handles.color = Color.red;
                    Handles.DrawLine(worldStart, worldEnd);
                }
                
                Handles.color = Color.white;
            }
        }
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LineBackground lineBackground = (LineBackground)target;
        
        if (GUILayout.Button("Update Lines"))
        {
            if (!Application.isPlaying)
            {
                lineBackground.UpdateLines();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
#endif