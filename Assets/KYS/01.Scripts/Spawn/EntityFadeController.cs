using System.Collections;
using UnityEngine;

public class EntityFadeController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;

    private Material[][] _materials;
    private bool         _initialized;

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;

        var renderers = GetComponentsInChildren<Renderer>(true);
        _materials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] is ParticleSystemRenderer)
            {
                continue;
            }
            _materials[i] = renderers[i].materials;
        }
    }

    public void PrepareForFadeIn()
    {
        EnsureInitialized();
        SetTransparent(true);
        SetAlpha(0f);
    }

    public IEnumerator FadeIn()
    {
        EnsureInitialized();
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(1f);
        SetTransparent(false);
    }

    public IEnumerator FadeOut()
    {
        EnsureInitialized();
        SetTransparent(true);
        SetAlpha(1f);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (_materials == null)
        {
            return;
        }
        foreach (var mats in _materials)
        {
            if (mats == null)
            {
                continue;
            }
            foreach (var mat in mats)
            {
                if (mat == null)
                {
                    continue;
                }
                string prop = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                Color c = mat.GetColor(prop);
                c.a = alpha;
                mat.SetColor(prop, c);
            }
        }
    }

    private void SetTransparent(bool on)
    {
        if (_materials == null)
        {
            return;
        }
        foreach (var mats in _materials)
        {
            if (mats == null)
            {
                continue;
            }
            foreach (var mat in mats)
            {
                if (mat == null)
                {
                    continue;
                }
                if (on)
                {
                    mat.SetFloat("_Surface", 1f);
                    mat.SetFloat("_Blend", 0f);
                    mat.SetFloat("_ZWrite", 0f);
                    mat.SetFloat("_SrcBlend", 5f);
                    mat.SetFloat("_DstBlend", 10f);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
                else
                {
                    mat.SetFloat("_Surface", 0f);
                    mat.SetFloat("_ZWrite", 1f);
                    mat.SetFloat("_SrcBlend", 1f);
                    mat.SetFloat("_DstBlend", 0f);
                    mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = -1;
                }
            }
        }
    }
}
