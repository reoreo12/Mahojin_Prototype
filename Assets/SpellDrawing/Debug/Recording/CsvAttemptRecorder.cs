// attempts.csv에 시도 한 줄을 추가한다. 엑셀에서 한글이 깨지지 않도록 UTF-8 BOM으로 쓴다.
using System.IO;
using System.Text;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class CsvAttemptRecorder : FileAttemptRecorder
    {
        private static readonly Encoding Utf8WithBom = new UTF8Encoding(true);
        private readonly RecordSettings _settings;

        public CsvAttemptRecorder(RecordSettings settings, string sessionFolder) : base(sessionFolder)
        {
            _settings = settings;
        }

        protected override bool IsEnabled => _settings.WriteCsv;

        protected override void Write(AttemptRecord record)
        {
            string path = PathOf(AttemptFileNames.Csv);
            var sb = new StringBuilder();
            if (!File.Exists(path)) sb.AppendLine(AttemptLogEntry.CsvHeader);
            sb.AppendLine(AttemptLogEntry.From(record).ToCsvRow());
            File.AppendAllText(path, sb.ToString(), Utf8WithBom);
        }
    }
}
