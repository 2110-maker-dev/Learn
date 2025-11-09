using UnityEngine;
using UnityEngine.UI;

public class UIMainController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Button startBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private GameObject mainUI; // 就是我们创建的MainUI面板

    [Header("游戏核心引用")]
    [SerializeField] private MazeGenerator mazeGenerator; // 拖拽场景中的迷宫生成器对象
    [SerializeField] private FirstPersonController playerController; // 拖拽场景中的玩家对象

    void Start()
    {
        // 绑定按钮点击事件
        startBtn.onClick.AddListener(OnStartGame);
        quitBtn.onClick.AddListener(OnQuitGame);

        // 初始状态：显示UI，锁定玩家输入（避免未开始就移动）
        mainUI.SetActive(true);
        if (playerController != null)
        {
            playerController.enabled = false; // 禁用玩家控制
        }
    }

    // 开始游戏逻辑
    private void OnStartGame()
    {
        // 隐藏UI面板
        mainUI.SetActive(false);

        // 生成新迷宫（确保每次开始都是新迷宫）
        if (mazeGenerator != null)
        {
            mazeGenerator.GenerateMaze();
        }

        // 启用玩家控制（允许移动、视角操作）
        if (playerController != null)
        {
            playerController.enabled = true;
            // 重新锁定鼠标（符合第一人称游戏操作）
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 退出游戏逻辑
    private void OnQuitGame()
    {
        // 编辑器中退出播放模式，打包后退出程序
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // 通关后重新显示UI（对接玩家胜利逻辑）
    public void ShowMainUIAfterWin()
    {
        mainUI.SetActive(true);
        if (playerController != null)
        {
            playerController.enabled = false; // 禁用玩家控制
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}