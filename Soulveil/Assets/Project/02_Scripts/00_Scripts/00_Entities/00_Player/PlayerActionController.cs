using System.Collections.Generic;
using UnityEngine;
[System.Flags]
public enum PlayerActionBlock
{
    None = 0,
    Movement = 1 << 0,
    Combat = 1 << 1, 
    Dodge = 1 << 2, 
    Skills = 1 << 3,
    Interaction = 1 << 4,
    FallAttack = 1 << 5,
    All = Movement | Combat | Dodge | Skills | Interaction | FallAttack
}

public class PlayerActionController : MonoBehaviour
{
    private readonly Dictionary<object, PlayerActionBlock> blocks = new();

    public bool CanMove => !IsBlocked(PlayerActionBlock.Movement);
    public bool CanCombat => !IsBlocked(PlayerActionBlock.Combat);
    public bool CanDodge => !IsBlocked(PlayerActionBlock.Dodge);
    public bool CanUseSkills => !IsBlocked(PlayerActionBlock.Skills);
    public bool CanInteract => !IsBlocked(PlayerActionBlock.Interaction);
    public bool CanPound => !IsBlocked(PlayerActionBlock.FallAttack);

    public PlayerActionBlock BlockedActions
    {
        get
        {
            PlayerActionBlock result = PlayerActionBlock.None;

            foreach (PlayerActionBlock block in blocks.Values)
            {
                result |= block;
            }

            return result;
        }
    }

    public bool IsBlocked ( PlayerActionBlock action )
    {
        // Básicamente miramos si la acción está bloqueada.
        // Cuando el resultado de la operación AND de bits es diferente de cero,
        // significa que blockedActions y action comparten al menos un bit,
        // por lo tanto esa acción está bloqueada.
        //
        // OR no sirve para comprobar si una acción está bloqueada,
        // porque añade/combina los bits de ambos operandos y el resultado
        // será distinto de cero aunque la acción consultada no estuviera bloqueada.
        /* EJEMPLO: 
            blockedActions = Movement | Dodge;

            (blockedActions & Combat) != 0;   // false
            (blockedActions & Movement) != 0; // true
         */
        return (BlockedActions & action) != 0;
    }
    public bool AreAllBlocked ( PlayerActionBlock actions )
    {
        return (BlockedActions & actions) == actions;
    }

    public void Block ( object owner, PlayerActionBlock actions )
    {
        if (owner == null) return;
        if (actions == PlayerActionBlock.None) return;

        if (blocks.TryGetValue(owner, out PlayerActionBlock current))
        {
            blocks[owner] = current | actions;
            return;
        }

        blocks.Add(owner, actions);
    }

    public void Unblock ( object owner )
    {
        if (owner == null) return;

        blocks.Remove(owner);
    }

    public void Unblock ( object owner, PlayerActionBlock actions )
    {
        if (owner == null) return;
        if (!blocks.TryGetValue(owner, out PlayerActionBlock current)) return;

        current &= ~actions;

        if (current == PlayerActionBlock.None)
        {
            blocks.Remove(owner);
            return;
        }

        blocks[owner] = current;
    }

    public bool HasBlock ( object owner )
    {
        if (owner == null) return false;

        return blocks.ContainsKey(owner);
    }

    public PlayerActionBlock GetBlock ( object owner )
    {
        if (owner == null) return PlayerActionBlock.None;

        return blocks.TryGetValue(owner, out PlayerActionBlock block)
            ? block
            : PlayerActionBlock.None;
    }

    public void ClearAll ( )
    {
        blocks.Clear();
    }
}