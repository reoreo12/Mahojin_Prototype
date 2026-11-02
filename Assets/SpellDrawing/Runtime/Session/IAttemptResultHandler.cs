// 시도가 끝났을 때 결과를 처리하는 곳. 이번에는 파일 기록, 나중에는 결과 분류·월드 이펙트를 여기에 붙인다.
namespace VoxelMagic.SpellDrawing
{
    public interface IAttemptResultHandler
    {
        void Handle(AttemptRecord record);
    }
}
