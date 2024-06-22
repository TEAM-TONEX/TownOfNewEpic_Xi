using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace TONEX.OptionUI
{
    public class UIBase : MonoBehaviour
    {
        private static UIBase instance;

        public GameObject canvas;
        public GameObject mainUI;
        public GameObject openButton;
        public GameObject closeButton;
        public GameObject sidebar;

        public Dictionary<Button, bool> tabButtons = new();
        public float size;
        public List<GameObject> Headers = new();

        public virtual void Init() { }
        public virtual void AfterCreate() { }

        public void KeepGameObjects()
        {
            DontDestroyOnLoad(GetInstance().canvas);
            DontDestroyOnLoad(GetInstance().mainUI);
            DontDestroyOnLoad(GetInstance().openButton);
            DontDestroyOnLoad(GetInstance().closeButton);
            DontDestroyOnLoad(GetInstance().sidebar);
        }
        public void Awake()

        {
            Initialize();
            DontDestroyOnLoad(gameObject);
        }
        public void Initialize()
        {
            // 如果已经存在一个实例，则销毁它
            if (instance != null && instance != this)
            {
                Destroy(instance);
            }

            // 保存当前实例
            if (instance == null)
            {
                instance = this;
            }
                // 确保不在场景切换时销毁
                
            
        }

        public static UIBase GetInstance()
        {
            return instance;
        }
    }
}

