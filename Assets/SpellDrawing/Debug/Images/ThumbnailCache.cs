// 기록 리스트 썸네일 텍스처를 펼칠 때만 만들고, 펼치지 않은 시도가 상한을 넘으면 오래 안 본 것부터 파괴한다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class ThumbnailSet
    {
        public Texture2D Template { get; }
        public Texture2D Drawn { get; }
        public Texture2D Overlay { get; }

        public ThumbnailSet(Texture2D template, Texture2D drawn, Texture2D overlay)
        {
            Template = template;
            Drawn = drawn;
            Overlay = overlay;
        }

        public void Destroy()
        {
            Object.Destroy(Template);
            Object.Destroy(Drawn);
            Object.Destroy(Overlay);
        }
    }

    public sealed class ThumbnailCache
    {
        private sealed class Entry
        {
            public ThumbnailSet Set;
            public int Users;
            public long LastUsed;
        }

        private readonly Dictionary<int, Entry> _entries = new Dictionary<int, Entry>();
        private readonly OverlayImageBuilder _builder;
        private readonly int _capacity;
        private long _clock;

        public int Count => _entries.Count;

        public ThumbnailCache(OverlayImageBuilder builder, int capacity)
        {
            _builder = builder;
            _capacity = Mathf.Max(1, capacity);
        }

        public ThumbnailSet Acquire(AttemptRecord record)
        {
            if (!_entries.TryGetValue(record.Id, out var entry))
            {
                entry = new Entry
                {
                    Set = new ThumbnailSet(
                        _builder.CreateTexture(_builder.BuildTemplate(record)),
                        _builder.CreateTexture(_builder.BuildDrawn(record)),
                        _builder.CreateTexture(_builder.BuildOverlay(record))),
                };
                _entries.Add(record.Id, entry);
            }
            entry.Users++;
            entry.LastUsed = ++_clock;
            Trim();
            return entry.Set;
        }

        public void Release(int attemptId)
        {
            if (!_entries.TryGetValue(attemptId, out var entry)) return;
            entry.Users = Mathf.Max(0, entry.Users - 1);
            entry.LastUsed = ++_clock;
            Trim();
        }

        public void Clear()
        {
            foreach (var entry in _entries.Values) entry.Set.Destroy();
            _entries.Clear();
        }

        private void Trim()
        {
            while (_entries.Count > _capacity)
            {
                int oldestId = -1;
                long oldest = long.MaxValue;
                foreach (var pair in _entries)
                {
                    if (pair.Value.Users > 0 || pair.Value.LastUsed >= oldest) continue;
                    oldest = pair.Value.LastUsed;
                    oldestId = pair.Key;
                }
                if (oldestId < 0) return;
                _entries[oldestId].Set.Destroy();
                _entries.Remove(oldestId);
            }
        }
    }
}
