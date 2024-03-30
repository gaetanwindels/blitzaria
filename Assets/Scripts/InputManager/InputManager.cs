using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InputManager
{
    void Init(int playernumber);

    bool GetButtonUp(string actionName);

    bool GetButtonDown(string actionName);

    bool GetButton(string actionName);

    float GetAxis(string actionName);

    void UnregisterInputEvents();

    void RegisterInputEvents();

    void ForceRegisterInputEvents();
    
    void UnForceRegisterInputEvents();

}
