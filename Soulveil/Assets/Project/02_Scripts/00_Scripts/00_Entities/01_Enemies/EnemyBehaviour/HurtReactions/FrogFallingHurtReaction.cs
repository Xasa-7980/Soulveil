using System.Collections;
using UnityEngine;

public class FrogFallingHurtReaction : EnemyHurtReaction
{
    [Header("Falling Attack Reaction")]
    [SerializeField] private float verticalBounceForce = 12f;
    [SerializeField] private float horizontalBounceSpeed = 6f;
    [SerializeField] private float horizontalBounceDuration = 0.25f;

    [Header("Falling Attack VFX")]
    [SerializeField] private GameObject fallingAttackVFX;

    private Coroutine launchRoutine;

    protected override void HandleOnDamaged ( object sender, DamageInfo damageInfo )
    {
        base.HandleOnDamaged(sender, damageInfo);

        if (!damageInfo.HasAnyDamageType(DamageType.FallingAttack)) return;
        if (damageInfo.attacker == null) return;

        PlayerMovement playerMovement = damageInfo.attacker.GetComponent<PlayerMovement>();
        CharacterController characterController = damageInfo.attacker.GetComponent<CharacterController>();

        if (playerMovement == null) return;
        if (characterController == null) return;

        Vector3 launchDirection = damageInfo.attacker.transform.position - transform.position;
        launchDirection.y = 0f;

        if (launchDirection.sqrMagnitude <= 0.001f) launchDirection = transform.forward;

        launchDirection.Normalize();

        playerMovement.ApplyUpwardVelocity(verticalBounceForce);

        if (launchRoutine != null) StopCoroutine(launchRoutine);
        launchRoutine = StartCoroutine(LaunchHorizontal(characterController, launchDirection));

        PlayFallingAttackVFX(damageInfo);

        Debug.Log($"{gameObject.name} reaccionó a FallingAttack | Horizontal: {horizontalBounceSpeed} | Vertical: {verticalBounceForce}");
    }

    private IEnumerator LaunchHorizontal ( CharacterController characterController, Vector3 direction )
    {
        float timer = 0f;

        while (timer < horizontalBounceDuration)
        {
            if (characterController == null) yield break;

            characterController.Move(direction * horizontalBounceSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        launchRoutine = null;
    }

    private void PlayFallingAttackVFX ( DamageInfo damageInfo )
    {
        if (fallingAttackVFX == null) return;

        Instantiate(fallingAttackVFX, damageInfo.hitPoint, Quaternion.identity);
    }

    protected override void OnDestroy ( )
    {
        if (launchRoutine != null) StopCoroutine(launchRoutine);

        base.OnDestroy();
    }
}