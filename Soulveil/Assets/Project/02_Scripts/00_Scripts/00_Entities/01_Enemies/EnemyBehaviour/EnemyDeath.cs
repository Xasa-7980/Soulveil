using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    [Header("Loot")]
    [SerializeField] private ItemData[] possibleLoot;
    [SerializeField] private Transform lootSpawnPoint;
    [SerializeField, Range(0f, 1f)] private float dropChance = 1f;

    [Header("Loot Level")]
    [SerializeField] private int minItemLevel = 1;
    [SerializeField] private int maxItemLevel = 1;

    private EnemyController controller;

    private void Awake ( )
    {
        controller = GetComponent<EnemyController>();
    }

    private void OnEnable ( )
    {
        controller.ClearTarget();

        if (controller.Agent != null && controller.Agent.isOnNavMesh)
        {
            controller.Agent.isStopped = true;
            controller.Agent.ResetPath();
        }

        DropLoot();
        //Animacion de muerte, etc...
    }

    private void DropLoot ( )
    {
        if (possibleLoot == null || possibleLoot.Length == 0) return;
        if (Random.value > dropChance) return;

        ItemData itemData = possibleLoot[Random.Range(0, possibleLoot.Length)];

        if (itemData == null) return;
        if (itemData.WorldPrefab == null) return;

        int level = Random.Range(minItemLevel, maxItemLevel + 1);
        ItemInstance itemInstance = new ItemInstance(itemData, level, itemData.Rarity);

        Vector3 spawnPosition = lootSpawnPoint != null ? lootSpawnPoint.position : transform.position;

        GameObject lootObject = Instantiate(itemData.WorldPrefab, spawnPosition, Quaternion.identity);

        LootItem lootItem = lootObject.GetComponent<LootItem>();

        if (lootItem != null) lootItem.Initialize(itemInstance);
    }
}