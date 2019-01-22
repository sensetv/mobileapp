using CoreGraphics;
using Foundation;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Autocomplete
{
    public sealed class TagTextAttachment : TokenTextAttachment
    {
        private static readonly UIColor borderColor = Color.StartTimeEntry.TokenBorder.ToNativeColor();

        public TagTextAttachment(string tagName, UIColor color, UIFont font)
        {
            Image = drawToken(tagName, color, font);
        }

        private static UIImage drawToken(string tagName, UIColor color, UIFont font)
        {
            var stringToDraw = new NSAttributedString(
                tagName, new UIStringAttributes { Font = font, ForegroundColor = color });

            var size = CalculateSize(stringToDraw);

            UIGraphics.BeginImageContextWithOptions(size, false, 0.0f);
            using (var context = UIGraphics.GetCurrentContext())
            {
                var tokenPath = CalculateTokenPath(size);
                context.AddPath(tokenPath.CGPath);
                context.SetStrokeColor(borderColor.CGColor);
                context.StrokePath();

                var textOffset = (TokenHeight - font.LineHeight) / 2;
                stringToDraw.DrawString(new CGPoint(TokenMargin + TokenPadding, textOffset));

                var image = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                return image;
            }
        }
    }
}
