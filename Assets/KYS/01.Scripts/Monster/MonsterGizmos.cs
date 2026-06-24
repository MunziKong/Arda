using UnityEngine;

public class MonsterGizmos : MonoBehaviour
{
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float viewAngle = 120f;
    [SerializeField] private float patrolRadius = 10f;

    private Vector3 homePosition;
    private bool homeSet = false;

    void Start()
    {
        homePosition = transform.position;
        homeSet = true;
    }

    void OnDrawGizmos()
    {
        Vector3 patrolOrigin = homeSet ? homePosition : transform.position;

        // 순찰 범위 - 노란색 원 (홈 위치 고정)
        Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
        DrawFlatCircle(patrolOrigin, patrolRadius);

        // 감지 범위 - 빨간색 원 (몬스터 위치 기준)
        Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
        DrawFlatCircle(transform.position, detectionRange);

        // 시야각 - 선 2개
        Gizmos.color = Color.red;
        Vector3 leftDir = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDir * detectionRange);
        Gizmos.DrawRay(transform.position, rightDir * detectionRange);
    }

    void DrawFlatCircle(Vector3 center, float radius, int segments = 36)
    {
        float angleStep = 360f / segments;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
