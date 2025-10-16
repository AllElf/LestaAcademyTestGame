using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using System;

public class UIManager : MonoBehaviour
{
    [Header("=== Общие панели ===")]
    [SerializeField] private GameObject mainUIPanel;
    [SerializeField] private GameObject pausePanel;

    private bool gamePaused = false;

    [Header("=== Кнопки ===")]
    [SerializeField] public Button pauseButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    [Header("=== Логи боя ===")]
    [SerializeField] private Text battleLogText;
    private readonly StringBuilder logBuffer = new StringBuilder(2048);

    [Header("=== Панель игрока ===")]
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text playerLevelText;
    [SerializeField] private Text playerWeaponText;
    [SerializeField] private Slider playerHPBar;
    [SerializeField] private Text playerStatsText;

    [Header("=== Панель врага ===")]
    [SerializeField] private Text enemyNameText;
    [SerializeField] private Text enemyWeaponText;
    [SerializeField] private Slider enemyHPBar;
    [SerializeField] private Text enemyStatsText;

    private PlayerUnit player;
    private EnemyUnit enemy;

    // ===================== ПАНЕЛЬ ТРОФЕЯ =====================
    [Header("=== Панель трофея (оружие) ===")]
    [SerializeField] private GameObject lootPanel;
    [SerializeField] private Text lootTitleText;
    [SerializeField] private Text currentWeaponText;
    [SerializeField] private Text droppedWeaponText;
    [SerializeField] private Button takeButton;
    [SerializeField] private Button skipButton;

    private Action<bool> lootCallback;

    // ===================== ФИНАЛЬНАЯ ПАНЕЛЬ =====================
    [Header("=== Финальная панель ===")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private Text endGameText;
    [SerializeField] private Button endRestartButton;
    [SerializeField] private Button endExitButton;

    private void Awake()
    {
        if (pauseButton) pauseButton.onClick.AddListener(TogglePause);
        if (restartButton) restartButton.onClick.AddListener(RestartScene);
        if (exitButton) exitButton.onClick.AddListener(ExitGame);

        if (lootPanel) lootPanel.SetActive(false);
        if (endGamePanel) endGamePanel.SetActive(false);

        if (endRestartButton) endRestartButton.onClick.AddListener(RestartScene);
        if (endExitButton) endExitButton.onClick.AddListener(ExitGame);
    }

    private void Update()
    {
        if (player != null && enemy != null)
            UpdateBattleUI(player, enemy);
    }

    // ===================== Глобальный UI =====================
    public void TogglePause()
    {
        gamePaused = !gamePaused;
        Time.timeScale = gamePaused ? 0 : 1;

        if (pausePanel) pausePanel.SetActive(gamePaused);
        if (pauseButton) pauseButton.GetComponentInChildren<Text>().text = gamePaused ? "▶" : "⏸";
    }

    public void RestartScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Debug.Log("⏻ Выход из игры");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ===================== Логи боя =====================
    public void AppendLog(string line)
    {
        if (string.IsNullOrEmpty(line)) return;

        if (logBuffer.Length > 2000) logBuffer.Length = 0;
        logBuffer.AppendLine(line);
        if (battleLogText) battleLogText.text = logBuffer.ToString();
    }

    // ===================== Привязка и обновление панели боя =====================
    public void BindUnits(PlayerUnit playerUnit, EnemyUnit enemyUnit)
    {
        player = playerUnit;
        enemy = enemyUnit;

        if (playerNameText) playerNameText.text = player.unitName;
        if (enemyNameText) enemyNameText.text = enemy.unitName;

        UpdateBattleUI(player, enemy);
    }

    public void UpdateBattleUI(UnitBase playerU, UnitBase enemyU)
    {
        if (playerU == null || enemyU == null) return;

        if (playerHPBar)
        {
            playerHPBar.maxValue = playerU.maxHealth;
            playerHPBar.value = playerU.currentHealth;
        }
        if (enemyHPBar)
        {
            enemyHPBar.maxValue = enemyU.maxHealth;
            enemyHPBar.value = enemyU.currentHealth;
        }

        if (playerStatsText)
            playerStatsText.text = $"Сила: {playerU.strength}\nЛовкость: {playerU.dexterity}\nВыносливость: {playerU.endurance}";
        if (enemyStatsText)
            enemyStatsText.text = $"Сила: {enemyU.strength}\nЛовкость: {enemyU.dexterity}\nВыносливость: {enemyU.endurance}";

        if (playerU is PlayerUnit p && playerLevelText)
            playerLevelText.text = $"Уровень: {p.level}";

        if (playerU is PlayerUnit pl && playerWeaponText)
        {
            if (pl.currentWeapon != null)
                playerWeaponText.text = $"Оружие: {pl.currentWeapon.weaponName} ({pl.currentWeapon.baseDamage} урона)";
            else
                playerWeaponText.text = "Оружие: —";
        }

        if (enemyU is EnemyUnit e && enemyWeaponText)
        {
            if (e.enemyData != null && e.enemyData.weapon != null)
                enemyWeaponText.text = $"Оружие: {e.enemyData.weapon.weaponName} ({e.enemyData.weapon.baseDamage} урона)";
            else
                enemyWeaponText.text = "Оружие: —";
        }
    }

    // ===================== Выбор трофея =====================
    public void ShowWeaponChoice(WeaponData current, WeaponData dropped, Action<bool> onResult)
    {
        lootCallback = onResult;

        if (lootPanel) lootPanel.SetActive(true);
        Time.timeScale = 0f;

        if (lootTitleText)
            lootTitleText.text = $"Трофей: {dropped.weaponName}";

        if (currentWeaponText)
        {
            if (current != null)
                currentWeaponText.text = $"Текущее оружие: {current.weaponName}\nУрон: {current.baseDamage}";
            else
                currentWeaponText.text = $"Текущее оружие: (нет)\nУрон: —";
        }

        if (droppedWeaponText)
            droppedWeaponText.text = $"Выпало: {dropped.weaponName}\nУрон: {dropped.baseDamage}";

        if (takeButton)
        {
            takeButton.onClick.RemoveAllListeners();
            takeButton.onClick.AddListener(() =>
            {
                lootPanel.SetActive(false);
                Time.timeScale = 1f;
                lootCallback?.Invoke(true);
                lootCallback = null;
            });
        }

        if (skipButton)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() =>
            {
                lootPanel.SetActive(false);
                Time.timeScale = 1f;
                lootCallback?.Invoke(false);
                lootCallback = null;
            });
        }
    }

    // ===================== Финальная панель =====================
    public void ShowEndGameMessage(string message)
    {
        if (endGamePanel)
        {
            endGamePanel.SetActive(true);
            Time.timeScale = 0f; // полная пауза игры
        }

        if (endGameText)
            endGameText.text = message;
    }
}
