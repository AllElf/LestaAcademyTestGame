using UnityEngine;
using System.Collections;

public class UnitBase : MonoBehaviour
{
    [Header("Идентификатор")]
    public string unitName = "Unit";

    [Header("Характеристики")]
    public int maxHealth = 10;
    public float currentHealth = 10f;

    public float strength = 1f;
    public float dexterity = 1f;
    public float endurance = 1f;

    [Header("Оружие")]
    public WeaponData currentWeapon;
    public float weaponDamage = 1f;

    [Header("Состояние")]
    public bool isDead = false;

    protected BattleSystem battleSystem;
    protected TurnManager turnManager;

    [Header("Анимация и звук")]
    [SerializeField] protected AnimationControllerUnit animController;
    public AnimationControllerUnit AnimController => animController;

    public Cell CurrentCell { get; private set; }

    private Vector3 startPosition;

    // ==========================================================
    // 🔹 ИНИЦИАЛИЗАЦИЯ
    // ==========================================================
    public virtual void Initialize(BattleSystem battle, TurnManager turn)
    {
        battleSystem = battle;
        turnManager = turn;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        isDead = currentHealth <= 0;
        startPosition = transform.position;

        // Подключаем контроллер анимации (если есть)
        animController = GetComponentInChildren<AnimationControllerUnit>();
        if (animController != null)
            animController.PlayIdle();
    }

    // ==========================================================
    // 🔹 ПРИВЯЗКА К КЛЕТКЕ
    // ==========================================================
    public virtual void BindToCell(Cell cell)
    {
        if (cell == null) return;

        if (CurrentCell != null)
            CurrentCell.SetOccupied(false);

        CurrentCell = cell;
        CurrentCell.SetOccupied(true);
        transform.position = cell.transform.position;
        startPosition = transform.position;
    }

    // ==========================================================
    // 🔹 ДЕЙСТВИЕ (переопределяется у наследников)
    // ==========================================================
    public virtual void Act(UnitBase target) { }

    // ==========================================================
    // 🔹 ПОЛУЧЕНИЕ УРОНА
    // ==========================================================
    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= Mathf.Max(0f, amount);
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    // ==========================================================
    // 🔹 СМЕРТЬ
    // ==========================================================
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animController != null)
            StartCoroutine(animController.PlayDeath());

        if (CurrentCell != null)
        {
            CurrentCell.SetOccupied(false);
            CurrentCell = null;
        }
    }

    // ==========================================================
    // 🔹 ПЛАВНОЕ ДВИЖЕНИЕ К ЦЕЛИ
    // ==========================================================
    public IEnumerator MoveToPoint(Vector3 targetWorldPos, float stopDistance, float duration)
    {
        Vector3 from = transform.position;
        Vector3 dir = (targetWorldPos - from);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            yield break;

        dir.Normalize();
        Vector3 to = targetWorldPos - dir * Mathf.Max(0.0f, stopDistance);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            transform.position = Vector3.Lerp(from, to, k);
            yield return null;
        }
        transform.position = to;
    }

    // ==========================================================
    // 🔹 ВОЗВРАТ НА СТАРТОВУЮ ПОЗИЦИЮ
    // ==========================================================
    public IEnumerator ReturnToStart(float duration)
    {
        Vector3 from = transform.position;
        Vector3 to = startPosition;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            transform.position = Vector3.Lerp(from, to, k);
            yield return null;
        }
        transform.position = to;

        if (!isDead && animController != null)
            animController.PlayIdle();
    }
}
