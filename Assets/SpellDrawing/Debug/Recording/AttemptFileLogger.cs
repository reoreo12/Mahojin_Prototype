// 시도 결과 처리기(IAttemptResultHandler) 중 파일 기록 담당. 등록된 레코더를 차례로 실행한다.
using System.Collections.Generic;
using System.IO;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class AttemptFileLogger : IAttemptResultHandler
    {
        private readonly RecordSettings _settings;
        private readonly List<IAttemptRecorder> _recorders;

        public string SessionFolder { get; }

        public AttemptFileLogger(RecordSettings settings, string sessionFolder, IEnumerable<IAttemptRecorder> recorders)
        {
            _settings = settings;
            SessionFolder = sessionFolder;
            _recorders = new List<IAttemptRecorder>(recorders);
        }

        // 세션 폴더 = {persistentDataPath}/{folderName}/{세션 시작 시각}
        public static string CreateSessionFolderPath(string persistentDataPath, RecordSettings settings, System.DateTime start) =>
            Path.Combine(persistentDataPath, settings.FolderName, start.ToString("yyyy-MM-dd_HH-mm-ss"));

        public void Handle(AttemptRecord record)
        {
            if (!_settings.RecordEnabled) return;
            foreach (var recorder in _recorders) recorder.Record(record);
        }
    }
}
