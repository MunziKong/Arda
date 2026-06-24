using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _dieAnimDuration = 1.5f;
    [SerializeField] private float _fadeOutDuration = 1f;
    [SerializeField] private string _deathDialogue = "쓰러지면 안돼..";
    [SerializeField] private string _penaltyDialogue = "(사망 시 보유한 아이템, 골드를 30%씩 잃게 됩니다)";

    [Header("Death Penalty")]
    [SerializeField, Range(0f, 1f)] private float _itemLossPenalty = 0.3f;
    [SerializeField, Range(0f, 1f)] private float _goldLossPenalty = 0.3f;

    [Header("Ref")]
    [SerializeField] private PlayerAnimController _animController;

    private bool _isDying;

    public void HandleDeath()
    {
        if (_isDying) return;
        _isDying = true;
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        GameCore.GameController.InactiveCoreUI();
        GameCore.PlayerInputs.SetUIInput();
        GameCore.PlayerInputs.SetCursorLock(false);

        _animController.SetDeathTrigger();

        yield return new WaitForSecondsRealtime(_dieAnimDuration);

        ApplyDeathPenalty();

        GameCore.FadePanel.gameObject.SetActive(true);
        GameCore.SoundManager?.FadeOutBgm(_fadeOutDuration);

        bool fadeOutDone = false;
        GameCore.FadePanel.FadeOut(_fadeOutDuration, () => fadeOutDone = true);
        yield return new WaitUntil(() => fadeOutDone);

        bool dialogueDone = false;
        GameCore.FadePanel.ShowDialogue(
            _deathDialogue,
            _penaltyDialogue,
            () => dialogueDone = true
        );
        yield return new WaitUntil(() => dialogueDone);

        GameCore.RescueManager.ClearList();
        GameCore.SceneTransitionManager.MoveToSceneAfterDeath(
            SceneNames.GetSceneName(SceneType.Main),
            "main_start"
        );
    }

    private void ApplyDeathPenalty()
    {
        PlayerInventory inventory = GameCore.PlayerInventory;
        if (inventory == null) return;

        List<(ItemData item, int amount)> reductions = new();

        foreach (InventoryItem item in inventory.Items)
        {
            if (item == null || item.ItemData == null || item.Quantity <= 0) continue;

            int reduceAmount = Mathf.FloorToInt(item.Quantity * _itemLossPenalty);
            if (reduceAmount > 0)
            {
                reductions.Add((item.ItemData, reduceAmount));
            }
        }

        foreach ((ItemData item, int amount) in reductions)
        {
            inventory.RemoveItem(item, amount);
        }

        int goldReduce = Mathf.FloorToInt(inventory.Gold * _goldLossPenalty);
        if (goldReduce > 0)
        {
            inventory.RemoveGold(goldReduce);
        }
    }

    public void ResetDying()
    {
        _isDying = false;
        GetComponent<PlayerMove>()?.ResetState();
    }
}