using UnityEngine;
using UnityEngine.SceneManagement;

public class DayNightSkyboxController : MonoBehaviour
{
    public static DayNightSkyboxController Instance { get; private set; }
    [Header("Cubemap (Legacy Cubemap 에셋 드래그)")]
    [SerializeField] private Cubemap animeDayCubemap;
    [SerializeField] private Cubemap animeSunsetCubemap;
    [SerializeField] private Cubemap animeNightCubemap;

    [Header("시간대 설정 (0 ~ 24)")]
    [SerializeField, Range(0f, 24f)] private float dawnHour    = 6f;
    [SerializeField, Range(0f, 24f)] private float sunsetHour  = 18f;
    [SerializeField, Range(0f, 24f)] private float nightHour   = 21f;
    [SerializeField, Range(0.1f, 3f)] private float blendHours = 1f;

    [Header("Directional Light")]
    [SerializeField] private Transform sunTransform;
    [SerializeField] private Light sunLight;
    [SerializeField, Range(10f, 80f)] private float sunElevation = 50f;  // 위에서 내리쬐는 고도 (고정)
    [SerializeField, Range(0f, 360f)] private float sunYOffset   = 180f; // 달/해 방향 오프셋
    [SerializeField] private Color dayLightColor    = new Color(1.0f, 0.95f, 0.8f);
    [SerializeField] private Color sunsetLightColor = new Color(1.0f, 0.5f,  0.2f);
    [SerializeField] private Color nightLightColor  = new Color(0.7f, 0.8f,  1.0f);  // 달빛: 차가운 청백
    [SerializeField] private float dayLightIntensity   = 1.2f;
    [SerializeField] private float nightLightIntensity = 0.2f;                        // 달빛: 0.1~0.3 권장

    [Header("구름 회전 속도 (도/초)")]
    [SerializeField] private float rotationSpeed = 0.5f;

    private Material _blendMat;
    private float _rotation;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 후 스카이박스 재적용
        if (_blendMat != null)
        {
            RenderSettings.skybox = _blendMat;
        }
    }

    private void Start()
    {
        Shader blendShader = Shader.Find("Custom/SkyboxBlend");
        if (blendShader == null)
        {
            Debug.LogError("[SkyboxController] Custom/SkyboxBlend 셰이더를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        if (animeDayCubemap == null || animeSunsetCubemap == null || animeNightCubemap == null)
        {
            Debug.LogError("[SkyboxController] Cubemap 3개를 모두 할당해주세요.");
            enabled = false;
            return;
        }

        _blendMat = new Material(blendShader);
        RenderSettings.skybox = _blendMat;

        float initHour = GameTimeManager.Instance != null ? GameTimeManager.Instance.CurrentHour : 8f;
        UpdateBlend(initHour);
        UpdateSun(initHour);
        DynamicGI.UpdateEnvironment();
    }

    private void Update()
    {
        if (_blendMat == null)
        {
            return;
        }

        float hour = GameTimeManager.Instance != null ? GameTimeManager.Instance.CurrentHour : 8f;
        UpdateBlend(hour);
        UpdateSun(hour);
        UpdateRotation();
    }

    private void UpdateBlend(float hour)
    {
        float dawnStart   = dawnHour   - blendHours;
        float sunsetStart = sunsetHour - blendHours;
        float nightStart  = nightHour  - blendHours;

        Cubemap from, to;
        float blend;

        if (hour < dawnStart)
        {
            from = animeNightCubemap; to = animeNightCubemap; blend = 0f;
        }
        else if (hour < dawnHour)
        {
            from  = animeNightCubemap;
            to    = animeDayCubemap;
            blend = Mathf.InverseLerp(dawnStart, dawnHour, hour);
        }
        else if (hour < sunsetStart)
        {
            from = animeDayCubemap; to = animeDayCubemap; blend = 0f;
        }
        else if (hour < sunsetHour)
        {
            from  = animeDayCubemap;
            to    = animeSunsetCubemap;
            blend = Mathf.InverseLerp(sunsetStart, sunsetHour, hour);
        }
        else if (hour < nightStart)
        {
            from = animeSunsetCubemap; to = animeSunsetCubemap; blend = 0f;
        }
        else if (hour < nightHour)
        {
            from  = animeSunsetCubemap;
            to    = animeNightCubemap;
            blend = Mathf.InverseLerp(nightStart, nightHour, hour);
        }
        else
        {
            from = animeNightCubemap; to = animeNightCubemap; blend = 0f;
        }

        _blendMat.SetTexture("_Tex1", from);
        _blendMat.SetTexture("_Tex2", to);
        _blendMat.SetFloat("_Blend", blend);
    }

    private void UpdateSun(float hour)
    {
        // 고도는 고정, Y축은 UpdateRotation에서 스카이박스와 함께 갱신
        if (sunLight == null)
        {
            return;
        }

        if (hour >= dawnHour && hour < sunsetHour)
        {
            // 낮: 일출~정오~일몰
            float mid = (dawnHour + sunsetHour) * 0.5f;
            float t = hour < mid
                ? Mathf.InverseLerp(dawnHour, mid, hour)
                : Mathf.InverseLerp(sunsetHour, mid, hour);
            sunLight.color     = Color.Lerp(sunsetLightColor, dayLightColor, t);
            sunLight.intensity = Mathf.Lerp(nightLightIntensity, dayLightIntensity, t);
        }
        else if (hour >= sunsetHour && hour < nightHour)
        {
            // 노을→밤: 태양색 → 달빛색
            float t = Mathf.InverseLerp(sunsetHour, nightHour, hour);
            sunLight.color     = Color.Lerp(sunsetLightColor, nightLightColor, t);
            sunLight.intensity = Mathf.Lerp(dayLightIntensity, nightLightIntensity, t);
        }
        else
        {
            // 밤 (새벽 포함): 달빛
            sunLight.color     = nightLightColor;
            sunLight.intensity = nightLightIntensity;
        }
    }

    private void UpdateRotation()
    {
        _rotation = (_rotation + rotationSpeed * Time.deltaTime) % 360f;
        _blendMat.SetFloat("_Rotation", _rotation);

        // 디렉셔널 라이트: 고도 고정, Y축은 스카이박스와 동기화
        if (sunTransform != null)
        {
            sunTransform.rotation = Quaternion.Euler(sunElevation, _rotation + sunYOffset, 0f);
        }

        DynamicGI.UpdateEnvironment();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (_blendMat != null)
        {
            Destroy(_blendMat);
        }
    }
}
