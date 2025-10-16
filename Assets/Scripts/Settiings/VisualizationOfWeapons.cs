using UnityEngine;

public class VisualizationOfWeapons : MonoBehaviour
{
    [Header("Виды оружий")]
    [SerializeField] GameObject[] prefabs;
    [Header("Текущее оружие")]
    [SerializeField] GameObject currentPrefab;
    [SerializeField] string nameWeapon;
    [SerializeField] string nameModification;
    [Header("Место крепления оружия")]
    [SerializeField] Transform pointSpawn;
    [Header("Ссылка на Игрока")]
    [SerializeField] PlayerUnit playerUnit;
    bool find = false;



    private void Start()
    {
        if(GetComponent<PlayerUnit>() != null && pointSpawn != null && prefabs != null) { find = true; }
        if(find)
        {
            playerUnit = GetComponent<PlayerUnit>();
            nameModification = nameWeapon;
            NameWeapon();
        } 
        else if(!find) { Debug.Log("скрипт PlayerUnit или точка крепления или префабы не назначены"); }
    }

    private void Update()
    {
        if (find)
            NameWeapon();
    }
    void NameWeapon()
    {
        nameWeapon = playerUnit.currentWeapon.name;
        if (nameModification != nameWeapon)
        {
            nameModification = nameWeapon;
            SpawnWeapon();
        }
    }
    public void SpawnWeapon()
    {
        DeleteWeapon();
        if (prefabs != null)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i].name == nameModification && pointSpawn != null)
                {
                    currentPrefab = Instantiate(prefabs[i], pointSpawn);
                    currentPrefab.transform.localPosition = Vector3.zero;
                    currentPrefab.transform.localRotation = Quaternion.identity;
                    currentPrefab.transform.localScale = prefabs[i].transform.localScale;
                    break;
                }
            }
        }
        if(currentPrefab == null)
        {
            Debug.Log("Орудий с нужным именем нет!");
        }
    }
    void DeleteWeapon()
    {
        if (currentPrefab != null)
        {
            Destroy(currentPrefab);
            currentPrefab = null;
        }
    }
}
