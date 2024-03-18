using UnityEngine;

public class CenterRectTransform : MonoBehaviour {

    public RectTransform canvas;
    void Start() {
        // Get the RectTransform component
        RectTransform rectTransform = GetComponent<RectTransform>();
        // Calculate the center position
        Vector2 centerPosition = new Vector2((canvas.sizeDelta.x - rectTransform.sizeDelta.x) / 2, (canvas.sizeDelta.y - rectTransform.sizeDelta.y) / 2);
        // Set the anchoredPosition of the RectTransform to the center position
        rectTransform.anchoredPosition = centerPosition;
    }
}
