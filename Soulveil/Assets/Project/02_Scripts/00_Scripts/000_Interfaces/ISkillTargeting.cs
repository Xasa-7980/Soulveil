using UnityEngine;
public interface ISkillTargeting
{
    bool RequiresHold { get; }

    void BeginTargeting ( SkillTargetContext context );
    void UpdateTargeting ( SkillTargetContext context );
    void EndTargeting ( SkillTargetContext context );
}

public interface ISkillPreview
{
    GameObject PreviewVFX { get; }
}

public interface ISkillDebug
{
    void DrawDebug ( SkillTargetContext context );
}