// 포획 마법진 자리표시 도안(원 안에 삼각형)의 점 목록을 코드로 만든다. 실제 디자인이 나오면 도안 에셋만 교체한다.
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public static class PlaceholderTemplateFactory
    {
        public const string Id = "capture_placeholder";
        public const string DisplayName = "포획 마법진 (임시: 원 안 삼각형)";

        public static StrokeSet CreateCircleWithTriangle(Vector2 center, float radius, int circleSegments)
        {
            circleSegments = Mathf.Max(8, circleSegments);
            var circle = new Vector2[circleSegments + 1];
            for (int i = 0; i <= circleSegments; i++)
            {
                float angle = Mathf.PI * 0.5f + i * Mathf.PI * 2f / circleSegments;
                circle[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            // 꼭짓점이 원에 닿는 정삼각형, 위쪽이 꼭짓점.
            var triangle = new Vector2[4];
            for (int i = 0; i < 3; i++)
            {
                float angle = Mathf.PI * 0.5f + i * Mathf.PI * 2f / 3f;
                triangle[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            triangle[3] = triangle[0];

            return new StrokeSet(new[] { circle, triangle });
        }

        public static StrokeSet CreateDefault() => CreateCircleWithTriangle(new Vector2(0.5f, 0.5f), 0.4f, 72);
    }
}
