
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using ShareX.ScreenCaptureLib.Properties;
using System.Drawing;

namespace ShareX.ScreenCaptureLib
{
    public class HighlightEffectShape : BaseEffectShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.EffectHighlight;

        public override string OverlayText => Resources.Highlight;

        public Color HighlightColor { get; set; }

        public override void OnConfigLoad()
        {
            HighlightColor = AnnotationOptions.HighlightColor;
        }

        public override void OnConfigSave()
        {
            AnnotationOptions.HighlightColor = HighlightColor;
        }

        public override void ApplyEffect(Bitmap bmp)
        {
            ImageHelpers.HighlightImage(bmp, HighlightColor);
        }
    }
}