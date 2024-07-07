﻿using Assets.Scripts.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance;

        public List<BaseUnit> allUnits = new List<BaseUnit>();

        public List<BaseUnit> ATB = new List<BaseUnit>();
        private void Awake()
        {
            Instance = this;
        }

        public void SetATB()
        {
            allUnits.AddRange(SpawnManager.Instance.PlayerUnits);
            allUnits.AddRange(SpawnManager.Instance.EnemyUnits);
            allUnits.Sort((x, y) => x.UnitTime.CompareTo(y.UnitTime));

            while (ATB.Count < 100)
            {
                var time = allUnits.FirstOrDefault().UnitTime;
                foreach (var unit in allUnits)
                {
                    unit.UnitATB += unit.UnitInitiative * time;
                    if (unit.UnitATB >= 100)
                    {
                        unit.UnitATB -= 100;
                        ATB.Add(unit);
                    }
                    unit.UnitTime = (100 - unit.UnitATB) / unit.UnitInitiative;
                }
                allUnits.Sort((x, y) => x.UnitTime.CompareTo(y.UnitTime));
            }

            MenuManager.Instance.ShowUnitsPortraits();

            StartTurn(ATB.First());
        }

        public void UpdateATB()
        {
            BaseUnit currentUnit = allUnits.FirstOrDefault();
            currentUnit.GetComponent<SpriteRenderer>().sortingOrder = 1;
            var unitTime = currentUnit.UnitTime;
            ATB.RemoveAt(0);

            foreach (var ATBunit in allUnits)
            {
                ATBunit.UnitATB += ATBunit.UnitInitiative * unitTime;
                if (ATBunit.UnitATB >= 100)
                {
                    ATBunit.UnitATB -= 100;
                    ATB.Add(ATBunit);
                }
                ATBunit.UnitTime = (100 - ATBunit.UnitATB) / ATBunit.UnitInitiative;
            }

            allUnits.Sort((x, y) => x.UnitTime.CompareTo(y.UnitTime));

            MenuManager.Instance.ShowUnitsPortraits();
        }

        public void StartTurn(BaseUnit unit)
        {
            unit.UnitResponse = true;
            unit.GetComponent<SpriteRenderer>().sortingOrder = 2;

            if (unit.Faction == Faction.Hero)
            {
                UnitManager.Instance.SetSelectedHero(unit);
                Tile.Instance.SetHighlight(UnitManager.Instance.SelectedHero);
                GameManager.Instance.ChangeState(GameState.HeroesTurn);
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.EnemiesTurn);
            }
        }

        public void EndTurn(BaseUnit unit)
        {
            if (GameManager.Instance.GameState == GameState.HeroesTurn)
            {
                UnitManager.Instance.SetSelectedHero(null);
            }

            UpdateATB();

            if (SpawnManager.Instance.EnemyUnits.Count == 0)
            {
                MenuManager.Instance.WinPanel();
                GameManager.Instance.ChangeState(GameState.GameOver);
            }
            else if (SpawnManager.Instance.PlayerUnits.Count == 0)
            {
                MenuManager.Instance.LosePanel();
                GameManager.Instance.ChangeState(GameState.GameOver);
            }
            else
            {
                StartTurn(ATB.First());
            }
        }
    }
}