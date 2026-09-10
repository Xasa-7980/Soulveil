using UnityEngine;

public class WaterWormHurtReaction : EnemyHurtReaction
{
    [Header("Element Reaction")]
    [SerializeField] private GameObject waterFireReactionVFX;

    [Header("Spikes")]
    [SerializeField] private GameObject spikesObject;

    [Header("Stun"), Range(0,100f)]
    [SerializeField] private float stunDuration = 3;

    private EnemyController enemyController;

    protected override void Awake ( )
    {
        base.Awake();

        enemyController = GetComponent<EnemyController>();
    }

    protected override void HandleOnDamaged ( object sender, DamageInfo damageInfo )
    {
        base.HandleOnDamaged(sender, damageInfo);

        // Aquí pueden ir futuras reacciones propias del Water Worm
        // relacionadas directamente con DamageType, HitZone, etc.
        Debug.Log($"{damageInfo.element} | {damageInfo.damageType} | {damageInfo.hitZone} | {damageInfo.damage}");
    }

    protected override void HandleElementReaction ( object sender, ElementReactionType reaction )
    {
        base.HandleElementReaction(sender, reaction);

        if (reaction != ElementReactionType.Water_Fire) return;

        DisableSpikes();
        EnterStunnedState();

        Debug.Log($"{gameObject.name} reaccionó a " + reaction.ToString());
    }

    private void DisableSpikes ( )
    {
        if (spikesObject == null) return;

        spikesObject.SetActive(false);
    }

    private void EnterStunnedState ( )
    {
        if (enemyController == null) return;
        if (enemyController.StunnedState == null) return;
        enemyController.StunnedState.SetStunDuration(stunDuration);
        enemyController.ChangeState(enemyController.StunnedState);
    }
}