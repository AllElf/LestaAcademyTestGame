using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Основная информация")]
    public string enemyName = "Гоблин";
    public float health = 10f;

    [Header("Характеристики")]
    public float strength = 2f;
    public float dexterity = 2f;
    public float endurance = 1f;

    [Header("Оружие врага (трофей после победы)")]
    public WeaponData weapon;

    [Header("Особенность врага (опционально)")]
    [TextArea(2, 3)]
    public string specialAbility = "Нет особенностей.";

    [Header("Награда за победу (для логов)")]
    public string rewardText = "Получен трофей: оружие врага";
}
