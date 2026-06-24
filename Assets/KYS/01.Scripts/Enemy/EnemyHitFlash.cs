using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _mpb;
    private Coroutine _flashCoroutine;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    public void Flash()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetFlashColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        ClearFlashColor();
        _flashCoroutine = null;
    }

    private void SetFlashColor(Color color)
    {
        foreach (var r in _renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorID, color);
            r.SetPropertyBlock(_mpb);
        }
    }

    private void ClearFlashColor()
    {
        foreach (var r in _renderers)
        {
            r.SetPropertyBlock(null);
        }
    }
}
