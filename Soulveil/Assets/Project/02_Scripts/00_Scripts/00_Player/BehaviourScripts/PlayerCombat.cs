using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private enum AttackType { None, Light, Heavy }

    [Header("Combo")]
    [SerializeField] private float comboResetDelay = 1f;
    [SerializeField, Range(0f, 1f)] private float normTime = 0.75f;

    [Header("Combat State")]
    [SerializeField] private float inCombatCountdown = 4f;

    [Header("Debug")]
    [SerializeField] private bool debugCombat = true;
    [SerializeField] private bool debugEveryFrame = false;

    private PlayerSpeacialist playerSpecialist;
    private PlayerAnimationController animationController;

    private int lightAttackIndex = 1;
    private int heavyAttackIndex = 1;

    private float comboResetTimer;
    private float combatTimer;

    private bool attackLocked;
    private bool attackStateEntered;
    private bool inCombat;
    private bool normTimeReached;

    private AttackType currentAttack;
    private AttackType bufferedAttack;

    public bool InCombat => inCombat;
    public bool IsAttacking => attackLocked;

    private void Awake ( )
    {
        playerSpecialist = GetComponent<PlayerSpeacialist>();
        animationController = GetComponent<PlayerAnimationController>();

        PlayerCombat[] combats = FindObjectsByType<PlayerCombat>(
            FindObjectsSortMode.None
        );

        Debug.Log($"PLAYER COMBAT INSTANCES: {combats.Length}");

        foreach (PlayerCombat combat in combats)
        {
            Debug.Log(
                $"Combat encontrado: {combat.gameObject.name} | ID:{combat.GetInstanceID()}"
            );
        }

        Log("AWAKE");
    }
    private void Update ( )
    {
        UpdateAttack();
        UpdateComboReset();
        UpdateCombatTimer();

        if (debugEveryFrame)
        {
            Log(
                $"FRAME | Locked:{attackLocked} | Entered:{attackStateEntered} | " +
                $"Current:{currentAttack} | Buffer:{bufferedAttack} | " +
                $"L:{lightAttackIndex} H:{heavyAttackIndex} | " +
                $"AttackState:{animationController.IsInAttackState()} | " +
                $"Transition:{animationController.IsInAttackTransition()} | " +
                $"Norm:{animationController.GetCombatNormalizedTime():0.00}"
            );
        }
    }

    public void OnLightAttack ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;


        Debug.Log(
            $"LIGHT INPUT | Object:{gameObject.name} | " +
            $"ID:{GetInstanceID()} | " +
            $"Frame:{Time.frameCount}"
        );
        if (!attackLocked)
        {
            Log("LIGHT -> inicia cadena");
            StartAttack(AttackType.Light);
        }
        else
        {
            bufferedAttack = AttackType.Light;
            Log("LIGHT -> guardado en buffer");
        }
    }

    public void OnHeavyAttack ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;

        Debug.Log(
       $"HEAVY INPUT | Object:{gameObject.name} | " +
       $"ID:{GetInstanceID()} | " +
       $"Frame:{Time.frameCount}"
   );
        if (!attackLocked)
        {
            Log("HEAVY -> inicia cadena");
            StartAttack(AttackType.Heavy);
        }
        else
        {
            bufferedAttack = AttackType.Heavy;
            Log("HEAVY -> guardado en buffer");
        }
    }

    private void StartAttack ( AttackType type )
    {
        if (GetAttackLength(type) <= 0)
        {
            Log($"START CANCELADO | {type} no tiene ataques");
            return;
        }

        bool startingNewChain = !attackLocked;

        attackLocked = true;

        if (startingNewChain)
            attackStateEntered = false;

        normTimeReached = false;
        currentAttack = type;
        bufferedAttack = AttackType.None;
        comboResetTimer = 0f;

        EnterCombat();

        Log($"START {type} {GetAttackIndex(type)} | NuevaCadena:{startingNewChain}");

        PlayAttack(type);
    }
    private void UpdateAttack ( )
    {
        if (!attackLocked)
            return;

        if (animationController.IsInAttackTransition())
            return;

        bool inAttackState = animationController.IsInAttackState();

        // Acabamos de disparar el primer trigger y todavía
        // no hemos entrado físicamente al state.
        if (!inAttackState && !attackStateEntered)
            return;

        // Ya estuvimos atacando y ahora hemos vuelto a Idle.
        if (!inAttackState && attackStateEntered)
        {
            Log($"FIN REAL | {currentAttack} {GetAttackIndex(currentAttack)}");

            FinishAttack();
            return;
        }

        bool isCurrentAttack = animationController.IsCurrentAttack(
            currentAttack == AttackType.Light,
            GetAttackIndex(currentAttack)
        );

        if (!isCurrentAttack)
            return;

        if (!attackStateEntered)
        {
            attackStateEntered = true;

            Log(
                $"ENTRÓ AL STATE | {currentAttack} " +
                $"{GetAttackIndex(currentAttack)}"
            );
        }

        float normalizedTime =
            animationController.GetCombatNormalizedTime();

        if (normalizedTime < normTime)
            return;

        if (!normTimeReached)
        {
            normTimeReached = true;

            Log(
                $"VENTANA COMBO | {currentAttack} " +
                $"{GetAttackIndex(currentAttack)} | " +
                $"Buffer:{bufferedAttack}"
            );
        }

        if (bufferedAttack == AttackType.None)
            return;

        AttackType nextAttack = bufferedAttack;

        Log($"CONSUME BUFFER | {currentAttack} -> {nextAttack}");

        AdvanceCombo(nextAttack);
        StartAttack(nextAttack);
    }
    private void PlayAttack ( AttackType type )
    {
        int index = GetAttackIndex(type);

        Log($"PLAY ATTACK | {type} {index}");

        if (type == AttackType.Light)
            animationController.PlayLightAttack(index);
        else
            animationController.PlayHeavyAttack(index);
    }

    private void AdvanceCombo ( AttackType type )
    {
        int previousLight = lightAttackIndex;
        int previousHeavy = heavyAttackIndex;

        if (type == AttackType.Light)
        {
            lightAttackIndex++;

            if (lightAttackIndex > playerSpecialist.lightAttackLength)
                lightAttackIndex = 1;

            heavyAttackIndex = 1;
        }
        else
        {
            heavyAttackIndex++;

            if (heavyAttackIndex > playerSpecialist.heavyAttackLength)
                heavyAttackIndex = 1;

            lightAttackIndex = 1;
        }

        Log(
            $"ADVANCE COMBO {type} | " +
            $"Light:{previousLight}->{lightAttackIndex} | " +
            $"Heavy:{previousHeavy}->{heavyAttackIndex}"
        );
    }

    private int GetAttackIndex ( AttackType type )
    {
        return type == AttackType.Light
            ? lightAttackIndex
            : heavyAttackIndex;
    }

    private int GetAttackLength ( AttackType type )
    {
        return type == AttackType.Light
            ? playerSpecialist.lightAttackLength
            : playerSpecialist.heavyAttackLength;
    }

    private void FinishAttack ( )
    {
        Log(
            $"FINISH ATTACK | {currentAttack} | " +
            $"L:{lightAttackIndex} H:{heavyAttackIndex}"
        );

        attackLocked = false;
        attackStateEntered = false;
        normTimeReached = false;

        currentAttack = AttackType.None;
        bufferedAttack = AttackType.None;

        comboResetTimer = comboResetDelay;

        // Desde ahora empiezan a contar los segundos
        // para salir de combate.
        combatTimer = inCombatCountdown;

        Log("ATTACK UNLOCKED");
    }
    private void UpdateComboReset ( )
    {
        if (attackLocked || comboResetTimer <= 0f)
            return;

        comboResetTimer -= Time.deltaTime;

        if (debugEveryFrame)
            Log($"ComboResetTimer:{comboResetTimer:0.00}");

        if (comboResetTimer <= 0f)
        {
            Log("COMBO RESET TIMER TERMINADO");
            ResetCombo();
        }
    }

    private void ResetCombo ( )
    {
        Log($"RESET COMBO | L:{lightAttackIndex}->1 H:{heavyAttackIndex}->1");

        lightAttackIndex = 1;
        heavyAttackIndex = 1;

        currentAttack = AttackType.None;
        bufferedAttack = AttackType.None;

        comboResetTimer = 0f;
    }

    private void EnterCombat ( )
    {
        bool wasInCombat = inCombat;

        inCombat = true;
        combatTimer = inCombatCountdown;

        if (!wasInCombat)
            Log("ENTER COMBAT");
        else
            Log("COMBAT TIMER REFRESH");
    }

    private void UpdateCombatTimer ( )
    {
        if (!inCombat)
            return;

        // Mientras estamos atacando, no podemos salir de combate.
        if (attackLocked)
            return;

        combatTimer -= Time.deltaTime;

        if (debugEveryFrame)
            Log($"CombatTimer:{combatTimer:0.00}");

        if (combatTimer > 0f)
            return;

        Log("EXIT COMBAT");

        inCombat = false;
        combatTimer = 0f;

        ResetCombo();
    }
    private void Log ( string message )
    {
        if (!debugCombat)
            return;

        Debug.Log(
            $"[PLAYER COMBAT | {gameObject.name} | ID:{GetInstanceID()} | Frame:{Time.frameCount}] {message}"
        );
    }
}