using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool IsOccupied { get; private set; } = false;

    [Header("Материалы визуализации")]
    [SerializeField] private Renderer rend;
    [SerializeField] private Material defaultMat;
    [SerializeField] private Material occupiedMat;

    private void Awake()
    {
        if (rend == null) rend = GetComponent<Renderer>();
    }

    public void SetOccupied(bool state)
    {
        IsOccupied = state;
        if (rend != null)
        {
            rend.material = state ? occupiedMat : defaultMat;
        }
    }

    public void SetDefault()
    {
        if (rend != null && defaultMat != null)
            rend.material = defaultMat;
    }
}
