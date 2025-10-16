using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Основные компоненты")]
    public GridManager gridManager;
    public BattleSystem battleSystem;
    public TurnManager turnManager;
    public UIManager uiManager;

    [Header("Префабы врагов")]
    public EnemyUnit[] enemyPrefabs;

    [Header("Игрок")]
    private PlayerUnit player;
    private ClassData selectedClass;

    [Header("Текущее состояние боя")]
    private EnemyUnit currentEnemy;
    private List<EnemyUnit> availableEnemies = new List<EnemyUnit>();
    private int victories = 0;
    private bool battleActive = false;
    private Coroutine battleLoopRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ==========================================================
    // 🔹 ВЫБОР КЛАССА И ЗАПУСК
    // ==========================================================
    public void SelectCharacter(ClassData cls)
    {
        selectedClass = cls;
        Debug.Log($"✅ GameManager получил выбранный класс: {cls.className}");
        SetupScene();
    }

    // ==========================================================
    // 🔹 НАСТРОЙКА СЦЕНЫ
    // ==========================================================
    private void SetupScene()
    {
        if (gridManager != null) gridManager.GenerateGrid();

        // --- создаём игрока ---
        if (player == null && selectedClass != null && selectedClass.playerPrefab != null)
        {
            Cell playerCell = gridManager.GetRandomFreeCell();
            GameObject playerObj = Instantiate(selectedClass.playerPrefab, playerCell.transform.position, Quaternion.identity);

            player = playerObj.GetComponent<PlayerUnit>();
            player.classData = selectedClass;
            player.unitName = selectedClass.className;
            player.Initialize(battleSystem, turnManager);
            player.BindToCell(playerCell);
        }
        else if (player == null)
        {
            Debug.LogError("❌ Не выбран класс или отсутствует префаб игрока!");
            return;
        }

        // --- первичное заполнение пула врагов ---
        RefillEnemyPool();

        // --- первый враг ---
        SpawnNextEnemy();
    }

    // ==========================================================
    // ♻️ Пополнение пула врагов (для разнообразия, без влияния на победу)
    // ==========================================================
    private void RefillEnemyPool()
    {
        availableEnemies.Clear();
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
            availableEnemies.AddRange(enemyPrefabs);
    }

    // ==========================================================
    // 🔹 СПАВН СЛЕДУЮЩЕГО ВРАГА
    // ==========================================================
    private void SpawnNextEnemy()
    {
        // стопим старую корутину, если вдруг идёт
        if (battleLoopRoutine != null)
        {
            StopCoroutine(battleLoopRoutine);
            battleLoopRoutine = null;
        }

        // отписка от логов, чтобы не было дублей
        battleSystem.OnBattleLog -= uiManager.AppendLog;

        // единственное условие финальной победы — 5 побед
        if (victories >= 5)
        {
            uiManager.AppendLog("🎉 Победа! Все 5 врагов побеждены! Игра завершена!");
            battleActive = false;
            uiManager.ShowEndGameMessage("Победа!");
            return;
        }

        // если пул пустой, просто перезаполняем (а НЕ завершаем игру)
        if (availableEnemies.Count == 0)
            RefillEnemyPool();

        // чистим предыдущего врага
        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }

        // выбираем случайный префаб
        int randIndex = Random.Range(0, availableEnemies.Count);
        EnemyUnit randomEnemyPrefab = availableEnemies[randIndex];

        // спавним
        Cell enemyCell = gridManager.GetRandomFreeCell();
        currentEnemy = Instantiate(randomEnemyPrefab, enemyCell.transform.position, Quaternion.identity);
        currentEnemy.Initialize(battleSystem, turnManager);
        currentEnemy.BindToCell(enemyCell);

        // убираем из пула, чтобы была ротация типов; пул сам рефилится при опустошении
        availableEnemies.RemoveAt(randIndex);

        // очередь ходов
        var units = new List<UnitBase> { player, currentEnemy };
        turnManager.Initialize(units);

        // UI
        uiManager.BindUnits(player, currentEnemy);
        uiManager.AppendLog($"⚔️ Бой начался: {player.unitName} против {currentEnemy.unitName}");

        // подписываем логи (однократно на бой)
        battleSystem.OnBattleLog += uiManager.AppendLog;

        // старт боя
        battleActive = true;
        battleLoopRoutine = StartCoroutine(BattleLoop());
    }

    // ==========================================================
    // 🔹 ОСНОВНОЙ ЦИКЛ БОЯ
    // ==========================================================
    private IEnumerator BattleLoop()
    {
        while (battleActive)
        {
            yield return new WaitForSeconds(1f);

            UnitBase current = turnManager.CurrentUnit;
            if (current == null || current.isDead)
            {
                turnManager.EndTurn();
                continue;
            }

            // ход (ожидаем корутину с анимацией и паузами)
            if (current is PlayerUnit)
                yield return battleSystem.ProcessAttackRoutine(player, currentEnemy);
            else if (current is EnemyUnit)
                yield return battleSystem.ProcessAttackRoutine(currentEnemy, player);

            uiManager.UpdateBattleUI(player, currentEnemy);

            // проигрыш
            if (player.isDead)
            {
                uiManager.AppendLog($"☠️ {player.unitName} погиб! Игра окончена!");
                AudioSourceScanner audioSourceScanner = FindFirstObjectByType<AudioSourceScanner>();
                audioSourceScanner.musicGameOver = true;
                audioSourceScanner.GameOverMusic();
                battleActive = false;
                uiManager.ShowEndGameMessage("Вы проиграли!");
                UIManager uIManager = GetComponent<UIManager>();
                uiManager.pauseButton.interactable = false;
                battleLoopRoutine = null;
                yield break;
            }

            // победа в бою
            if (currentEnemy.isDead)
            {
                victories++;
                uiManager.AppendLog($"🏆 Победа #{victories}! {currentEnemy.unitName} повержен!"); 
                player.LevelUp();

                // дроп
                if (currentEnemy.enemyData != null && currentEnemy.enemyData.weapon != null)
                {
                    var droppedWeapon = currentEnemy.enemyData.weapon;
                    battleSystem.OfferWeaponChange(player, droppedWeapon, uiManager);
                }

                // ждём выбор оружия
                yield return new WaitUntil(() => battleSystem.weaponChoiceMade);
                yield return new WaitForSeconds(1.0f);

                // финальная победа — только по 5 победам
                if (victories >= 5)
                {
                    uiManager.ShowEndGameMessage("Победа!");

                    AudioSourceScanner audioSourceScanner = FindFirstObjectByType<AudioSourceScanner>();
                    audioSourceScanner.musicGameOver = false;
                    audioSourceScanner.GameOverMusic();

                    UIManager uIManager = GetComponent<UIManager>();
                    uiManager.pauseButton.interactable = false;
                    battleActive = false;
                    battleLoopRoutine = null;
                    yield break;
                }

                // следующий бой
                SpawnNextEnemy();
                battleLoopRoutine = null;
                yield break;
            }

            // следующий ход
            turnManager.EndTurn();
        }

        battleLoopRoutine = null;
    }
}
