using System;
using UnityEngine;

public class MonsterInteraction : MonoBehaviour, IInteractable
{
    [Header("Interaction UI")]
    [SerializeField] private Transform _popupPoint;
    [SerializeField] private Sprite _interactionIcon;

    [Header("Hold")]
    [SerializeField] private float _holdTime = 3f;


    [Header("Outline")]
    [SerializeField] private EPOOutline.Outlinable _outlinable;

    public Transform PopupPoint => _popupPoint;
    public Sprite InteractionIcon => _interactionIcon;

    public bool IsEnabled => true;
    public bool RequireHold => true;
    public float HoldTime => _holdTime;

    // UI, 사운드 등이 구독
    public static event Action<MonsterData> OnRescueSuccess;
    public static event Action<MonsterData> OnRescueFailed;

    private MonsterController _controller;

    private void Awake()
    {
        _controller = GetComponentInChildren<MonsterController>();

        if (_controller == null)
            Debug.LogWarning($"[MonsterInteraction] MonsterController를 찾을 수 없습니다: {gameObject.name}");
    }

    public void Interact()
    {
        if (_controller == null || _controller.data == null)
        {
            return;
        }

        MonsterData data = _controller.data;

        float chance = GameCore.RescueManager.GetRescueRatio(data);
        bool usedItem = GameCore.RescueManager.UseCurrentRescueItem();

        if (!usedItem)
        {
            Debug.Log($"<color=red>[Rescue] 구조 아이템 사용 실패</color>");
            return;
        }

        bool success = UnityEngine.Random.value <= chance;

        if (success)
        {
            Debug.Log($"<color=green>[Rescue] 구조 성공: {data.monsterName} (확률 {chance * 100f:0}%)</color>");
            OnRescueSuccess?.Invoke(data);
            GameCore.RescueManager.AddRescuedMonster(data);
            GameCore.SoundManager?.PlayRescue(true);
        }
        else
        {
            Debug.Log($"<color=red>[Rescue] 구조 실패: {data.monsterName} (확률 {chance * 100f:0}%)</color>");
            OnRescueFailed?.Invoke(data);
            GameCore.RescueManager.FailRescuedMonster(data);
            GameCore.SoundManager?.PlayRescue(false);
        }

        _controller.Rescue();
    }

    public void SetOutline(bool enable)
    {
        if (_outlinable == null)
        {
            return;
        }

        _outlinable.enabled = enable;
    }
}