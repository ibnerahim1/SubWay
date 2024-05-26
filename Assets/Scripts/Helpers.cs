using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Helpers
{
    private static Camera m_MainCamera;

    public static Camera MainCamera
    {
        get
        {
            if (m_MainCamera == null) m_MainCamera = Camera.main;
            return m_MainCamera;
        }
    }

    private static readonly Dictionary<float, WaitForSeconds> m_WaitDictionary = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds GetWait(float i_Delay)
    {
        if (m_WaitDictionary.TryGetValue(i_Delay, out var wait)) return wait;
        m_WaitDictionary[i_Delay] = new WaitForSeconds(i_Delay);
        return m_WaitDictionary[i_Delay];
    }

    private static PointerEventData m_PointerCurrentposition;
    private static List<RaycastResult> m_RaycastResults;

    public static bool IsOnUI()
    {
        m_PointerCurrentposition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        m_RaycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(m_PointerCurrentposition, m_RaycastResults);
        return m_RaycastResults.Count > 0;
    }

    public static Vector2 GetWorldPositionOnUI(RectTransform i_RectTransform)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(i_RectTransform, i_RectTransform.position, MainCamera, out var result);
        return result;
    }

}