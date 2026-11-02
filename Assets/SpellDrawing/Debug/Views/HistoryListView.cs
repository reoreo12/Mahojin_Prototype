// 스크롤되는 기록 리스트. 시도가 끝날 때마다 토글을 맨 위에 추가하고, "모두 접기"와 "리스트 지우기"를 처리한다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class HistoryListView : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private HistoryEntryView entryPrefab;
        [SerializeField] private Button collapseAllButton;
        [SerializeField] private ScrollRect scrollRect;

        private readonly List<HistoryEntryView> _entries = new List<HistoryEntryView>();
        private DrawingSession _session;
        private ThumbnailCache _cache;
        private RecordSettings _settings;
        private KoreanFontApplier _fontApplier;

        public void Bind(DrawingSession session, ThumbnailCache cache, RecordSettings settings, KoreanFontApplier fontApplier)
        {
            _session = session;
            _cache = cache;
            _settings = settings;
            _fontApplier = fontApplier;
            _session.AttemptCompleted += AddEntry;
            collapseAllButton.onClick.AddListener(CollapseAll);
        }

        private void OnDestroy()
        {
            if (_session != null) _session.AttemptCompleted -= AddEntry;
        }

        public void CollapseAll()
        {
            foreach (var entry in _entries) entry.Collapse();
        }

        public void Clear()
        {
            foreach (var entry in _entries) Destroy(entry.gameObject);
            _entries.Clear();
            _cache.Clear();
        }

        private void AddEntry(AttemptRecord record)
        {
            var entry = Instantiate(entryPrefab, content);
            entry.transform.SetAsFirstSibling();
            if (_fontApplier != null) _fontApplier.Apply(entry.gameObject);
            entry.Setup(record, _cache, _settings);
            _entries.Add(entry);
            if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
