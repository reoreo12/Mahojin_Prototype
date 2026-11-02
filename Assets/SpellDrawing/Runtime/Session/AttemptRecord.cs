// 시도 하나의 모든 정보(번호, 시각, 도안, 시간, 그린 획, 판정 결과). 기록·리스트·나중의 이펙트가 모두 이것을 받는다.
using System;

namespace VoxelMagic.SpellDrawing
{
    public sealed class AttemptRecord
    {
        public int Id { get; }
        public DateTime Timestamp { get; }
        public string TemplateId { get; }
        public float TimeLimitSec { get; }
        public float DrawSec { get; }
        public StrokeSet Drawn { get; }
        public StrokeSet Template { get; }
        public JudgeResult Result { get; }

        public int StrokeCount => Drawn.StrokeCount;
        public int RawPointCount => Drawn.PointCount;
        public bool IsEmpty => Result.IsEmpty;
        public float Similarity => Result.Similarity;

        public AttemptRecord(int id, DateTime timestamp, string templateId, float timeLimitSec, float drawSec,
            StrokeSet drawn, StrokeSet template, JudgeResult result)
        {
            Id = id;
            Timestamp = timestamp;
            TemplateId = templateId;
            TimeLimitSec = timeLimitSec;
            DrawSec = drawSec;
            Drawn = drawn;
            Template = template;
            Result = result;
        }
    }
}
