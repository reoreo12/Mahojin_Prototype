// 시도마다 원본·그린 것·겹쳐 보기 PNG 3장을 저장한다. 리스트 썸네일과 같은 그림이다.
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public sealed class ImageAttemptRecorder : FileAttemptRecorder
    {
        private readonly RecordSettings _settings;
        private readonly OverlayImageBuilder _builder;

        public ImageAttemptRecorder(RecordSettings settings, OverlayImageBuilder builder, string sessionFolder) : base(sessionFolder)
        {
            _settings = settings;
            _builder = builder;
        }

        protected override bool IsEnabled => _settings.WriteImages;

        protected override void Write(AttemptRecord record)
        {
            Save(AttemptFileNames.TemplateImage(record.Id), _builder.BuildTemplate(record));
            Save(AttemptFileNames.DrawnImage(record.Id), _builder.BuildDrawn(record));
            Save(AttemptFileNames.OverlayImage(record.Id), _builder.BuildOverlay(record));
        }

        private void Save(string relativePath, Color32[] pixels)
        {
            byte[] png = ImageConversion.EncodeArrayToPNG(pixels, GraphicsFormat.R8G8B8A8_SRGB,
                (uint)_builder.Size, (uint)_builder.Size);
            File.WriteAllBytes(PathOf(relativePath), png);
        }
    }
}
