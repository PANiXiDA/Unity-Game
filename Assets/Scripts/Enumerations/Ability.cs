﻿using System.ComponentModel;

namespace Assets.Scripts.Enumerations
{
    public enum Ability
    {
        [Description("Летающее существо")]
        Fly = 0,
        [Description("Стрелок")]
        Archer = 1,
        [Description("Двойной удар")]
        DoubleAttack = 2,
        [Description("Огненное дыхание")]
        FieryBreath = 3,
        [Description("Нежить")]
        Undead = 4,
        [Description("Площадный выстрел")]
        AreaRangeAttack = 5,
        [Description("Нет штрафа в ближнем бою")]
        FullMeleeDamage = 6,
        [Description("Отбрасывающий удар")]
        DiscardMeleeAttack = 7,
        [Description("Большой щит")]
        BigShield = 8,
        [Description("Враг не сопротивляется")]
        NoResponseAttack = 9,
        [Description("Бдительность")]
        AttackFirst = 10,
        [Description("Рыцарский разбег")]
        KnightRunUp = 11,
    }
}
