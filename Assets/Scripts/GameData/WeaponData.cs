using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game Data/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Основные параметры оружия")]
    public string weaponName = "Меч";
    public float baseDamage = 3f;

    [Header("Тип урона")]
    public DamageType damageType = DamageType.Slashing;

    [TextArea(2, 3)]
    public string description = "Стандартное оружие ближнего боя.";

    // 🔹 Типы урона
    public enum DamageType
    {
        Slashing,   // рубящий
        Blunt,      // дробящий
        Piercing,   // колющий
        Fire,       // огненный (если нужно для Дракона)
        Magic       // магический (для Призрака, если захочешь)
    }
}
