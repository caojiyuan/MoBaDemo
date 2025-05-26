using System;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using System.Collections.Generic;

/// <summary>
/// 用于管理游戏逻辑的Manager类
/// 这里指的游戏逻辑是，在搭建技能、人物、AI、物品、装备等基本系统之后，
/// 游戏规则的逻辑，即如果是MOBA游戏，则用于管理出兵，英雄复活，玩家经济，人头数量计算等等逻辑
/// 如果是大逃杀游戏，用来管理英雄复活，玩家出生，空投等等逻辑
/// 是指游戏的高层逻辑，而技能、装备等基础系统不算在内
/// 
/// 单例类，确保游戏中只有一个GamePlayManager实例
/// </summary>
public class GamePlayManager : MonoBehaviour{

    // 单例模式实现
    private static GamePlayManager instance;
    public static GamePlayManager Instance {
        get {
            if (instance == null) {
                // 如果实例不存在，则从场景中查找
                instance = FindObjectOfType<GamePlayManager>();
            }
            return instance;
        }
    }

    // 出兵地点坐标数组，分为红蓝两方
    public Vector3[] placesOfDispatchRed;  // 红方出兵点位置
    public Vector3[] placesOfDispatchBlue; // 蓝方出兵点位置
    
    // 塔集合，按照位置和队伍分类
    public List<GameObject> towersRedUp;     // 红方上路塔
    public List<GameObject> towersRedMiddle; // 红方中路塔
    public List<GameObject> towersRedDown;   // 红方下路塔
    public List<GameObject> towersBlueUp;    // 蓝方上路塔
    public List<GameObject> towersBlueMiddle;// 蓝方中路塔
    public List<GameObject> towersBlueDown;  // 蓝方下路塔

    // 小兵的对象池，用于优化内存和实例化性能
    private GameObjectPool poolObjectFactory;
    
    // 小兵的预设集合，即每一次出兵会出现的那一群单位
    public GameObject[] solidersPrefabs;
    
    // 出兵频率（以秒为单位）
    public float deltaTime;
    
    // 玩家队伍集合
    public UnitFaction[] playerTeams;

    // 游戏是否结束的标志
    public bool isGameOver;

    /// <summary>
    /// 产生NPC对象的委托和事件，用于解耦网络联机产生NPC的逻辑
    /// 在网络游戏中，需要在所有客户端同步创建NPC
    /// </summary>
    /// <param name="position">NPC生成位置</param>
    /// <param name="soliderObject">NPC对象</param>
    /// <param name="gameObjectPool">对象池引用</param>
    public delegate void CreateNPCGameObject(Vector3 position, GameObject soliderObject, GameObjectPool gameObjectPool);
    public event CreateNPCGameObject OnCreateNPCGameObject;

    /// <summary>
    /// 初始化游戏管理器
    /// </summary>
    public void Init() {
        // 初始化对象池，容量为50
        poolObjectFactory = new GameObjectPool(50);

        // 开始出兵协程
        StartCoroutine(DispatchSoliders());
    }

    /// <summary>
    /// 出兵协程，控制游戏中小兵的生成
    /// </summary>
    IEnumerator DispatchSoliders() {
        // 游戏开始后等待2秒再出兵
        yield return new WaitForSeconds(2);
        
        // 只要游戏没有结束就持续出兵
        while (!isGameOver) {
            int kind = 0; // 用于标记路线（上中下）

            // 遍历每个红方出兵点，进行出兵
            foreach (var p in placesOfDispatchRed) {
                // 对每个出兵点产生一群单位
                foreach (var solider in solidersPrefabs) {
                    // 在出兵点附近随机位置生成小兵，避免重叠
                    Vector3 position = (p + UnityEngine.Random.insideUnitSphere * 3);

                    // 从对象池中取出对象
                    GameObject soliderObject = poolObjectFactory.AcquireObject(position, templateObject: solider);

                    // 设置该单位的阵营为红方
                    soliderObject.GetComponent<CharacterMono>().characterModel.unitFaction = UnitFaction.Red;

                    // 触发产生对象的事件，用于网络同步
                    if (OnCreateNPCGameObject != null) OnCreateNPCGameObject(position,soliderObject, poolObjectFactory);

                    // 根据kind值设置小兵的路线
                    if (kind == 0) {
                        // 上路
                        soliderObject.GetComponent<CharacterMono>().wayPointsUnit = new WayPointsUnit(WayPointEnum.UpRoad, UnitFaction.Red);
                    } else if (kind == 1) {
                        // 中路
                        soliderObject.GetComponent<CharacterMono>().wayPointsUnit = new WayPointsUnit(WayPointEnum.MiddleRoad, UnitFaction.Red);
                    } else if (kind == 2) {
                        // 下路
                        soliderObject.GetComponent<CharacterMono>().wayPointsUnit = new WayPointsUnit(WayPointEnum.DownRoad, UnitFaction.Red);
                    }
                    
                    // 将小兵添加到战争迷雾系统中
                    FogSystem.Instace.AddFOVUnit(soliderObject.GetComponent<CharacterMono>());
                }

                kind++; // 切换到下一条路线
            }

            // 蓝方出兵逻辑，与红方类似
            int b = 0;
            foreach (var p in placesOfDispatchBlue) {
                // 对每个出兵点产生一群单位
                foreach (var solider in solidersPrefabs) {
                    Vector3 position = (p + UnityEngine.Random.insideUnitSphere * 3);
                    GameObject soliderObject = poolObjectFactory.AcquireObject(position, templateObject: solider);

                    // 设置该单位的阵营为蓝方
                    soliderObject.GetComponent<CharacterMono>().characterModel.unitFaction = UnitFaction.Blue;

                    // 触发产生对象的事件，用于网络同步
                    if (OnCreateNPCGameObject != null) OnCreateNPCGameObject(position, soliderObject,poolObjectFactory);

                    // 根据b值设置小兵的路线
                    if (b == 0) {
                        // 上路
                        soliderObject.GetComponent<CharacterMono>().wayPointsUnit = new WayPointsUnit(WayPointEnum.UpRoad, UnitFaction.Blue);
                    } else if (b == 1) {
                        // 中路
                        soliderObject.GetComponent<CharacterMono>().wayPointsUnit = new WayPointsUnit(WayPointEnum.MiddleRoad, UnitFaction.Blue);
                    } else if (b == 2) {
                        // 下路
                        soliderObject.GetComponent<CharacterMono>().wayPointsUnit = new WayPointsUnit(WayPointEnum.DownRoad, UnitFaction.Blue);
                    }

                    // 将小兵添加到战争迷雾系统中
                    FogSystem.Instace.AddFOVUnit(soliderObject.GetComponent<CharacterMono>());
                }
                b++; // 切换到下一条路线
            }

            // 等待指定时间后再次出兵
            yield return new WaitForSeconds(deltaTime);
        }
    }

    /// <summary>
    /// 在Scene视图中可视化出兵点位置
    /// </summary>
    private void OnDrawGizmosSelected() {
        // 红方出兵点显示为红色
        Gizmos.color = Color.red;
        foreach (var position in placesOfDispatchRed) {
            Gizmos.DrawSphere(position,1f);
        }
        
        // 蓝方出兵点显示为蓝色
        Gizmos.color = Color.blue;
        foreach (var position in placesOfDispatchBlue) {
            Gizmos.DrawSphere(position, 1f);
        }
    }
}

