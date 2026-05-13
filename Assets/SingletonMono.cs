using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    //记录单例对象是否存在。用于防止在OnDestroy方法中访问单例对象报错
    public static bool IsExisted { get; private set; } = false;

    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();//如果被隐藏的话，也会返回null
                if (instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name); // 创建游戏对象
                    instance = go.AddComponent<T>(); // 挂载脚本
                }
            }
            if (Application.isPlaying) // 关键检查：仅在运行时调用
            {
                DontDestroyOnLoad(instance);
            }
            IsExisted = true;
            return instance;
        }
    }


    // 构造方法私有化，防止外部 new 对象
    protected SingletonMono() { }

    private void OnDestroy()
    {
        IsExisted = false;
    }
}
