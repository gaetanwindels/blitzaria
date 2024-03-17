using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    [SerializeField] public int playerNumber;

    private TextMeshProUGUI numberText;


    // Start is called before the first frame update
    void Start()
    {
        numberText = GetComponentInChildren<TextMeshProUGUI>();
        numberText.text = (playerNumber + 1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnScreenPositionChanged(Vector2 screenPosition)
    {
        RectTransform canvasRectTransform = transform.root.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        Rect rootCanvasRect = canvasRectTransform.rect;
        var image = GetComponent<RawImage>().texture;
        Vector2 viewportPos = Camera.main.ScreenToViewportPoint(screenPosition - new Vector2(image.width / 2, image.height / 2));
        viewportPos.x = (viewportPos.x * rootCanvasRect.width) - canvasRectTransform.pivot.x * rootCanvasRect.width;
        viewportPos.y = (viewportPos.y * rootCanvasRect.height) - canvasRectTransform.pivot.y * rootCanvasRect.height;
        
        (transform as RectTransform).anchoredPosition = viewportPos;
    }
}
