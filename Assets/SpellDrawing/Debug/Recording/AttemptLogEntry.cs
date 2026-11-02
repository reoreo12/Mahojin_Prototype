// 기록 파일 한 줄의 내용. CSV와 JSONL이 같은 항목·같은 값을 쓰도록 이 클래스 하나에서 만든다.
using System;
using System.Globalization;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    [Serializable]
    public sealed class AttemptLogEntry
    {
        public const string CsvHeader =
            "id,timestamp,templateId,timeLimitSec,drawSec,strokeCount,rawPointCount,resampleCount,tolerance," +
            "similarity,coverage,precision,similarityRaw,similarityNorm,coverageNorm,precisionNorm,judgeMs,empty," +
            "pointsFile,templateImage,drawnImage,overlayImage";

        [SerializeField] private int id;
        [SerializeField] private string timestamp;
        [SerializeField] private string templateId;
        [SerializeField] private float timeLimitSec;
        [SerializeField] private float drawSec;
        [SerializeField] private int strokeCount;
        [SerializeField] private int rawPointCount;
        [SerializeField] private int resampleCount;
        [SerializeField] private float tolerance;
        [SerializeField] private float similarity;
        [SerializeField] private float coverage;
        [SerializeField] private float precision;
        [SerializeField] private float similarityRaw;
        [SerializeField] private float similarityNorm;
        [SerializeField] private float coverageNorm;
        [SerializeField] private float precisionNorm;
        [SerializeField] private double judgeMs;
        [SerializeField] private bool empty;
        [SerializeField] private string pointsFile;
        [SerializeField] private string templateImage;
        [SerializeField] private string drawnImage;
        [SerializeField] private string overlayImage;

        public static AttemptLogEntry From(AttemptRecord record)
        {
            var r = record.Result;
            return new AttemptLogEntry
            {
                id = record.Id,
                timestamp = record.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                templateId = record.TemplateId,
                timeLimitSec = record.TimeLimitSec,
                drawSec = record.DrawSec,
                strokeCount = record.StrokeCount,
                rawPointCount = record.RawPointCount,
                resampleCount = r.ResampleCount,
                tolerance = r.Tolerance,
                similarity = r.Similarity,
                coverage = r.Coverage,
                precision = r.Precision,
                similarityRaw = r.Raw.Similarity,
                similarityNorm = r.Normalized.Similarity,
                coverageNorm = r.Normalized.Coverage,
                precisionNorm = r.Normalized.Precision,
                judgeMs = r.JudgeMs,
                empty = r.IsEmpty,
                pointsFile = AttemptFileNames.Points(record.Id),
                templateImage = AttemptFileNames.TemplateImage(record.Id),
                drawnImage = AttemptFileNames.DrawnImage(record.Id),
                overlayImage = AttemptFileNames.OverlayImage(record.Id),
            };
        }

        public string ToJson() => JsonUtility.ToJson(this);

        public string ToCsvRow()
        {
            var c = CultureInfo.InvariantCulture;
            return string.Join(",",
                id.ToString(c), timestamp, Quote(templateId), timeLimitSec.ToString("F2", c), drawSec.ToString("F2", c),
                strokeCount.ToString(c), rawPointCount.ToString(c), resampleCount.ToString(c), tolerance.ToString("F4", c),
                similarity.ToString("F4", c), coverage.ToString("F4", c), precision.ToString("F4", c),
                similarityRaw.ToString("F4", c), similarityNorm.ToString("F4", c), coverageNorm.ToString("F4", c),
                precisionNorm.ToString("F4", c), judgeMs.ToString("F3", c), empty ? "true" : "false",
                pointsFile, templateImage, drawnImage, overlayImage);
        }

        private static string Quote(string value) =>
            value != null && (value.Contains(",") || value.Contains("\"")) ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}
