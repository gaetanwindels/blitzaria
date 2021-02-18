using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class PlayerSelect : MonoBehaviour
{
    private RawImage image;
    private Texture textureImage;

   // private Rewired.Player rwPlayer;

    // Start is called before the first frame update
    void Start()
    {
        //rwPlayer = ReInput.players.GetPlayer(0);
        image = GetComponentInChildren<RawImage>();
        textureImage = image.texture;
        image.texture = null;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void ManageNewPlayer()
    {
        throw new NotImplementedException();
    }
}
