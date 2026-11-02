// StrokeRasterizer 테스트: 선 위 픽셀은 칠하고 멀리 떨어진 픽셀은 건드리지 않는다.
using NUnit.Framework;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.Tests
{
    public sealed class StrokeRasterizerTests
    {
        [Test]
        public void DrawsSegmentPixelsOnly()
        {
            var raster = new StrokeRasterizer(32, 32);
            raster.Clear(new Color32(0, 0, 0, 0));
            raster.DrawSegment(new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f), 2f, new Color32(255, 0, 0, 255));
            Assert.AreEqual(255, raster.Pixels[16 * 32 + 16].r);
            Assert.AreEqual(0, raster.Pixels[2 * 32 + 16].a);
        }
    }
}
