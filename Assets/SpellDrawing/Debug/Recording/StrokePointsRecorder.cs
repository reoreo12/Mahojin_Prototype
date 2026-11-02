// 시도마다 그린 점 목록(획별, 그리기 공간 좌표)을 points/attempt_001.json으로 저장한다. 나중에 다시 판정해 보는 데 쓴다.
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class StrokePointsRecorder : FileAttemptRecorder
    {
        [Serializable]
        private sealed class StrokeDto
        {
            [SerializeField] private Vector2[] points;
            public StrokeDto(Vector2[] points) { this.points = points; }
        }

        [Serializable]
        private sealed class PointsDto
        {
            [SerializeField] private int id;
            [SerializeField] private string templateId;
#pragma warning disable 0414 // JsonUtility가 읽는다
            [SerializeField] private string coordinateSpace = "drawing area 0~1, origin bottom-left";
#pragma warning restore 0414
            [SerializeField] private StrokeDto[] strokes;

            public PointsDto(AttemptRecord record)
            {
                id = record.Id;
                templateId = record.TemplateId;
                strokes = Enumerable.Range(0, record.Drawn.StrokeCount)
                    .Select(i => new StrokeDto(record.Drawn.GetStroke(i).ToArray()))
                    .ToArray();
            }
        }

        private readonly RecordSettings _settings;

        public StrokePointsRecorder(RecordSettings settings, string sessionFolder) : base(sessionFolder)
        {
            _settings = settings;
        }

        protected override bool IsEnabled => _settings.WritePoints;

        protected override void Write(AttemptRecord record)
        {
            File.WriteAllText(PathOf(AttemptFileNames.Points(record.Id)), JsonUtility.ToJson(new PointsDto(record)));
        }
    }
}
