using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Карточка персонажа")]
    [SerializeField] private Image classIcon;
    [SerializeField] private Text classNameText;
    [SerializeField] private Text classDescriptionText;

    [Header("Кнопки управления")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button selectButton;

    [Header("Доступные классы")]
    public ClassData[] availableClasses;

    private int currentIndex = 0;

    private void Start()
    {
        if (availableClasses == null || availableClasses.Length == 0)
        {
            Debug.LogError("❌ В CharacterSelectionUI не заданы доступные классы!");
            return;
        }

        // Подписываем кнопки
        if (prevButton != null) prevButton.onClick.AddListener(ShowPreviousClass);
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextClass);
        if (selectButton != null) selectButton.onClick.AddListener(SelectCurrentClass);

        UpdateClassCard();
    }

    private void UpdateClassCard()
    {
        if (availableClasses == null || availableClasses.Length == 0) return;

        ClassData cls = availableClasses[currentIndex];

        if (classIcon != null) classIcon.sprite = cls.classIcon;
        if (classNameText != null) classNameText.text = cls.className;
        if (classDescriptionText != null) classDescriptionText.text = cls.description;

        Debug.Log($"📜 Отображается класс: {cls.className} ({currentIndex + 1}/{availableClasses.Length})");
    }

    private void ShowNextClass()
    {
        if (availableClasses == null || availableClasses.Length == 0) return;

        currentIndex++;
        if (currentIndex >= availableClasses.Length)
            currentIndex = 0; // зацикливаем

        UpdateClassCard();
    }

    private void ShowPreviousClass()
    {
        if (availableClasses == null || availableClasses.Length == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = availableClasses.Length - 1; // зацикливаем

        UpdateClassCard();
    }

    private void SelectCurrentClass()
    {
        if (availableClasses == null || availableClasses.Length == 0) return;

        ClassData cls = availableClasses[currentIndex];
        Debug.Log($"✅ Выбран класс: {cls.className}");
        GameManager.Instance.SelectCharacter(cls);
    }
}
