using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CombatAIDebugVisualizer : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool drawWhenNotSelected = true;

    [Header("Attack Ranges (BT 그래프와 동일하게 설정)")]
    [SerializeField] private float basicAttackRange = 2f;
    [SerializeField] private float skill1Range = 3f;
    [SerializeField] private float skill2Range = 5f;
    [SerializeField] private SkillDefinition skill1;
    [SerializeField] private SkillDefinition skill2;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] [Range(0f, 360f)] private float viewAngle = 120f;

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 15f;

    [Header("Gizmo Settings")]
    [SerializeField] [Range(12, 96)] private int ringSegments = 40;
    [SerializeField] [Range(6, 48)] private int sectorSegments = 18;
    [SerializeField] private float labelHeight = 2.8f;

    private SkillCooldownTracker _tracker;
    private string _currentState = "Idle";
    private string _lastAttackName = "None";
    private int _attackCount;
    private Vector3 _homePosition;
    private bool _homeSet = false;

    private void Awake()
    {
        _tracker = GetComponent<SkillCooldownTracker>();
        _homePosition = transform.position;
        _homeSet = true;
    }

    public void ReportChase() => _currentState = "Chase";

    public void ReportAttack(string attackName)
    {
        _lastAttackName = attackName;
        _attackCount++;
        _currentState = $"Attack: {attackName}";
    }

    private void OnDrawGizmos()
    {
        if (drawWhenNotSelected)
        {
            DrawAll();
        }
    }

    private void OnDrawGizmosSelected() => DrawAll();

    private void DrawAll()
    {
        if (!showGizmos)
        {
            return;
        }

        GameObject player = null;
        try { player = GameObject.FindGameObjectWithTag("Player"); }
        catch (UnityException) { }

        Vector3 pos = transform.position;

        DrawRangeRings(pos);
        DrawFovSector(pos);
        DrawHomePosition();

        if (player == null)
        {
            return;
        }

        float dist = Vector3.Distance(pos, player.transform.position);
        DrawPlayerLine(pos, player.transform.position, dist);

#if UNITY_EDITOR
        DrawLabels(pos, dist);
#endif
    }

    private void DrawRangeRings(Vector3 pos)
    {
        DrawRing(pos, detectionRange, new Color(1f, 0.2f, 0.2f, 0.9f));
        DrawRing(pos, skill2Range, new Color(1f, 0.55f, 0f, 0.9f));
        DrawRing(pos, skill1Range, new Color(0.7f, 0.3f, 1f, 0.9f));
        DrawRing(pos, basicAttackRange, new Color(1f, 0.4f, 0.4f, 0.9f));
    }

    private void DrawHomePosition()
    {
        Vector3 drawPos = _homeSet ? _homePosition : transform.position;
        Gizmos.color = new Color(1f, 0.92f, 0.16f, 0.9f);
        Gizmos.DrawSphere(drawPos, 0.4f);
        float arm = 1f;
        Gizmos.DrawLine(drawPos + Vector3.left * arm, drawPos + Vector3.right * arm);
        Gizmos.DrawLine(drawPos + Vector3.back * arm, drawPos + Vector3.forward * arm);
        Gizmos.DrawLine(drawPos + Vector3.down * arm, drawPos + Vector3.up * arm);
        DrawRing(drawPos, patrolRadius, new Color(1f, 0.92f, 0.16f, 0.7f));
    }

    private void DrawFovSector(Vector3 origin)
    {
        if (detectionRange <= 0f || viewAngle <= 0f)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
        float half = viewAngle * 0.5f;
        int segs = Mathf.Max(6, sectorSegments);

        Vector3 prevDir = Quaternion.Euler(0f, -half, 0f) * transform.forward;
        Vector3 prevPt = origin + prevDir * detectionRange;
        Gizmos.DrawLine(origin, prevPt);

        for (int i = 1; i <= segs; i++)
        {
            float t = i / (float)segs;
            Vector3 dir = Quaternion.Euler(0f, Mathf.Lerp(-half, half, t), 0f) * transform.forward;
            Vector3 pt = origin + dir * detectionRange;
            Gizmos.DrawLine(prevPt, pt);
            Gizmos.DrawLine(origin, pt);
            prevPt = pt;
        }
    }

    private void DrawPlayerLine(Vector3 from, Vector3 to, float dist)
    {
        Color zoneColor = dist <= basicAttackRange ? new Color(1f, 0.2f, 0.2f, 1f) :
                          dist <= skill1Range ? new Color(0.7f, 0.3f, 1f, 1f) :
                          dist <= skill2Range ? new Color(1f, 0.55f, 0f, 1f) :
                                                     new Color(1f, 1f, 1f, 0.5f);
        Gizmos.color = zoneColor;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawSphere(to, 0.1f);

        Vector3 mid = (from + to) * 0.5f;
        Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.5f);
        Gizmos.DrawSphere(mid, 0.05f);
    }

#if UNITY_EDITOR
    private void DrawLabels(Vector3 pos, float dist)
    {
        var tracker = _tracker != null ? _tracker : GetComponent<SkillCooldownTracker>();

        string zone = dist <= basicAttackRange ? "● BasicRange" :
                      dist <= skill1Range ? "● Skill1Range" :
                      dist <= skill2Range ? "● Skill2Range" : "○ OutOfRange";

        string available = "";
        if (dist <= basicAttackRange)
        {
            available += "[Basic] ";
        }
        if (skill1 != null && dist <= skill1Range && (tracker == null || tracker.IsReady(skill1)))
        {
            available += "[Skill1] ";
        }
        if (skill2 != null && dist <= skill2Range && (tracker == null || tracker.IsReady(skill2)))
        {
            available += "[Skill2] ";
        }
        if (string.IsNullOrEmpty(available))
        {
            available = "None";
        }

        string cd1 = skill1 == null ? "—" : tracker == null || tracker.IsReady(skill1) ? "READY" : $"CD {tracker.GetRemainingCooldown(skill1):F1}s";
        string cd2 = skill2 == null ? "—" : tracker == null || tracker.IsReady(skill2) ? "READY" : $"CD {tracker.GetRemainingCooldown(skill2):F1}s";

        float sp = 0.28f;
        Vector3 b = pos + Vector3.up * labelHeight;

        Handles.Label(b, $"State : {_currentState}");
        Handles.Label(b + Vector3.up * sp, $"Dist : {dist:F2}m {zone}");
        Handles.Label(b + Vector3.up * sp * 2f, $"Avail : {available}");
        Handles.Label(b + Vector3.up * sp * 3f, $"Skill1: {cd1} Skill2: {cd2}");
        Handles.Label(b + Vector3.up * sp * 4f, $"Last : {_lastAttackName} (x{_attackCount})");
    }
#endif

    private void DrawRing(Vector3 center, float radius, Color color)
    {
        if (radius <= 0f)
        {
            return;
        }
        Gizmos.color = color;
        int segs = Mathf.Max(12, ringSegments);
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segs; i++)
        {
            float angle = (i / (float)segs) * Mathf.PI * 2f;
            Vector3 curr = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prev, curr);
            prev = curr;
        }
    }
}
