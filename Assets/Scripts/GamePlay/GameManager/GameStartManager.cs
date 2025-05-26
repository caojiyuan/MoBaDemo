using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用来管理游戏刚刚初始化完毕之后要做的必要操作
/// 负责初始化玩家角色、UI界面以及联网游戏设置等
/// </summary>
public class GameStartManager : MonoBehaviour{

    // 玩家控制的角色
    private HeroMono characterMono;
    
    // 角色预制体
    public GameObject characterPrafab;
    
    // UI视图引用
    public SkillView skillView;                 // 技能视图
    public StoreView storeView;                 // 商店视图
    public MouseCursorChanged mouseCursorChanged; // 鼠标光标管理
    public HPAndMPDetailView hPAndMPDetailView; // 血量和魔法值详情视图
    public AvatarView avatarView;               // 头像视图
    public ShowPlayerMoney showPlayerMoneyView; // 玩家金钱显示视图
    public ItemListView itemListView;           // 物品列表视图
    public BattleStatusListView battleStatusListView; // 战斗状态列表视图
    
    // 游戏管理器引用
    public GamePlayManager gamePlayManager;
    
    // 玩家角色的投影显示器（用于指示玩家控制的角色）
    public GameObject operateCharacterProjector;

    // 单例模式实现
    public static GameStartManager instance;
    public static GameStartManager Instance {
        get {
            if (instance == null)
                instance = GameObject.FindObjectOfType<GameStartManager>();
            return instance;
        }
    }

    /// <summary>
    /// Unity生命周期方法，在游戏开始时调用
    /// </summary>
    private void Start() {
        StartGame();
    }

    /// <summary>
    /// 开始游戏的方法，初始化角色、UI和游戏玩法
    /// </summary>
    private void StartGame() {
        // 创建玩家角色并初始化
        characterMono = CharacterMonoFactory.AcquireObject(
            TestDatabase.Instance.characterModels[0], // 从数据库获取角色模型
            characterPrafab,                         // 使用设定的预制体
            new Vector3(16,0f,15)                    // 初始位置
        ) as HeroMono;
        
        characterMono.Init(); 
        
        #region Test
        // 测试代码：设置角色为当前玩家控制，并添加投影标识
        characterMono.IsOperateByNowPlayer = true;
        GameObject test = GameObject.Instantiate(operateCharacterProjector, characterMono.transform, false);
        test.transform.localPosition = new Vector3(0,2,0);
        #endregion

        // 初始化UI和游戏玩法
        InitUI();
        InitGamePlay();

        // 确保网络管理器存在
        if (NetWorkManager.Instance == null) {
            new GameObject().AddComponent<NetWorkManager>();
        }
            
        // 判断当前是联机模式还是单人模式
        if (NetWorkManager.Instance.IsConnect()) {
            // 联机模式：设置NPC创建时的网络同步
            gamePlayManager.OnCreateNPCGameObject += NetWorkManager.Instance.AddNetworkNpc;

            Debug.Log("根据当前玩家的阵营:" + NetWorkManager.Instance.NowPlayerFaction + "来创建玩家");
            
            // 根据玩家阵营设置角色属性和初始位置
            if (NetWorkManager.Instance.NowPlayerFaction == "Red") {
                // 红方阵营设置
                characterMono.characterModel.unitFaction = UnitFaction.Red;
                NetWorkManager.Instance.StartGame(new Vector3(17, 0, 18), characterMono);
                Camera.main.transform.position = new Vector3(15, Camera.main.transform.position.y, 6);
            } else {
                // 蓝方阵营设置
                characterMono.characterModel.unitFaction = UnitFaction.Blue;
                NetWorkManager.Instance.StartGame(new Vector3(181, 0, 179), characterMono);
                Camera.main.transform.position = new Vector3(181, Camera.main.transform.position.y, 179);
            }

            Debug.Log("当前玩家是否是房主:" + NetWorkManager.Instance.IsHomeowner);
            
            // 房主专属逻辑：只有房主初始化游戏管理器（控制出兵等）
            if (NetWorkManager.Instance.IsHomeowner) {
                gamePlayManager.Init();
            }
        } else {
            // 单人模式设置
            gamePlayManager.Init();
            Camera.main.transform.position = new Vector3(15, Camera.main.transform.position.y, 6);
            
            // 启用角色操作状态机
            characterMono.GetComponent<CharacterOperationFSM>().enabled = true;
        }
    }

    /// <summary>
    /// 初始化所有UI组件
    /// </summary>
    public void InitUI() {
        skillView.Init(characterMono);             // 初始化技能视图
        storeView.Init(characterMono);             // 初始化商店视图
        InstallHpAndMPDetailView();                // 初始化血量和魔法值视图
        InstallAvaterView();                       // 初始化头像视图
        showPlayerMoneyView.Init(characterMono);   // 初始化金钱显示
        itemListView.Init(characterMono);          // 初始化物品列表
        battleStatusListView.Init(characterMono);  // 初始化状态效果列表
    }

    /// <summary>
    /// 初始化游戏玩法相关组件
    /// </summary>
    public void InitGamePlay() {
        // 初始化鼠标光标管理器
        mouseCursorChanged.Init();
    }

    /// <summary>
    /// 安装血量和魔法值详情视图
    /// </summary>
    private void InstallHpAndMPDetailView() {
        hPAndMPDetailView.Init(characterMono);
    }

    /// <summary>
    /// 安装头像视图
    /// </summary>
    private void InstallAvaterView() {
        avatarView.Init(characterMono);
    }
}

