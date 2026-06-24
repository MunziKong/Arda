// 몬스터 등급 — 등급이 높을수록 구조 확률 낮아짐
public enum MonsterGrade
{
    // 일반 — 구조 확률 높음
    Common,
    // 비일반 — 구조 확률 보통
    Uncommon,
    // 희귀 — 구조 확률 낮음
    Rare,
    // 에픽 — 구조 확률 매우 낮음
    Epic,
    // 전설 — 구조 확률 극히 낮음
    Legendary,
    // 유니크 — 특수 아이템 필요
    Unique
}

// 몬스터 행동 타입 — MonsterAI 의 행동 방식 결정
public enum MonsterType
{
    // 우호 — 플레이어 인지 시 다가옴
    Friendly,
    // 중립 — 플레이어 탐지 없이 혼자 배회
    Ignore,
    // 경계 — 플레이어 인지 시 도망감
    Flee
}
