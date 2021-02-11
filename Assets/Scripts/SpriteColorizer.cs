using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorizer : MonoBehaviour
{

    [SerializeField] Color pixelColour;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        var mySprite = spriteRenderer.sprite;

        if (mySprite == null || mySprite.texture == null) return;


        var texture = mySprite.texture;
        var newTexture = new Texture2D(texture.width, texture.height);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++) //Goes through each pixel
            {
                var pixel = texture.GetPixel(x, y);
                if (pixel.r > 170)
                {
                    newTexture.SetPixel(x, y, pixelColour);
                } else
                {
                    newTexture.SetPixel(x, y, pixel);
                }

            }
        }

        newTexture.Apply();
        spriteRenderer.material.mainTexture = newTexture;


        Debug.Log(spriteRenderer);
    }
}
