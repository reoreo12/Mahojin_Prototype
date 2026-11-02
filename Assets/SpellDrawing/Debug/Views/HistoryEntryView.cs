// 기록 리스트의 토글 하나. 접히면 한 줄 요약, 펼치면 원본·그린 것·겹쳐 보기 썸네일과 값을 보여 준다.
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class HistoryEntryView : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image scoreBar;
        [SerializeField] private Text summaryText;
        [SerializeField] private Text arrowText;
        [SerializeField] private GameObject details;
        [SerializeField] private RawImage templateImage;
        [SerializeField] private RawImage drawnImage;
        [SerializeField] private RawImage overlayImage;
        [SerializeField] private Text detailText;

        private AttemptRecord _record;
        private ThumbnailCache _cache;
        private bool _holdingThumbnails;

        public void Setup(AttemptRecord record, ThumbnailCache cache, RecordSettings settings)
        {
            _record = record;
            _cache = cache;
            float similarity = record.Similarity;
            summaryText.text = $"#{record.Id}  {record.Timestamp:HH:mm:ss}  유사도 {similarity:F2}" +
                               (record.IsEmpty ? "  (그린 획 없음)" : string.Empty);
            scoreBar.color = settings.ScoreColor(similarity);
            var result = record.Result;
            detailText.text =
                $"유사도 {similarity:F2}\n커버리지 {result.Coverage:F2}\n정밀도 {result.Precision:F2}\n" +
                $"정규화 {result.Normalized.Similarity:F2}\n획 {record.StrokeCount}개\n그린 시간 {record.DrawSec:F1}초";

            toggle.SetIsOnWithoutNotify(false);
            toggle.onValueChanged.AddListener(SetExpanded);
            SetExpanded(false);
        }

        public void Collapse() => toggle.isOn = false;

        private void SetExpanded(bool expanded)
        {
            details.SetActive(expanded);
            arrowText.text = expanded ? "▼" : "▶";

            if (expanded && !_holdingThumbnails)
            {
                var set = _cache.Acquire(_record);
                templateImage.texture = set.Template;
                drawnImage.texture = set.Drawn;
                overlayImage.texture = set.Overlay;
                _holdingThumbnails = true;
            }
            else if (!expanded && _holdingThumbnails)
            {
                ReleaseThumbnails();
            }
        }

        private void ReleaseThumbnails()
        {
            templateImage.texture = null;
            drawnImage.texture = null;
            overlayImage.texture = null;
            _cache.Release(_record.Id);
            _holdingThumbnails = false;
        }

        private void OnDestroy()
        {
            if (_holdingThumbnails) _cache.Release(_record.Id);
        }
    }
}
