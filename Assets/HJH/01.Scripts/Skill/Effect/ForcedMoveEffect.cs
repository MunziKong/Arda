using UnityEngine;

[CreateAssetMenu(fileName = "ForcedMoveEffect", menuName = "Arda/Effect/ForcedMoveEffect")]
public class ForcedMoveEffect : SkillEffect
{
    public const float MOVE_SPEED = 1f;
    public const float MOVE_DURATION = 1f;
    public const float JUMP_HEIGHT = 1f;
    public const float JUMP_DELAY = 0f;

    // multipliers[0] : 스피드 배율
    // multipliers[1] : Duration 배율
    // multipliers[2] : Jump Height 배율
    // multipliers[3] : Jump Delay (이 시간 이후에 점프)
    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context.Caster == null)
        {
            return;
        }

        PlayerMove move = context.Caster.GetComponent<PlayerMove>();

        if (move == null)
        {
            return;
        }

        float calculatedSpeed = MOVE_SPEED * multipliers[0];
        float calculatedDuration = MOVE_DURATION * multipliers[1];
        float calculatedJumpHeight = multipliers.Length > 2 ? JUMP_HEIGHT * multipliers[2] : 0f;
        float calculatedJumpDelay = multipliers.Length > 3 ? JUMP_DELAY + multipliers[3] : 0f;

        move.StartActionMove(
            context.Skill.actionDirectionMode,
            context.CastForward,
            calculatedSpeed,
            calculatedDuration,
            calculatedJumpHeight,
            calculatedJumpDelay
        );
    }
}