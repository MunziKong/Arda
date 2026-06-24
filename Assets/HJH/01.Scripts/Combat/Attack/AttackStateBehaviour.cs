using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    [SerializeField] private int _stepIndex;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        PlayerAnimController animController = animator.GetComponent<PlayerAnimController>();

        if (animController == null)
        {
            return;
        }

        animController.OnAttackStateEnter(_stepIndex);
    }
}