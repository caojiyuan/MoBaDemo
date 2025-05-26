using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 路径点单位类，用于管理游戏单位的寻路路径
/// 在MOBA游戏中，小兵会沿着特定的路线前进（上路、中路、下路）
/// 该类管理单位沿着指定路线移动的路径点
/// </summary>
public class WayPointsUnit {
    /// <summary>
    /// 该单位寻路的类型，表示上中下三条路径中的哪一条
    /// </summary>
    private WayPointEnum wayPointType;
    
    /// <summary>
    /// 单位所属阵营（红方或蓝方）
    /// 不同阵营的单位沿相反方向移动
    /// </summary>
    private UnitFaction unitFaction;

    /// <summary>
    /// 该单位拥有的路径点列表，表示单位需要经过的所有位置
    /// </summary>
    private List<Vector3> wayPoints;
    
    /// <summary>
    /// 该单位当前所在的路径点索引
    /// 红方从0开始递增，蓝方从最大值开始递减
    /// </summary>
    public int nowIndex = 0;

    /// <summary>
    /// 根据路径类型和阵营初始化路径点
    /// </summary>
    /// <param name="WayPointType">路径类型（上路、中路、下路）</param>
    /// <param name="unitFaction">单位所属阵营</param>
    public WayPointsUnit(WayPointEnum WayPointType, UnitFaction unitFaction) {
        switch (WayPointType) {
            case WayPointEnum.UpRoad:
                // 获取上路路径点
                wayPoints = WayPointsManager.Instance.upRoadPoints;
                break;
            case WayPointEnum.MiddleRoad:
                // 获取中路路径点
                wayPoints = WayPointsManager.Instance.middleRoadPoints;
                break;
            case WayPointEnum.DownRoad:
                // 获取下路路径点
                wayPoints = WayPointsManager.Instance.downRoadPoints;
                break;
        }
        
        this.unitFaction = unitFaction;
        
        // 根据阵营设置初始索引位置
        // 蓝方从终点（最大索引）开始向起点移动
        // 红方从起点（索引0）开始向终点移动
        switch (unitFaction) {
            case UnitFaction.Blue:
                nowIndex = wayPoints.Count - 1;
                break;
            case UnitFaction.Red:
                nowIndex = 0;
                break;
        }
    }

    /// <summary>
    /// 获取单位的下一个目标路径点
    /// 红方索引递增，蓝方索引递减
    /// </summary>
    /// <returns>下一个路径点的世界坐标</returns>
    public Vector3 GetNextWayPoint() {
        if (unitFaction == UnitFaction.Red) {
            // 红方单位到达终点后停留在终点
            if (nowIndex == wayPoints.Count - 1) {
                return wayPoints[nowIndex];
            }
            // 返回当前路径点，并将索引递增
            return wayPoints[nowIndex++];
        } else if (unitFaction == UnitFaction.Blue) {
            // 蓝方单位到达起点后停留在起点
            if (nowIndex == 0) {
                return wayPoints[nowIndex];
            }
            // 返回当前路径点，并将索引递减
            return wayPoints[nowIndex--];
        }
        return Vector3.zero; // 默认返回原点（理论上不会执行到这里）
    }

    /// <summary>
    /// 获取单位当前的路径点位置
    /// </summary>
    /// <returns>当前路径点的世界坐标</returns>
    public Vector3 GetNowWayPoint() {
        return wayPoints[nowIndex];
    }

    /// <summary>
    /// 找到离单位当前位置最近的路径点，并更新当前索引
    /// 用于单位偏离路径时重新找到合适的路径点继续前进
    /// </summary>
    /// <param name="position">单位当前位置</param>
    public void GetNearestWayPoint(Vector3 position) {
        // 如果已经到达终点，则不再寻找
        if (nowIndex == wayPoints.Count-1) return;

        // 从所有路径点中找到距离当前位置最近的点
        float distance = Vector3.Distance(position, wayPoints[nowIndex]);
        int nearestPointIndex = nowIndex;
        
        for (int i = 0; i < wayPoints.Count; i++) {
            float tempDistance = Vector3.Distance(position, wayPoints[i]);
            //Debug.Log("TempDistance:"+tempDistance+" i:"+i);
            
            // 如果找到更近的点，更新最近点信息
            if (tempDistance < distance) {
                distance = tempDistance;
                nearestPointIndex = i;
            }            
        }
        //Debug.Log("最近的点是："+nearestPointIndex+" 距离是："+distance);

        // 设置下一个目标点为最近点之后的一个点（红方）或之前的一个点（蓝方）
        nowIndex = nearestPointIndex + (unitFaction == UnitFaction.Red ? 1 : -1);

        // 确保索引不超出范围
        if (nowIndex >= wayPoints.Count()) {
            nowIndex = wayPoints.Count() - 1; // 不超过最大索引
        } else if (nowIndex < 0) {
            nowIndex = 0; // 不小于0
        }
    }
}

