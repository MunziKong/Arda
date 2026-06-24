using UnityEngine;

public class SceneSpawnPoint : MonoBehaviour
{
    [SerializeField] private string _spawnId;

    public string SpawnId => _spawnId;

    private void OnEnable()
    {
        if (GameCore.SceneTransitionManager != null)
        {
            GameCore.SceneTransitionManager.RegisterSpawnPoint(this);
        }
    }

    private void OnDisable()
    {
        if (GameCore.SceneTransitionManager != null)
        {
            GameCore.SceneTransitionManager.UnregisterSpawnPoint(this);
        }
    }
}