using UnityEngine;

[CreateAssetMenu(fileName = "New_Class", menuName = "Scriptable Objects/Class Player", order = 0)]
public class ClassData : ScriptableObject
{
    [Header("Основные параметры класса")]
    [Tooltip("Название класса (например: Warrior, The Barbarian, Outlaw)")]
    public string className;

    [Tooltip("Количество здоровья, добавляемое за уровень")]
    public int healthPerLevel = 5;

    [Tooltip("Оружие, с которым персонаж начинает игру")]
    public WeaponData startingWeapon;

    [Tooltip("Префаб игрока для этого класса (используется при спавне)")]
    public GameObject playerPrefab;

    [Header("Визуальные данные")]
    [Tooltip("Иконка класса для меню выбора персонажа")]
    public Sprite classIcon;

    [Header("Описание бонусов по уровням")]
    [TextArea(2, 4)]
    [Tooltip("Описание эффекта на 1 уровне")]
    public string bonusLevel1;

    [TextArea(2, 4)]
    [Tooltip("Описание эффекта на 2 уровне")]
    public string bonusLevel2;

    [TextArea(2, 4)]
    [Tooltip("Описание эффекта на 3 уровне")]
    public string bonusLevel3;

    [Header("Описание класса (необязательно)")]
    [TextArea(3, 6)]
    [Tooltip("Общее текстовое описание класса")]
    public string description;
}
