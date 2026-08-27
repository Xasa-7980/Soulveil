using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSpecialist",
    menuName = "Soulveil/SpecialistCard/Specialist Data"
)]
public class SpecialistHandler : ScriptableObject
{
    [Header("Visual")]
    public GameObject weaponPrefab;

    [Header("Combat")]
    [Min(0)]
    public int lightAttackLength = 1;

    [Min(0)]
    public int heavyAttackLength = 1;

    [Header("Animation")]
    public AnimatorOverrideController animatorOverrideController;
}