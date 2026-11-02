// 세션 요약: 시도 수, 평균·최고·최저 유사도. 값이 바뀌면 이벤트로 알린다.
using System;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public sealed class SessionStats
    {
        private float _sum;

        public event Action Changed;

        public int Count { get; private set; }
        public float Average => Count > 0 ? _sum / Count : 0f;
        public float Best { get; private set; }
        public float Worst { get; private set; }

        public void Add(AttemptRecord record)
        {
            float similarity = record.Similarity;
            Best = Count == 0 ? similarity : Mathf.Max(Best, similarity);
            Worst = Count == 0 ? similarity : Mathf.Min(Worst, similarity);
            _sum += similarity;
            Count++;
            Changed?.Invoke();
        }

        public void Clear()
        {
            _sum = 0f;
            Count = 0;
            Best = 0f;
            Worst = 0f;
            Changed?.Invoke();
        }
    }
}
