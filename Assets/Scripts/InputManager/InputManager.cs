using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InputManager
{
    void init(int playernumber);

    bool GetButtonUp(string actionName);

    bool GetButtonDown(string actionName);

    bool GetButton(string actionName);

    float GetAxis(string actionName);

}
