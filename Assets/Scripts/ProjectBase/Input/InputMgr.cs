using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public delegate float GetInputEvent(out float value);
/// <summary>
/// 1.Input类
/// 2.事件中心模块
/// 3.公共Mono模块的使用
/// </summary>
public class InputMgr : BaseManager<InputMgr>
{
    public Dictionary<InputType, GetInputEvent> input = new Dictionary<InputType, GetInputEvent>();
    /// <summary>
    /// 构造函数中 添加Updata监听
    /// </summary>
    public InputMgr()
    {
        
        
    }

    public float GetInput(InputType inputType)
    {
        return input[inputType](out float value);
    }

    public void RegisterInput(InputType inputType, InputAxle inputAxle, IBaseInput baseInput)
    {
        GetInputEvent value = null;
        if (!input.ContainsKey(inputType))
        {
            switch (inputAxle)
            {
                case InputAxle.x:
                    value += baseInput.GetAxisX;
                    break;
                case InputAxle.y:
                    value += baseInput.GetAxisY;
                    break;
            }
            input.Add(inputType, value);
        }
        else
        {
            switch (inputAxle)
            {
                case InputAxle.x:
                    input[inputType] += baseInput.GetAxisX;
                    break;
                case InputAxle.y:
                    input[inputType] += baseInput.GetAxisY;
                    break;
            }
        }

    }

	public void UnLoadInput(InputType inputType, InputAxle inputAxle, IBaseInput baseInput)
    {
        if (input.TryGetValue(inputType, out GetInputEvent value))
        {
            switch (inputAxle)
            {
                case InputAxle.x:
                    value -= baseInput.GetAxisX;
                    break;
                case InputAxle.y:
                    value -= baseInput.GetAxisY;
                    break;
            }
        }
    }
}

public enum InputType
{
    x,y
}

public enum InputAxle
{
    x,y, joystick
}