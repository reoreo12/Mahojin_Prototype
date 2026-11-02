// 파일 레코더의 공통 흐름: 켜져 있는지 확인 → 세션 폴더 준비 → Write → 실패하면 경고만 남기고 게임은 계속.
using System;
using System.IO;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public abstract class FileAttemptRecorder : IAttemptRecorder
    {
        protected string SessionFolder { get; }

        protected FileAttemptRecorder(string sessionFolder)
        {
            SessionFolder = sessionFolder;
        }

        protected abstract bool IsEnabled { get; }

        public void Record(AttemptRecord record)
        {
            if (!IsEnabled) return;
            try
            {
                Directory.CreateDirectory(SessionFolder);
                Write(record);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[SpellDrawing] {GetType().Name} 저장 실패: {e.Message}");
            }
        }

        protected abstract void Write(AttemptRecord record);

        protected string PathOf(string relativePath)
        {
            string path = Path.Combine(SessionFolder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }
    }
}
