// attempts.jsonl에 시도 하나를 JSON 한 줄로 추가한다.
using System.IO;
using System.Text;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class JsonlAttemptRecorder : FileAttemptRecorder
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);
        private readonly RecordSettings _settings;

        public JsonlAttemptRecorder(RecordSettings settings, string sessionFolder) : base(sessionFolder)
        {
            _settings = settings;
        }

        protected override bool IsEnabled => _settings.WriteJsonl;

        protected override void Write(AttemptRecord record)
        {
            File.AppendAllText(PathOf(AttemptFileNames.Jsonl), AttemptLogEntry.From(record).ToJson() + "\n", Utf8);
        }
    }
}
