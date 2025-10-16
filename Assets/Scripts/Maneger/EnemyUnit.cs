using UnityEngine;

public class EnemyUnit : UnitBase
{
    [Header("Данные врага")]
    public EnemyData enemyData;

    [Header("Взгляд и анимации")]
    [SerializeField] private GameObject playerObject; // ближайший игрок для ориентации

    // ==========================================================
    // 🔹 ИНИЦИАЛИЗАЦИЯ
    // ==========================================================
    public override void Initialize(BattleSystem battle, TurnManager turn)
    {
        base.Initialize(battle, turn);

        if (enemyData != null)
        {
            unitName = enemyData.enemyName;
            maxHealth = Mathf.RoundToInt(enemyData.health);
            currentHealth = maxHealth;

            strength = enemyData.strength;
            dexterity = enemyData.dexterity;
            endurance = enemyData.endurance;

            if (enemyData.weapon != null)
            {
                currentWeapon = enemyData.weapon;
                weaponDamage = currentWeapon.baseDamage;
            }
            else
            {
                weaponDamage = 3f;
            }
        }
        else
        {
            Debug.LogWarning($"{name}: EnemyData не назначен — используются дефолтные значения.");
        }
    }

    // ==========================================================
    // ⚔️ АТАКА
    // ==========================================================
    public override void Act(UnitBase target)
    {
        if (isDead || target == null) return;

        if (battleSystem != null)
            battleSystem.ProcessAttack(this, target);
        else
            Debug.LogWarning("battleSystem не задан в EnemyUnit.");
    }

    // ==========================================================
    // ☠️ СМЕРТЬ
    // ==========================================================
    protected override void Die()
    {
        base.Die();
    }

    // ==========================================================
    // 👀 ВЗГЛЯД НА БЛИЖАЙШЕГО ИГРОКА
    // ==========================================================
    public void FindAndLookAtNearestPlayer(float rotationSpeed = 360f)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return;

        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = player;
            }
        }

        playerObject = nearest;

        if (playerObject != null)
        {
            Vector3 direction = playerObject.transform.position - transform.position;
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
            FindAndLookAtNearestPlayer();
    }
}
