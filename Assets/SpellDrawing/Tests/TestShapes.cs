// 테스트용 모양 생성기: 원본 변형(이동·크기·거꾸로·획 나누기·흔들림·절반)과 전혀 다른 모양(직선·낙서).
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.Tests
{
    public static class TestShapes
    {
        public static StrokeSet Template => PlaceholderTemplateFactory.CreateDefault();

        public static StrokeSet Translated(StrokeSet s, Vector2 offset) => s.Select(p => p + offset);

        public static StrokeSet Scaled(StrokeSet s, float scale, Vector2 pivot) => s.Select(p => pivot + (p - pivot) * scale);

        // 획 순서와 각 획의 방향을 모두 뒤집는다.
        public static StrokeSet Reversed(StrokeSet s)
        {
            var strokes = new List<Vector2[]>();
            for (int i = s.StrokeCount - 1; i >= 0; i--) strokes.Add(s.GetStroke(i).Reverse().ToArray());
            return new StrokeSet(strokes);
        }

        // 각 획을 parts개로 나눠 그린 것처럼 만든다.
        public static StrokeSet Split(StrokeSet s, int parts)
        {
            var strokes = new List<Vector2[]>();
            for (int i = 0; i < s.StrokeCount; i++)
            {
                var stroke = s.GetStroke(i);
                int size = Mathf.CeilToInt((stroke.Count - 1) / (float)parts);
                for (int start = 0; start < stroke.Count - 1; start += size)
                {
                    int end = Mathf.Min(stroke.Count - 1, start + size);
                    strokes.Add(stroke.Skip(start).Take(end - start + 1).ToArray());
                }
            }
            return new StrokeSet(strokes);
        }

        // 촘촘히 다시 뽑은 뒤 점마다 amplitude 이내로 흔든다. 사람 손떨림 흉내.
        public static StrokeSet Jittered(StrokeSet s, float amplitude, int seed, int density = 400)
        {
            var random = new System.Random(seed);
            var dense = StrokeResampler.Resample(s, density);
            return dense.Select(p => p + new Vector2(
                ((float)random.NextDouble() * 2f - 1f) * amplitude,
                ((float)random.NextDouble() * 2f - 1f) * amplitude));
        }

        // 선 길이 기준으로 앞쪽 fraction만 남긴다.
        public static StrokeSet Partial(StrokeSet s, float fraction, int density = 400)
        {
            var dense = StrokeResampler.Resample(s, density);
            int keep = Mathf.RoundToInt(density * fraction);
            var strokes = new List<Vector2[]>();
            for (int i = 0; i < dense.StrokeCount && keep > 0; i++)
            {
                var stroke = dense.GetStroke(i);
                int take = Mathf.Min(keep, stroke.Count);
                strokes.Add(stroke.Take(take).ToArray());
                keep -= take;
            }
            return new StrokeSet(strokes);
        }

        public static StrokeSet HorizontalLine() =>
            new StrokeSet(new[] { new[] { new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f) } });

        // 영역 전체를 오가는 지그재그 낙서.
        public static StrokeSet Scribble(int seed, int points = 40)
        {
            var random = new System.Random(seed);
            var stroke = new Vector2[points];
            for (int i = 0; i < points; i++)
                stroke[i] = new Vector2(0.05f + (float)random.NextDouble() * 0.9f, 0.05f + (float)random.NextDouble() * 0.9f);
            return new StrokeSet(new[] { stroke });
        }
    }
}
