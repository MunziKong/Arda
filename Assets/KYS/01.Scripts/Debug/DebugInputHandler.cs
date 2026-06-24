using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DebugInputHandler : MonoBehaviour
{
    [Header("Debug - 엔딩 직전 상태 복원")]
    [SerializeField] private QuestDefinition _lastQuestBeforeEnding;

    private void Update()
    {
        if (Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            DebugCompleteQuest();
        }

        if (Keyboard.current.numpad0Key.wasPressedThisFrame)
        {
            DebugResetGame();
        }

        if (Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            DebugGiveResources();
        }

        if (Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            DebugSetBeforeEnding();
        }
    }

    private void DebugCompleteQuest()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.ActiveQuest == null)
        {
            Debug.Log("[Debug] 진행 중인 퀘스트 없음");
            return;
        }

        Debug.Log($"[Debug] 퀘스트 강제 완료: {QuestManager.Instance.ActiveQuest.questName}");
        QuestManager.Instance.CompleteQuest();
    }

    private void DebugGiveResources()
    {
        PlayerInventory inventory = GameCore.PlayerInventory;

        if (inventory == null)
        {
            Debug.Log("[Debug] PlayerInventory 없음");
            return;
        }

        inventory.AddGold(10000);

        if (GameCore.GameDatabase != null)
        {
            foreach (ItemData item in GameCore.GameDatabase.AllItems)
            {
                if (item != null)
                {
                    inventory.AddItem(item, 10);
                }
            }
        }

        Debug.Log("[Debug] 골드 10000 + 전체 아이템 10개씩 지급");
    }

    private void DebugSetBeforeEnding()
    {
        if (_lastQuestBeforeEnding == null)
        {
            Debug.Log("[Debug] _lastQuestBeforeEnding 미설정");
            return;
        }

        if (QuestManager.Instance != null)
            QuestManager.Instance.ResetState();

        if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
        {
            SaveData data = SaveManager.Instance.Data;
            data.activeQuestId = string.Empty;
            data.pendingQuestId = string.Empty;
            data.lastStartedQuestId = _lastQuestBeforeEnding.questId;
            data.isQuestCompleted = false;
            data.endingCompleted = false;
            SaveManager.Instance.Save();
        }

        PlayerPrefs.SetString("MainQuestUI_LastCompletedQuestId", _lastQuestBeforeEnding.questId);
        PlayerPrefs.Save();

        Debug.Log($"[Debug] 엔딩 직전 상태 설정 완료 - lastQuestId: '{_lastQuestBeforeEnding.questId}'");
    }

    private void DebugResetGame()
    {
        Debug.Log("[Debug] 게임 전체 초기화");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetSave();
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetState();
        }

        if (GameCore.GameManager != null)
        {
            GameCore.GameManager.ResetState();
        }

        if (GameCore.PlayerInventory != null)
        {
            GameCore.PlayerInventory.ResetState();
        }

        if (GameCore.PlayerOwnedData != null)
        {
            GameCore.PlayerOwnedData.FullDebugReset();
        }

        if (GameCore.LoadoutManager != null)
        {
            GameCore.LoadoutManager.ClearLoadout();
        }

        if (GameCore.PlayerStats != null)
        {
            GameCore.PlayerStats.Heal(GameCore.PlayerStats.MaxHp);
        }

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.SetTime(GameTimeManager.Instance.StartHour);
        }

        GameCore.PlayerDeathHandler?.GetComponent<PlayerMove>()?.ResetState();
        GameCore.PlayerAnimController?.ResetToIdle();

        GameCore.ResetHUDPanels();

        Time.timeScale = 1f;
        GameCore.SceneTransitionManager?.MoveToSceneDirect(SceneNames.GetSceneName(SceneType.Main), "main_1");
    }
}
