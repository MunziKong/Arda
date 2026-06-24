using UnityEngine;
using UnityEngine.Events;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [SerializeField] private float startHour = 8f;

    // 낮(8~24시) 20분 + 밤(0~8시) 10분 = 30분에 하루
    // 24게임시간 / 1800실제초 = 75초당 1게임시간
    [SerializeField] private float realSecondsPerGameDay = 1800f;

    public float CurrentHour { get; private set; }
    public float StartHour => startHour;

    public UnityEvent<float> OnHourChanged;

    private float _lastHour;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentHour = startHour;
        _lastHour = startHour;
    }

    private void Update()
    {
        float gameHoursPerSecond = 24f / realSecondsPerGameDay;
        CurrentHour += Time.deltaTime * gameHoursPerSecond;

        if (CurrentHour >= 24f)
        {
            CurrentHour -= 24f;
        }

        if (Mathf.FloorToInt(CurrentHour) != Mathf.FloorToInt(_lastHour))
        {
            OnHourChanged?.Invoke(CurrentHour);
        }

        _lastHour = CurrentHour;
    }

    public void SetTime(float hour)
    {
        CurrentHour = Mathf.Repeat(hour, 24f);
        _lastHour = CurrentHour;
        OnHourChanged?.Invoke(CurrentHour);
    }

    public bool IsDay() => CurrentHour >= 8f && CurrentHour < 20f;
    public bool IsNight() => !IsDay();

    // MonsterData의 spawnStartHour ~ spawnEndHour 범위 체크 (자정 넘김 지원)
    public bool IsSpawnTime(MonsterData data)
    {
        if (data.alwaysSpawn)
        {
            return true;
        }

        float start = data.spawnStartHour;
        float end = data.spawnEndHour;

        if (start <= end)
        {
            return CurrentHour >= start && CurrentHour < end;
        }
        else
        {
            return CurrentHour >= start || CurrentHour < end;
        }
    }
}
