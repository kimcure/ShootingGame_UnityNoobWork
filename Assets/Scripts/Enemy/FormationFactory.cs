using UnityEngine;

/// <summary>
/// 대열(Formation) 모양을 계산하는 순수 함수 모음. 씬/오브젝트와 무관하다.
///
/// 결과는 "대열의 기준점으로부터의 상대 좌표(offset)" 배열이다.
///  - x : 좌우 위치,  y : 위쪽으로 얼마나 더 뒤에서 출발하는지 (y가 클수록 늦게 화면에 들어온다)
///  - 호출한 쪽이 기준점(화면 위쪽 바깥)을 더해 실제 소환 좌표를 만든다. (수업의 "기준점 + 상대 위치")
///
/// 수업 17장의 규칙적 배치(i % columns, i / columns)와
/// 5강의 원형 배치(2π/N, sin·cos)를 일반화한 것이다.
/// </summary>
public static class FormationFactory
{
    public static Vector2[] GetOffsets(FormationType type, int count, float spacing, int columns = 5)
    {
        count = Mathf.Max(1, count);
        Vector2[] offsets = new Vector2[count];

        // 가운데 인덱스. count=5 → 2, count=4 → 1.5  (좌우 대칭 배치를 만들 때 사용)
        float center = (count - 1) * 0.5f;

        switch (type)
        {
            case FormationType.Line:
                // ■ ■ ■ ■ ■
                for (int i = 0; i < count; i++)
                    offsets[i] = new Vector2((i - center) * spacing, 0f);
                break;

            case FormationType.Column:
                // 세로로 길게: 뒤쪽일수록 y가 크다
                for (int i = 0; i < count; i++)
                    offsets[i] = new Vector2(0f, i * spacing);
                break;

            case FormationType.V:
                // 가운데가 가장 아래(먼저 도착), 양 끝이 위
                for (int i = 0; i < count; i++)
                {
                    float distanceFromCenter = Mathf.Abs(i - center);
                    offsets[i] = new Vector2((i - center) * spacing, distanceFromCenter * spacing * 0.8f);
                }
                break;

            case FormationType.InvertedV:
                // 가운데가 가장 위(늦게 도착), 양 끝이 아래  → ∧
                for (int i = 0; i < count; i++)
                {
                    float distanceFromCenter = Mathf.Abs(i - center);
                    offsets[i] = new Vector2((i - center) * spacing, (center - distanceFromCenter) * spacing * 0.8f);
                }
                break;

            case FormationType.Grid:
                {
                    int cols = Mathf.Clamp(columns, 1, count);
                    for (int i = 0; i < count; i++)
                    {
                        int column = i % cols;   // 몇 번째 칸
                        int row = i / cols;      // 몇 번째 줄
                        offsets[i] = new Vector2((column - (cols - 1) * 0.5f) * spacing, row * spacing);
                    }
                    break;
                }

            case FormationType.Circle:
                {
                    // 이웃한 적 사이 간격이 spacing이 되도록 반지름을 정한다 (둘레 = spacing × count)
                    float radius = count == 1 ? 0f : spacing * count / (2f * Mathf.PI);
                    float angleStep = (2f * Mathf.PI) / count;   // 한 바퀴 = 2π, 적 N명 → 각도 간격 = 2π / N

                    for (int i = 0; i < count; i++)
                    {
                        float angle = angleStep * i;
                        // 가장 아래 점이 y = 0이 되도록 radius만큼 올린다
                        offsets[i] = new Vector2(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius + radius);
                    }
                    break;
                }

            case FormationType.Zigzag:
                // 좌/우를 번갈아 서면서 조금씩 위로
                for (int i = 0; i < count; i++)
                    offsets[i] = new Vector2((i % 2 == 0 ? -0.5f : 0.5f) * spacing, i * spacing * 0.6f);
                break;
        }

        return offsets;
    }
}
