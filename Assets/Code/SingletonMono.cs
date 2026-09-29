using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)//这里需要注意一个bug，就是该单例被销毁了的话，最好不要再被访问了，同生命周期的对象就不要取消订阅了。
            {
                instance = FindObjectOfType<T>();//如果被隐藏的话，也会返回null
                if (instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name); // 创建游戏对象
                    instance = go.AddComponent<T>(); // 挂载脚本
                }
            }
            return instance;
        }
    }
    // 构造方法私有化，防止外部 new 对象
    protected SingletonMono() { }


}
