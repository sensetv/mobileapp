﻿using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using MvvmCross.Plugin.Color.Platforms.Ios;
using ObjCRuntime;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;
using static Toggl.Multivac.Math;

namespace Toggl.Daneel
{
    public sealed partial class ReportsPage : UIView
    {
        private readonly nfloat bluePercent = 0.42f;

        private readonly nfloat purplePercent = 0.25f;

        private readonly nfloat yellowPercent = 0.20f;

        private readonly nfloat grayPercent = 0.13f;

        private readonly CGColor blueColor = Color.Onboarding.BlueColor.ToNativeColor().CGColor;

        private readonly CGColor purpleColor = Color.Onboarding.PurpleColor.ToNativeColor().CGColor;

        private readonly CGColor yellowColor = Color.Onboarding.YellowColor.ToNativeColor().CGColor;

        private readonly CGColor grayColor = Color.Onboarding.GrayColor.ToNativeColor().CGColor;

        private CAShapeLayer blueSegment;

        private CAShapeLayer purpleSegment;

        private CAShapeLayer yellowSegment;

        private CAShapeLayer graySegment;

        public ReportsPage (IntPtr handle) : base (handle)
        {
        }

        public static ReportsPage Create()
        {
            var arr = NSBundle.MainBundle.LoadNib(nameof(ReportsPage), null, null);
            return Runtime.GetNSObject<ReportsPage>(arr.ValueAt(0));
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            blueSegment?.RemoveFromSuperLayer();
            purpleSegment?.RemoveFromSuperLayer();
            yellowSegment?.RemoveFromSuperLayer();
            graySegment?.RemoveFromSuperLayer();

            nfloat offset = 0;

            blueSegment = createChartSegment(offset, bluePercent, blueColor);
            offset += bluePercent;

            purpleSegment = createChartSegment(offset, purplePercent, purpleColor);
            offset += purplePercent;

            yellowSegment = createChartSegment(offset, yellowPercent, yellowColor);
            offset += yellowPercent;

            graySegment = createChartSegment(offset, grayPercent, grayColor);

            Layer.AddSublayer(blueSegment);
            Layer.AddSublayer(purpleSegment);
            Layer.AddSublayer(yellowSegment);
            Layer.AddSublayer(graySegment);
        }

        private CAShapeLayer createChartSegment(nfloat offset, nfloat percent, CGColor color)
        {
            var startAngle = (nfloat)FullCircle * offset - (nfloat)QuarterOfCircle;
            var angle = (nfloat)FullCircle * percent;
            var radius = Chart.Bounds.Width / 2;
            var start = new CGPoint(Chart.Center.X + (nfloat)Math.Cos(startAngle) * radius, Chart.Center.Y + (nfloat)Math.Sin(startAngle) * radius);

            var path = new UIBezierPath();
            path.MoveTo(Chart.Center);
            path.AddLineTo(start);
            path.AddArc(Chart.Center, radius, startAngle, startAngle + angle, true);
            path.ClosePath();

            var layer = new CAShapeLayer();
            layer.Path = path.CGPath;
            layer.FillColor = color;
            return layer;
        }
    }
}
