// 시도 번호 → 파일 이름 규칙. CSV·JSONL에 적는 이름과 실제 저장 이름이 항상 같도록 한 곳에서 만든다.
namespace VoxelMagic.SpellDrawing.DebugUI
{
    public static class AttemptFileNames
    {
        public const string Csv = "attempts.csv";
        public const string Jsonl = "attempts.jsonl";
        public const string PointsFolder = "points";
        public const string ImagesFolder = "images";

        public static string Prefix(int id) => $"attempt_{id:000}";
        public static string Points(int id) => $"{PointsFolder}/{Prefix(id)}.json";
        public static string TemplateImage(int id) => $"{ImagesFolder}/{Prefix(id)}_template.png";
        public static string DrawnImage(int id) => $"{ImagesFolder}/{Prefix(id)}_drawn.png";
        public static string OverlayImage(int id) => $"{ImagesFolder}/{Prefix(id)}_overlay.png";
    }
}
