﻿using System.Collections;
using UnityEngine;

public class NetWorkTest1 : MonoBehaviour{

    public GameObject soldier;

    // 小兵的对象池
    private GameObjectPool poolObjectFactory;

    // 产生对象的事件,用于解耦网络联机产生NPC的逻辑
    public delegate void CreateNPCGameObject(Vector3 position, GameObject soldierObject, GameObjectPool gameObjectPool);
    public event CreateNPCGameObject OnCreateNPCGameObject;

    public void Start() {
        poolObjectFactory = new GameObjectPool(50);

        OnCreateNPCGameObject += NetWorkManager.Instance.AddNetworkNpc;
    }

    IEnumerator Dispatchsoldiers() {
        for (int i=0;i<5;i++) {
            GameObject soldierObject = poolObjectFactory.AcquireObject(Random.insideUnitCircle*3, templateObject: soldier);

            soldierObject.GetComponent<CharacterMono>().characterModel.unitFaction = i%2==0? UnitFaction.Blue : UnitFaction.Red;

            // 触发产生对象的事件
            if (OnCreateNPCGameObject != null) OnCreateNPCGameObject(Vector3.zero, soldierObject, poolObjectFactory);

            yield return new WaitForSeconds(5f);
        }
        
    }

    public void BecomeHomeOwner() {

        // 成为房主
        NetWorkManager.Instance.IsHomeowner = true;

        // 出兵
        StartCoroutine(Dispatchsoldiers());
    }
}

