using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BehaviorDesigner.Runtime;
using UnityEngine.EventSystems;

/// <summary>
/// 测试类，用于检测UI交互和其他功能测试
/// </summary>
public class test : MonoBehaviour {

    /// <summary>
    /// 测试用纹理，可以在Inspector中设置
    /// </summary>
    public Texture2D texture;

    /// <summary>
    /// Unity生命周期方法，在对象启用时调用一次
    /// </summary>
	void Start () {
        // 初始化代码可以放在这里
    }

    /// <summary>
    /// Unity生命周期方法，每帧调用一次
    /// 检测鼠标/触摸是否在UI元素上
    /// </summary>
    private void Update() {
        // 检查当前是否有UI元素拦截了鼠标/触摸事件
        if (EventSystem.current.IsPointerOverGameObject()) {
            Debug.Log("当前触摸在UI上");
        } else {
            Debug.Log("当前没有触摸在UI上");
        }
    }
}
 