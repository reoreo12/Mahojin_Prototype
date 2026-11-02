// 그린 모양과 원본 모양을 비교해 0~1 유사도를 내는 계산 방식. 알고리즘을 바꿀 때 이 인터페이스로 교체한다.
namespace VoxelMagic.SpellDrawing
{
    public interface ISimilarityMetric
    {
        SimilarityScore Evaluate(StrokeSet drawn, StrokeSet template, float tolerance);
    }
}
