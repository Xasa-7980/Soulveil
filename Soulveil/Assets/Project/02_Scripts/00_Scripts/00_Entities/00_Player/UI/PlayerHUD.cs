using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Player Info")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider levelSlider;

    [Header("Stats")]
    [SerializeField] private TMP_Text healthValueText;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private TMP_Text attackValueText;
    [SerializeField] private Slider attackSlider;

    [SerializeField] private TMP_Text defenseValueText;
    [SerializeField] private Slider defenseSlider;

    [SerializeField] private TMP_Text criticalChanceValueText;
    [SerializeField] private Slider criticalChanceSlider;

    [SerializeField] private TMP_Text criticalDamageValueText;
    [SerializeField] private Slider criticalDamageSlider;

    [Header("Equipment")]
    [SerializeField] private EquipmentSlotUI weaponSlot;
    [SerializeField] private EquipmentSlotUI armorSlot;
    [SerializeField] private EquipmentSlotUI artifactSlot;

    private PlayerStats playerStats;
    private PlayerHealth playerHealth;
    private bool inventoryOpen;

    private void Awake ( )
    {
        playerStats = GetComponentInParent<PlayerStats>();
        playerHealth = GetComponentInParent<PlayerHealth>();
    }

    private void Start ( )
    {
        SetInventoryOpen(false);
    }

    public void OnInventory ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;

        SetInventoryOpen(!inventoryOpen);
    }

    private void SetInventoryOpen ( bool open )
    {
        inventoryOpen = open;

        if (inventoryPanel != null) inventoryPanel.SetActive(open);

        if (open) RefreshHUD();
    }

    public void RefreshHUD ( )
    {
        RefreshHealth();
        RefreshStats();
    }

    private void RefreshHealth ( )
    {
        if (playerHealth == null) return;

        healthValueText.text = $"{Mathf.RoundToInt(playerHealth.CurrentHealth)} / {Mathf.RoundToInt(playerHealth.MaxHealth)}";

        healthSlider.minValue = 0f;
        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = playerHealth.CurrentHealth;
    }

    private void RefreshStats ( )
    {
        if (playerStats == null) return;

        SetAttack();
        SetDefense();
        SetCriticalChance();
        SetCriticalDamage();
    }

    private void SetAttack ( )
    {
        float value = playerStats.AttackDamage;

        attackValueText.text = Mathf.RoundToInt(value).ToString();

        attackSlider.minValue = 0f;
        attackSlider.maxValue = 100f;
        attackSlider.value = value;
    }

    private void SetDefense ( )
    {
        float value = playerStats.Defense;

        defenseValueText.text = Mathf.RoundToInt(value).ToString();

        defenseSlider.minValue = 0f;
        defenseSlider.maxValue = 100f;
        defenseSlider.value = value;
    }

    private void SetCriticalChance ( )
    {
        float value = playerStats.CriticalChance;

        criticalChanceValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";

        criticalChanceSlider.minValue = 0f;
        criticalChanceSlider.maxValue = 1f;
        criticalChanceSlider.value = value;
    }

    private void SetCriticalDamage ( )
    {
        float value = playerStats.CriticalDamage;

        criticalDamageValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";

        criticalDamageSlider.minValue = 1f;
        criticalDamageSlider.maxValue = 3f;
        criticalDamageSlider.value = value;
    }

    public void SetPlayerName ( string playerName )
    {
        playerNameText.text = playerName;
    }

    public void SetLevel ( int level, float currentExperience, float requiredExperience )
    {
        levelText.text = level.ToString();

        levelSlider.minValue = 0f;
        levelSlider.maxValue = requiredExperience;
        levelSlider.value = currentExperience;
    }

    public void SetWeapon ( ItemInstance itemInstance )
    {
        if (weaponSlot == null) return;

        weaponSlot.SetItem(itemInstance);
    }

    public void SetArmor ( ItemInstance itemInstance )
    {
        if (armorSlot == null) return;

        armorSlot.SetItem(itemInstance);
    }

    public void SetArtifact ( ItemInstance itemInstance )
    {
        if (artifactSlot == null) return;

        artifactSlot.SetItem(itemInstance);
    }

    public void ClearWeapon ( )
    {
        if (weaponSlot == null) return;

        weaponSlot.Clear();
    }

    public void ClearArmor ( )
    {
        if (armorSlot == null) return;

        armorSlot.Clear();
    }

    public void ClearArtifact ( )
    {
        if (artifactSlot == null) return;

        artifactSlot.Clear();
    }
}