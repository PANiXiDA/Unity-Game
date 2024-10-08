﻿using Assets.Scripts.IActions;
using Assets.Scripts.Interfaces;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Actions.Attack.RangeAttack
{
    public class DefaultRangeAttack : IRangeAttack
    {
        private IDamage _damageCalculator;
        public DefaultRangeAttack(IDamage damageCalculator)
        {
            _damageCalculator = damageCalculator;
        }
        public async UniTask RangeAttack(BaseUnit attacker, BaseUnit defender)
        {
            Tile.Instance.DeleteHighlight();
            attacker.isBusy = true;

            attacker.UnitArrows -= 1;
            attacker.animator.Play("RangeAttack");

            bool isLuck = UnitManager.Instance.Luck(attacker);
            _damageCalculator.isLuck = isLuck;

            await defender.TakeRangeDamage(attacker, defender, _damageCalculator);
            bool death = UnitManager.Instance.IsDead(defender);
            if (death)
            {
                await defender.Death();
            }

            await UniTask.Delay(1000);
            attacker.isBusy = false;
        }
    }
}
