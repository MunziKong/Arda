using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Play Animation",
    story: "Set animation trigger [Trigger] in [Animator]",
    category: "Action/Monster",
    id: "d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9")]
public partial class PlayAnimationAction : Action
{
    [SerializeReference] public BlackboardVariable<string> Trigger;
    [SerializeReference] public BlackboardVariable<Animator> Animator;

    protected override Status OnStart()
    {
        if (Animator?.Value == null)
        {
            LogFailure("No Animator set.");
            return Status.Failure;
        }

        Animator.Value.SetTrigger(Trigger.Value);
        return Status.Success;
    }
}
