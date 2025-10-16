using UnityEngine;

public class PlayerUnit : UnitBase
{
    [Header("Данные класса игрока")]
    public ClassData classData;

    [Header("Прогресс игрока")]
    public int level = 1;

    [Header("Бонусы (постоянные, накапливаются на порогах уровней)")]
    [HideInInspector] public int strengthBonus = 0;
    [HideInInspector] public int dexterityBonus = 0;
    [HideInInspector] public int enduranceBonus = 0;

    [Header("Взгляд и анимации")]
    [SerializeField] private GameObject enemyObject; // ближайший враг для ориентации


    public float TotalStrength => strength + strengthBonus;
    public float TotalDexterity => dexterity + dexterityBonus;
    public float TotalEndurance => endurance + enduranceBonus;

    // ==========================================================
    // 🔹 ИНИЦИАЛИЗАЦИЯ
    // ==========================================================
    public override void Initialize(BattleSystem battle, TurnManager turn)
    {
        base.Initialize(battle, turn);

        if (classData != null)
        {
            unitName = classData.className;
            strength = Random.Range(1, 4);
            dexterity = Random.Range(1, 4);
            endurance = Random.Range(1, 4);

            maxHealth = classData.healthPerLevel + Mathf.RoundToInt(endurance);
            currentHealth = maxHealth;

            if (classData.startingWeapon != null)
            {
                EquipWeapon(classData.startingWeapon);
                Debug.Log($"⚙️ {unitName} экипирован стартовым оружием: {classData.startingWeapon.weaponName}");
            }
            else
            {
                weaponDamage = 2f;
            }
        }
        else
        {
            Debug.LogWarning($"{name}: classData не назначен!");
        }
    }

    // ==========================================================
    // ⚔️ ХОД ИГРОКА
    // ==========================================================
    public override void Act(UnitBase target)
    {
        if (isDead || target == null) return;

        if (battleSystem != null)
            battleSystem.ProcessAttack(this, target);
        else
            Debug.LogWarning("battleSystem не задан в PlayerUnit.");
    }

    // ==========================================================
    // 🗡️ Экипировка оружия
    // ==========================================================
    public void EquipWeapon(WeaponData newWeapon)
    {
        currentWeapon = newWeapon;
        weaponDamage = newWeapon.baseDamage;
    }

    // ==========================================================
    // 📈 ПОВЫШЕНИЕ УРОВНЯ
    // ==========================================================
    public void LevelUp()
    {
        if (level >= 5)
        {
            RestoreHealth();
            Debug.Log($"✅ Максимальный уровень достигнут (5). Здоровье восстановлено.");
            return;
        }

        level++;

        int hpGain = classData != null ? classData.healthPerLevel : 5;
        int endGain = Mathf.RoundToInt(TotalEndurance);
        maxHealth += hpGain + endGain;
        currentHealth = maxHealth;

        if (classData != null)
        {
            string cls = classData.className.ToLower();

            if (cls.Contains("разбой"))
            {
                if (level == 2) dexterityBonus += 1;
            }
            else if (cls.Contains("воин"))
            {
                if (level == 3) strengthBonus += 1;
            }
            else if (cls.Contains("варвар"))
            {
                if (level == 3) enduranceBonus += 1;
            }
        }

        Debug.Log($"⬆️ {unitName} повысил уровень до {level}! +HP: {hpGain}+{Mathf.RoundToInt(TotalEndurance)} → {maxHealth}.");
    }

    // ==========================================================
    public void RestoreHealth()
    {
        currentHealth = maxHealth;
        Debug.Log($"❤️ {unitName} восстановил здоровье ({currentHealth}/{maxHealth})");
    }

    // ==========================================================
    // 👀 ВЗГЛЯД НА БЛИЖАЙШЕГО ВРАГА
    // ==========================================================
    public void FindAndLookAtNearestEnemy(float rotationSpeed = 360f)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return;

        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }

        enemyObject = nearest;

        if (enemyObject != null)
        {
            Vector3 direction = enemyObject.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void Update()
    {
        if (!isDead)
            FindAndLookAtNearestEnemy();
    }
}
