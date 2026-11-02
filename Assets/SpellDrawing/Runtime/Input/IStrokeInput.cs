// 획 입력 장치 추상화. 누름·이동·뗌을 화면 좌표(픽셀)로 알린다. 마우스 외에 펜·터치를 같은 방식으로 붙인다.
using System;
using UnityEngine;

namespace VoxelMagic.SpellDrawing
{
    public interface IStrokeInput
    {
        event Action<Vector2> StrokeBegan;
        event Action<Vector2> StrokeMoved;
        event Action<Vector2> StrokeEnded;
    }
}
