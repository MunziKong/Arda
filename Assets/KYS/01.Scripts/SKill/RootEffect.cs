using UnityEngine;

// multipliers[0] : 속박 지속 시간 (초)
// multipliers[1] : 속박 범위 반경 (0이면 무조건 적용)
[CreateAssetMenu(fileName = "RootEffect", menuName = "Arda/Effect/RootEffect")]
public class RootEffect : SkillEffect
{
    private const int DURATION_IDX = 0;
    private const int RADIUS_IDX = 1;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Target == null)
        {
            return;
        }

        float radius = GetMultiplier(multipliers, RADIUS_IDX, 0f);
        if (radius > 0f)
        {
            float dist = Vector3.Distance(context.Point, context.Target.transform.position);
            if (dist > radius)
            {
                return;
            }
        }

        IRootable rootable = context.Target.GetComponentInParent<IRootable>();
        if (rootable == null)
        {
            return;
        }

        float duration = GetMultiplier(multipliers, DURATION_IDX, 2f);
        rootable.Root(duration);
    }

    private float GetMultiplier(float[] multipliers, int index, float defaultValue)
    {
        if (multipliers == null || multipliers.Length <= index)
        {
            return defaultValue;
        }

        return multipliers[index];
    }
}
