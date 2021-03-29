using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoardsManager : MonoBehaviour
{

    [SerializeField] private GameObject playerBoard;

    Vector2 res;

    // Start is called before the first frame update
    void Start()
    {
        res = new Vector2(Screen.width, Screen.height);

        for (var i = 0; i < 4; i++)
        {
            var go = Instantiate(playerBoard, transform);
            RectTransform rect = go.GetComponent<RectTransform>();

            if (rect != null)
            {
                var width = rect.rect.width;
                rect.anchoredPosition = new Vector2((-1.5f * width) + (i * width), 0);
            }
        }
    }

    void Resize()
    {
        var rects = GetComponentsInChildren<RectTransform>();

        var i = 0;
        foreach (RectTransform rect in rects)
        {
            var width = rect.rect.width;
            rect.anchoredPosition = new Vector2((-1.5f * width) + (i * width), 0);
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (res.x != Screen.width || res.y != Screen.height)
        {
            Debug.Log("res changed");
            res.x = Screen.width;
            res.y = Screen.height;
            //Resize();
        }
    }
}
