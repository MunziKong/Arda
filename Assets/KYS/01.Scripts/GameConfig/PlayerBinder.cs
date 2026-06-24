using Unity.Behavior;
using UnityEngine;

public class PlayerBinder : MonoBehaviour
{
    private void OnEnable() => Bind();

    public void Bind()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        if (!TryGetComponent<BehaviorGraphAgent>(out var agent))
        {
            return;
        }

        if (agent.GetVariable("PlayerTransform", out BlackboardVariable<Transform> playerVar))
        {
            playerVar.Value = player.transform;
        }

        // 재활성화 시 전투 상태 초기화
        if (agent.GetVariable("CanSeeTarget", out BlackboardVariable<bool> canSeeVar))
        {
            canSeeVar.Value = false;
        }
    }
}
