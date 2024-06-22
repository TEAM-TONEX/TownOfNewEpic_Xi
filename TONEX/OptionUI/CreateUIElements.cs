using UnityEngine;

namespace TONEX.OptionUI;

public class CreateUIElements : UIBase
{
    public static CreateUIElements Instance;

    public new void Awake()
    {
        Initialize();
        DontDestroyOnLoad(this);
    }

    public  new void Initialize()
    {
        if (Instance != null && Instance != this)
            Destroy(Instance);
        if (Instance == null)
        {
            Instance = this;
            
        }
        GetInstance().Initialize();

        Init();

    }
    public void Load()
    {
        if (CanvasManager.Instance != null)
        {
            CanvasManager.Instance.CreateCanvas();
        }
        else
        {
            Debug.LogError("CanvasManager instance is null!");
        }

        if (OpenButtonManager.Instance != null)
        {
            OpenButtonManager.Instance.CreateOpenButton();
        }
        else
        {
            Debug.LogError("OpenButtonManager instance is null!");
        }

        if (MainUIManager.Instance != null)
        {
            MainUIManager.Instance.CreateMainUI();
        }
        else
        {
            Debug.LogError("MainUIManager instance is null!");
        }

        Debug.Log("Finish Create Successfully!");
        AfterCreate();
        //KeepGameObjects();

    }
}

