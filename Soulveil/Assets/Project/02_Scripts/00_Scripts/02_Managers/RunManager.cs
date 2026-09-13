using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("Run")]
    [SerializeField] private bool runActive;

    private readonly List<InventorySlot> securedLoot = new();

    public bool RunActive => runActive;
    public IReadOnlyList<InventorySlot> SecuredLoot => securedLoot;

    private void Awake ( )
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void StartRun ( )
    {
        runActive = true;
        securedLoot.Clear();

        Debug.Log("[RunManager] Run iniciada.");
    }

    public void SecureLoot ( PlayerInventory inventory )
    {
        if (!runActive) return;
        if (inventory == null) return;

        securedLoot.Clear();

        foreach (InventorySlot slot in inventory.Slots)
        {
            if (slot == null || slot.IsEmpty) continue;

            securedLoot.Add(new InventorySlot(slot.ItemInstance, slot.Quantity));
        }

        Debug.Log($"[RunManager] Loot asegurado: {securedLoot.Count} slots.");
    }

    public void CompleteRun ( )
    {
        if (!runActive) return;

        runActive = false;

        Debug.Log("[RunManager] Run completada.");
    }

    public void FailRun ( )
    {
        if (!runActive) return;

        runActive = false;
        securedLoot.Clear();

        Debug.Log("[RunManager] Run fallida. Loot perdido.");
    }
}