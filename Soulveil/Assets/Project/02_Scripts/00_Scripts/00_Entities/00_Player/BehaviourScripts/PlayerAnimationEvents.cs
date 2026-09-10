using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerSkillController playerSkillController;

    private void Awake ( )
    {
        playerSkillController = GetComponentInParent<PlayerSkillController>();
    }

    public void BeginSkillHit ( )
    {
        playerSkillController?.BeginSkillHit();
    }

    public void PerformSkillHit ( )
    {
        playerSkillController?.PerformSkillHit();
    }

    public void EndSkillHit ( )
    {
        playerSkillController?.EndSkillHit();
    }

    public void EndSkill ( )
    {
        playerSkillController?.EndAnimationSkill();
    }
}