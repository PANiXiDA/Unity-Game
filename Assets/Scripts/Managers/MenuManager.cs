using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Helpers;
using TMPro;
using Assets.Scripts.Enumerations;
using Assets.Scripts.Managers;
using Unity.Mathematics;
using Cysharp.Threading.Tasks;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private GameObject _tileInfoPanel, _unitInfoPanel, _chatPanel, _endBattlePanel, _surrenderPanel, _exitBtn;
    [SerializeField] private RectTransform _ATBIcons;

    private void Awake()
    {
        Instance = this;
    }
    public void ShowOrientationOfAttack(Tile tile)
    {
        var enemyPos = GridManager.Instance.GetTileCoordinate(tile);
        var cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float normalizedAngle = Mathf.Atan2(cursorPos.y - enemyPos.y, cursorPos.x - enemyPos.x) * Mathf.Rad2Deg;
        normalizedAngle = (normalizedAngle + 360) % 360;

        var selectedHero = UnitManager.Instance.SelectedHero;
        var tilesForMove = UnitManager.Instance.GetTilesForMove(selectedHero);

        if (selectedHero != null)
        {
            if (UnitManager.Instance.IsRangeAttackPossible(selectedHero))
            {
                ShowArrowAttack(enemyPos, selectedHero);
            }
            else
            {
                ShowSwordAttack(enemyPos, normalizedAngle, tilesForMove);
            }
        }
    }
    private void ShowSwordAttack(Vector2 enemyPos, float normalizedAngle, Dictionary<Vector2, Tile> tilesForMove)
    {
        var swordsList = Resources.LoadAll<GameObject>("Swords").ToList();
        var swordPositions = new Dictionary<string, (Vector2 position, float angleRangeStart, float angleRangeEnd)>
        {
            {"Bottom", (new Vector2(enemyPos.x, enemyPos.y + 0.75f), 67.5f, 112.5f)},
            {"BottomLeft", (new Vector2(enemyPos.x + 0.75f, enemyPos.y + 0.75f), 22.5f, 67.5f)},
            {"Left", (new Vector2(enemyPos.x + 0.75f, enemyPos.y), 0f, 22.5f)},
            {"TopLeft", (new Vector2(enemyPos.x + 0.75f, enemyPos.y - 0.75f), 292.5f, 337.5f)},
            {"Top", (new Vector2(enemyPos.x, enemyPos.y - 0.75f), 247.5f, 292.5f)},
            {"TopRight", (new Vector2(enemyPos.x - 0.75f, enemyPos.y - 0.75f), 202.5f, 247.5f)},
            {"Right", (new Vector2(enemyPos.x - 0.75f, enemyPos.y), 157.5f, 202.5f)},
            {"BottomRight", (new Vector2(enemyPos.x - 0.75f, enemyPos.y + 0.75f), 112.5f, 157.5f)},
        };

        foreach (var (swordName, (position, angleStart, angleEnd)) in swordPositions)
        {
            var tilePos = new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
            var tile = GridManager.Instance.GetTileAtPosition(tilePos);

            if (tile != null &&
                normalizedAngle >= angleStart && normalizedAngle < angleEnd &&
                GameObject.Find($"{swordName}(Clone)") is null &&
                (tilesForMove.ContainsKey(tilePos) || tile.OccupiedUnit == UnitManager.Instance.SelectedHero))
            {
                DeleteSwords();
                var sword = swordsList.Find(e => e.name == swordName);
                Instantiate(sword, position, Quaternion.identity);
            }
        }
    }
    private void ShowArrowAttack(Vector2 enemyPos, BaseUnit selectedHero)
    {
        var arrowsList = Resources.LoadAll<GameObject>("Arrows").ToList();

        var distance = math.ceil(math.sqrt(math.pow(enemyPos.x - selectedHero.OccupiedTile.Position.x, 2) +
            math.pow(enemyPos.y - selectedHero.OccupiedTile.Position.y, 2)));

        var tilePos = new Vector2(enemyPos.x - 1, enemyPos.y);
        var tile = GridManager.Instance.GetTileAtPosition(tilePos);

        if (tile != null && GameObject.Find("Arrow(Clone)") is null && selectedHero.UnitRange >= distance)
        {
            DeleteArrows();
            var arrow = arrowsList.Find(e => e.name == "Arrow");
            Instantiate(arrow, new Vector3(tilePos.x + 0.25f, tilePos.y), Quaternion.identity);
        }
        else if (tile != null && GameObject.Find("BreakArrow(Clone)") is null && selectedHero.UnitRange < distance)
        {
            DeleteArrows();
            var arrow = arrowsList.Find(e => e.name == "BreakArrow");
            Instantiate(arrow, new Vector3(tilePos.x + 0.15f, tilePos.y), Quaternion.identity);
        }
    }
    public void DeleteSwords()
    {
        var swordsList = Resources.LoadAll<GameObject>("Swords").ToList();
        foreach (var sword in swordsList)
        {
            if (GameObject.Find(sword.name + "(Clone)") is not null)
            {
                Destroy(GameObject.Find(sword.name + "(Clone)"));
            }
        }
    }
    public void DeleteArrows()
    {
        var arrowsList = Resources.LoadAll<GameObject>("Arrows").ToList();
        foreach (var arrow in arrowsList)
        {
            if (GameObject.Find(arrow.name + "(Clone)") is not null)
            {
                Destroy(GameObject.Find(arrow.name + "(Clone)"));
            }
        }
    }
    public void DisplayDamageWithDeathCountInChat(BaseUnit hero, BaseUnit enemy, int damage, int countDeaths)
    {
        _chatPanel.GetComponentInChildren<TextMeshProUGUI>().text += $"<color=red>{hero.UnitName}</color> ����� {damage} ����� �� <color=blue>{enemy.UnitName}</color>." +
            $"{(countDeaths > 0 ? $" ������� {countDeaths}.\n" : $"\n")}";

        Scrollbar scrollbar = _chatPanel.GetComponentInChildren<Scrollbar>(true);
        AutoScrollToBottom(scrollbar).Forget();
    }
    public void SendMessageToChat()
    {
        TMP_InputField inputField = _chatPanel.GetComponentInChildren<TMP_InputField>();
        _chatPanel.GetComponentInChildren<TextMeshProUGUI>().text += $"<color=red>[������������ �����]</color>: {inputField.text}\n";
        inputField.text = "";
    }
    private async UniTaskVoid AutoScrollToBottom(Scrollbar scrollbar)
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        await UniTask.DelayFrame(2);

        scrollbar.value = 0f;
    }

    public void ShowUnitsPortraits()
    {
        foreach (var ATBunit in TurnManager.Instance.ATB)
        {
            CreatePortrait(ATBunit);
        }
    }

    public void ClearExistingIcons(BaseUnit unit)
    {
        var name = unit.name.Replace("(Clone)", "");
        foreach (Transform child in _ATBIcons)
        {
            if (child.name == name)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void CreatePortrait(BaseUnit ATBunit)
    {
        string unitName = ATBunit.name.Replace("(Clone)", "");

        GameObject portrait = new GameObject(unitName);
        portrait.transform.SetParent(_ATBIcons, false);

        Image image = portrait.AddComponent<Image>();
        image.sprite = Resources.Load<Sprite>($"Icons/{unitName}");

        CreateUnitCountText(portrait.transform, ATBunit.UnitCount);
        CreateContour(portrait.transform, ATBunit.Faction);
    }
    public void UpdatePortraits(BaseUnit newUnit)
    {
        if (_ATBIcons.childCount > 0)
        {
            Destroy(_ATBIcons.GetChild(0).gameObject);
        }

        CreatePortrait(newUnit);
    }
    public void UpdatePortraitsInfo(BaseUnit unit)
    {
        if (unit.UnitCount > 0)
        {
            var name = unit.name.Replace("(Clone)", "");
            foreach (Transform child in _ATBIcons)
            {
                if (child.name == name)
                {
                    Transform unitCountChild = child.Find("UnitCount");
                    unitCountChild.GetComponent<TextMeshProUGUI>().text = unit.UnitCount.ToString();
                }
            }
        }
    }

    public void CreateUnitCountText(Transform parent, int unitCount)
    {
        GameObject unitCountObject = new GameObject("UnitCount");
        unitCountObject.transform.SetParent(parent, false);

        TextMeshProUGUI unitCountTextMeshPro = unitCountObject.AddComponent<TextMeshProUGUI>();
        unitCountTextMeshPro.rectTransform.localPosition = new Vector3(-3, -60, 0);
        unitCountTextMeshPro.rectTransform.sizeDelta = new Vector2(170, 40);
        unitCountTextMeshPro.text = unitCount.ToString();
        unitCountTextMeshPro.fontSize = 48;
        unitCountTextMeshPro.alignment = TextAlignmentOptions.Right;
        unitCountTextMeshPro.color = Color.yellow;
    }

    private void CreateContour(Transform parent, Faction faction)
    {
        GameObject contour = new GameObject("Contour");
        contour.transform.SetParent(parent, false);
        contour.transform.localPosition = new Vector3(190, -180, 0);
        contour.transform.localScale = new Vector3(180, 170, 1);

        SpriteRenderer spriteRenderer = contour.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>("Contour");
        spriteRenderer.sortingOrder = 1;
        spriteRenderer.color = faction == Faction.Hero ? Color.red : Color.blue;
    }
    public void ShowTileInfo(Tile tile)
    {
        if (tile == null)
        {
            HideTileInfoPanels();
            return;
        }

        UpdateTileInfoPanel(tile);

        if (tile.OccupiedUnit != null)
        {
            UpdateUnitInfoPanel(tile.OccupiedUnit);
        }
        else
        {
            _unitInfoPanel.SetActive(false);
        }
    }
    private void HideTileInfoPanels()
    {
        _tileInfoPanel.SetActive(false);
        _unitInfoPanel.SetActive(false);
    }
    private void UpdateTileInfoPanel(Tile tile)
    {
        var tileCoordinate = GridManager.Instance.GetTileCoordinate(tile);
        _tileInfoPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"x = {tileCoordinate.x}\ny = {tileCoordinate.y}";
        _tileInfoPanel.SetActive(true);
    }
    private void UpdateUnitInfoPanel(BaseUnit unit)
    {
        string abilitiesText = string.Join(". ", unit.abilities.Select(ability => EnumHelper.GetDescription(ability)));

        SetUnitInfoText("UnitName", unit.UnitName);
        SetUnitInfoText("AttackValue", unit.UnitAttack.ToString());
        SetUnitInfoText("DefenceValue", unit.UnitDefence.ToString());
        SetUnitInfoText("HealthValue", $"{unit.UnitCurrentHealth}/{unit.UnitFullHealth}");
        SetUnitInfoText("ArrowsValue", unit.UnitArrows != null ? unit.UnitArrows.ToString() : "-");
        SetUnitInfoText("RangeValue", unit.UnitRange != null ? unit.UnitRange.ToString() : "-");
        SetUnitInfoText("DamageValue", $"{unit.UnitMinDamage} - {unit.UnitMaxDamage}");
        SetUnitInfoText("SpeedValue", $"{unit.UnitSpeed}");
        SetUnitInfoText("InitiativeValue", unit.UnitInitiative.ToString().Replace(',', '.'));
        SetUnitInfoText("MoraleValue", unit.UnitMorale.ToString());
        SetUnitInfoText("LuckValue", unit.UnitLuck.ToString());
        SetUnitInfoText("Abilities", abilitiesText);

        _unitInfoPanel.SetActive(true);
    }
    private void SetUnitInfoText(string parameterName, string textValue)
    {
        _unitInfoPanel.GetComponentsInChildren<TextMeshProUGUI>(true)
            .Where(item => item.name == parameterName)
            .FirstOrDefault()
            .text = textValue;
    }
    public void SetPanelTexts(GameObject panel, string battleResultText, string winSideInfoText, string loseSideInfoText)
    {
        var texts = panel.GetComponentsInChildren<TextMeshPro>(true);

        texts.FirstOrDefault(item => item.name == "BattleResultText").text = battleResultText;
        texts.FirstOrDefault(item => item.name == "WinSideInfoText").text = winSideInfoText;
        texts.FirstOrDefault(item => item.name == "LoseSideInfoText").text = loseSideInfoText;

        panel.SetActive(true);
    }

    public void WinPanel()
    {
        _exitBtn.SetActive(false);
        SetPanelTexts(
            _endBattlePanel,
            "������",
            "<color=#FF6666>������������ �����</color> �������!",
            "<color=#10CEEB>������������� ���������</color> ������� �������� ���������!"
        );
    }

    public void LosePanel()
    {
        _exitBtn.SetActive(false);
        SetPanelTexts(
            _unitInfoPanel,
            "���������",
            "<color=#FF6666>������������� ���������</color> �������!",
            "<color=#10CEEB>������������ �����</color> ������� �������� ���������!"
        );
    }
    public void ShowSurrenderPanel()
    {
        _surrenderPanel.SetActive(true);
    }
    public void CloseSurrenderPanel()
    {
        _surrenderPanel.SetActive(false);
    }
}
