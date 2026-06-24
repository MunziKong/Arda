using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CastIndicator : MonoBehaviour
{
    public static CastIndicator Current { get; private set; }

    [SerializeField] private DecalProjector _outlineDecal;
    [SerializeField] private DecalProjector _fillDecal;

    public bool IsDone { get; private set; }

    public void Initialize(float size, float duration)
    {
        Current = this;
        IsDone = false;

        _outlineDecal.size = new Vector3(size, size, 6f);
        _fillDecal.size = new Vector3(0f, 0f, 6f);

        StartCoroutine(GrowRoutine(size, duration));
    }

    private IEnumerator GrowRoutine(float targetSize, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float current = Mathf.Lerp(0f, targetSize, Mathf.Clamp01(elapsed / duration));
            _fillDecal.size = new Vector3(current, current, 6f);
            yield return null;
        }

        _fillDecal.size = new Vector3(targetSize, targetSize, 6f);
        IsDone = true;
        if (Current == this)
        {
            Current = null;
        }
    }

    private void OnDestroy()
    {
        if (Current == this)
        {
            Current = null;
        }
    }
}
