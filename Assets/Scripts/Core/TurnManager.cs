using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    [Header("Очередь ходов (runtime)")]
    public List<UnitBase> turnOrder = new List<UnitBase>();

    private int currentIndex = 0;

    /// <summary>
    /// Текущий активный юнит (возвращает null, если очередь пуста).
    /// </summary>
    public UnitBase CurrentUnit
    {
        get
        {
            if (turnOrder == null || turnOrder.Count == 0) return null;
            if (currentIndex < 0 || currentIndex >= turnOrder.Count) return null;
            return turnOrder[currentIndex];
        }
    }

    /// <summary>
    /// Инициализация очереди: сортировка по ловкости (DEX) по убыванию.
    /// При равенстве первым идёт игрок.
    /// </summary>
    public void Initialize(List<UnitBase> units)
    {
        if (units == null || units.Count == 0)
        {
            turnOrder = new List<UnitBase>();
            currentIndex = 0;
            return;
        }

        // Копируем и сортируем по DEX.
        turnOrder = new List<UnitBase>(units);
        turnOrder.Sort((a, b) =>
        {
            float aDex = (a is PlayerUnit ap) ? ap.TotalDexterity : a.dexterity;
            float bDex = (b is PlayerUnit bp) ? bp.TotalDexterity : b.dexterity;

            // по убыванию
            int cmp = -aDex.CompareTo(bDex);
            if (cmp != 0) return cmp;

            // при равенстве — игрок раньше
            bool aIsPlayer = a is PlayerUnit;
            bool bIsPlayer = b is PlayerUnit;
            if (aIsPlayer == bIsPlayer) return 0;
            return aIsPlayer ? -1 : 1;
        });

        // Старт с первого живого
        currentIndex = 0;
        SkipDeadForward();
    }

    /// <summary>
    /// Завершить ход текущего юнита и перейти к следующему живому.
    /// Вызывается из GameManager.BattleLoop().
    /// </summary>
    public void EndTurn()
    {
        if (turnOrder == null || turnOrder.Count == 0) return;

        // Сдвигаем индекс вперёд
        currentIndex = (currentIndex + 1) % turnOrder.Count;

        // Пропускаем мёртвых (на случай, если юнит умер в свой или чужой ход)
        SkipDeadForward();
    }

    /// <summary>
    /// Удаляет из очереди null и мёртвых юнитов.
    /// </summary>
    public void CleanupDead()
    {
        if (turnOrder == null || turnOrder.Count == 0) return;

        int prevCount = turnOrder.Count;
        turnOrder.RemoveAll(u => u == null || u.isDead);

        if (turnOrder.Count == 0)
        {
            currentIndex = 0;
            return;
        }

        // Нормализуем индекс
        currentIndex %= turnOrder.Count;

        // Убедимся, что стоим на живом
        SkipDeadForward();
    }

    /// <summary>
    /// Вспомогательная: прокрутка вперёд до ближайшего живого юнита.
    /// Защита от бесконечного цикла — ограничение по количеству элементов.
    /// </summary>
    private void SkipDeadForward()
    {
        if (turnOrder == null || turnOrder.Count == 0) return;

        int safety = 0;
        while (safety <= turnOrder.Count)
        {
            var u = CurrentUnit;
            if (u != null && !u.isDead) break;

            currentIndex = (currentIndex + 1) % turnOrder.Count;
            safety++;
        }
    }
}
