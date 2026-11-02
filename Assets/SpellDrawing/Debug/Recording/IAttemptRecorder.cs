// 시도 하나를 한 가지 형식(CSV, JSONL, 점 JSON, PNG 등)으로 저장한다.
namespace VoxelMagic.SpellDrawing.DebugUI
{
    public interface IAttemptRecorder
    {
        void Record(AttemptRecord record);
    }
}
