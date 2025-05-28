﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSM;
using UnityEngine;


class IdleToAttackTransition : FSMTransition {
    public override FSMState GetNextState() {
        return NextState;
    }


    // 选择一个敌人来进攻
    private CharacterMono ChooseOneEnemry() {
        try {
            while (BlackBorad.CharacterMono.arroundEnemies[0] == null || !BlackBorad.CharacterMono.arroundEnemies[0].IsCanBeAttack()) BlackBorad.CharacterMono.arroundEnemies.RemoveAt(0);
            return BlackBorad.CharacterMono.arroundEnemies[0];
        } catch (Exception) {
            Debug.LogWarning("怪物AI状态转换出现异常");
            return null;
        }
    }


    public override bool IsValid() {
        //Debug.Log("正处于IdleToAttackTransition中,周围敌人的数量是:" + BlackBorad.CharacterMono.arroundEnemies.Count);

        if (BlackBorad.CharacterMono.arroundEnemies.Count > 0) {
            CharacterMono target = ChooseOneEnemry();
            if (target == null) return false;
            BlackBorad.SetTransform("EnemryTransform", target.transform);
            BlackBorad.SetComponent("Enemry", target);
            return true;
        }
        return false;
    }

    public override void OnTransition() {
        
    }
}

