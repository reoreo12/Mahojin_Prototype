// 테스트 화면의 기록·썸네일 설정: 파일 저장 켜기/끄기, 썸네일 크기·선 굵기·캐시 상한, 겹쳐 보기와 점수 색.
using UnityEngine;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    [CreateAssetMenu(fileName = "RecordSettings", menuName = "VoxelMagic/SpellDrawing/Record Settings")]
    public sealed class RecordSettings : ScriptableObject
    {
        [Header("파일 기록")]
        [SerializeField] private bool recordEnabled = true;
        [SerializeField] private bool writeCsv = true;
        [SerializeField] private bool writeJsonl = true;
        [SerializeField] private bool writePoints = true;
        [SerializeField] private bool writeImages = true;
        [SerializeField] private string folderName = "DrawingLogs";

        [Header("썸네일")]
        [SerializeField, Min(32)] private int thumbnailSize = 256;
        [SerializeField, Min(0.5f)] private float lineThicknessPx = 3f;
        [SerializeField, Min(1), Tooltip("썸네일 텍스처를 유지할 최대 시도 수 (펼친 것은 제외)")] private int thumbnailCacheAttempts = 10;
        [SerializeField, Range(0f, 1f), Tooltip("허용 거리의 이 비율 안이면 '맞은 선'으로 색칠")] private float overlayHitRatio = 0.5f;

        [Header("겹쳐 보기 색")]
        [SerializeField] private Color32 background = new Color32(24, 26, 34, 255);
        [SerializeField] private Color32 templateLine = new Color32(110, 200, 255, 255);
        [SerializeField] private Color32 drawnLine = new Color32(255, 200, 90, 255);
        [SerializeField] private Color32 covered = new Color32(150, 150, 160, 255);
        [SerializeField] private Color32 missed = new Color32(235, 60, 60, 255);
        [SerializeField] private Color32 onTarget = new Color32(80, 220, 110, 255);
        [SerializeField] private Color32 stray = new Color32(255, 150, 30, 255);

        [Header("점수 색")]
        [SerializeField, Range(0f, 1f)] private float midThreshold = 0.5f;
        [SerializeField, Range(0f, 1f)] private float highThreshold = 0.8f;
        [SerializeField] private Color lowColor = new Color(0.9f, 0.3f, 0.3f);
        [SerializeField] private Color midColor = new Color(0.95f, 0.75f, 0.25f);
        [SerializeField] private Color highColor = new Color(0.35f, 0.85f, 0.45f);

        public bool RecordEnabled => recordEnabled;
        public bool WriteCsv => writeCsv;
        public bool WriteJsonl => writeJsonl;
        public bool WritePoints => writePoints;
        public bool WriteImages => writeImages;
        public string FolderName => folderName;
        public int ThumbnailSize => thumbnailSize;
        public float LineThicknessPx => lineThicknessPx;
        public int ThumbnailCacheAttempts => thumbnailCacheAttempts;
        public float OverlayHitRatio => overlayHitRatio;
        public Color32 Background => background;
        public Color32 TemplateLine => templateLine;
        public Color32 DrawnLine => drawnLine;
        public Color32 Covered => covered;
        public Color32 Missed => missed;
        public Color32 OnTarget => onTarget;
        public Color32 Stray => stray;

        public Color ScoreColor(float similarity) =>
            similarity >= highThreshold ? highColor : similarity >= midThreshold ? midColor : lowColor;
    }
}
