using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseInput 
{
    public float GetAxisX(out float value);
    public float GetAxisY(out float value);
}
