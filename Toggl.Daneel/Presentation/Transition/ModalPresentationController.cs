using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Multivac;
using UIKit;
using static System.Math;
using Toggl.Daneel.ViewControllers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Toggl.Daneel.Presentation.Transition
{
    public sealed class ModalPresentationController : UIPresentationController
    {
        private readonly Action onDismissedCallback;
        private readonly UIImpactFeedbackGenerator feedbackGenerator;

        private readonly nfloat backgroundAlpha = 0.8f;
        private const double returnAnimationDuration = 0.1;
        private const double impactThreshold = 0.4;

        private CGPoint originalCenter;
        private double keyboardHeight;

        private List<NSObject> observers = new List<NSObject>();

        private nfloat maximumHeight
        {
            get
            {
                var distanceFromTop = UIApplication.SharedApplication.StatusBarFrame.Height;
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    distanceFromTop += UIApplication.SharedApplication.KeyWindow.SafeAreaInsets.Top;
                }
                return UIScreen.MainScreen.Bounds.Height - distanceFromTop;
            }
        }

        private readonly UIView dimmingView = new UIView
        {
            BackgroundColor = Foundation.MvvmCross.Helper.Color.ModalDialog.BackgroundOverlay.ToNativeColor(),
            Alpha = 0
        };

        public UIView AdditionalContentView { get; }
            = new UIView();

        public ModalPresentationController(UIViewController presentedViewController,
            UIViewController presentingViewController, Action onDismissedCallback)
          : base(presentedViewController, presentingViewController)
        {
            Ensure.Argument.IsNotNull(onDismissedCallback, nameof(onDismissedCallback));

            this.onDismissedCallback = onDismissedCallback;

            var recognizer = new UITapGestureRecognizer(() => dismiss());
            AdditionalContentView.AddGestureRecognizer(recognizer);

            var dismissBySwipingDown = new UIPanGestureRecognizer(handleSwipe);
            presentedViewController.View.AddGestureRecognizer(dismissBySwipingDown);

            feedbackGenerator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);

            observers.AddRange(new[]
            {
                NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillChangeStatusBarFrameNotification, onStatusBarFrameChanged),
                UIKeyboard.Notifications.ObserveDidShow(keyboardVisibilityChanged),
                UIKeyboard.Notifications.ObserveDidHide(keyboardVisibilityChanged)
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            observers.ForEach(NSNotificationCenter.DefaultCenter.RemoveObserver);
            observers.Clear();
        }

        public override void PresentationTransitionWillBegin()
        {
            dimmingView.Frame = ContainerView.Bounds;
            AdditionalContentView.Frame = ContainerView.Bounds;
            AdditionalContentView.Layer.ZPosition += 1;

            ContainerView.AddSubview(dimmingView);
            ContainerView.AddSubview(AdditionalContentView);

            var coordinator = PresentedViewController.GetTransitionCoordinator();
            if (coordinator == null)
            {
                dimmingView.Alpha = backgroundAlpha;
                return;
            }

            coordinator.AnimateAlongsideTransition(_ => dimmingView.Alpha = backgroundAlpha, null);
        }

        public override void DismissalTransitionWillBegin()
        {
            var coordinator = PresentedViewController.GetTransitionCoordinator();
            if (coordinator == null)
            {
                dimmingView.Alpha = 0.0f;
                return;
            }

            coordinator.AnimateAlongsideTransition(_ => dimmingView.Alpha = 0.0f, null);
        }

        public override void DismissalTransitionDidEnd(bool completed)
        {
            base.DismissalTransitionDidEnd(completed);

            if (completed)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(UIApplication.WillChangeStatusBarFrameNotification);
            }
        }

        public override void ContainerViewWillLayoutSubviews()
        {
            PresentedView.Frame = FrameOfPresentedViewInContainerView;

            var shadowPath = UIBezierPath.FromRoundedRect(PresentedViewController.View.Bounds, 8.0f).CGPath;
            PresentedViewController.View.Layer.ShadowRadius = 20;
            PresentedViewController.View.Layer.CornerRadius = 8.0f;
            PresentedViewController.View.Layer.ShadowOpacity = 0.8f;
            PresentedViewController.View.Layer.MasksToBounds = false;
            PresentedViewController.View.Layer.ShadowPath = shadowPath;
            PresentedViewController.View.Layer.ShadowOffset = CGSize.Empty;
            PresentedViewController.View.Layer.ShadowColor = UIColor.FromRGB(181f / 255f, 188f / 255f, 192f / 255f).CGColor;
        }

        public override CGSize GetSizeForChildContentContainer(IUIContentContainer contentContainer, CGSize parentContainerSize)
        {
            var preferredHeight = Min(maximumHeight, PresentedViewController.PreferredContentSize.Height);
            return new CGSize(parentContainerSize.Width, preferredHeight == 0 ? maximumHeight : preferredHeight);
        }

        public override CGRect FrameOfPresentedViewInContainerView
        {
            get
            {
                if (ContainerView == null)
                    return CGRect.Empty;

                var containerSize = ContainerView.Bounds.Size;
                var frame = CGRect.Empty;
                frame.Size = GetSizeForChildContentContainer(PresentedViewController, containerSize);
                frame.X = 0;
                frame.Y = containerSize.Height - frame.Size.Height;
                return frame;
            }
        }

        private async void handleSwipe(UIPanGestureRecognizer recognizer)
        {
            var translation = recognizer.TranslationInView(recognizer.View);
            var height = FrameOfPresentedViewInContainerView.Size.Height - keyboardHeight;
            var percent = (nfloat)Max(0, translation.Y / height);

            switch (recognizer.State)
            {
                case UIGestureRecognizerState.Began:
                    originalCenter = recognizer.View.Center;
                    feedbackGenerator.Prepare();
                    break;
                case UIGestureRecognizerState.Changed:
                    var center = new CGPoint(originalCenter.X, Max(originalCenter.Y, originalCenter.Y + translation.Y));
                    recognizer.View.Center = center;
                    dimmingView.Alpha = backgroundAlpha * (1 - percent);
                    break;
                case UIGestureRecognizerState.Ended:
                case UIGestureRecognizerState.Cancelled:
                    if (percent > impactThreshold && await dismiss())
                    {
                        feedbackGenerator.ImpactOccurred();
                    }
                    else
                    {
                        resetPosition(recognizer);
                    }
                    break;
            }
        }

        private async Task<bool> dismiss()
        {
            if (PresentedViewController is IDismissableViewController dismissableViewController)
            {
                if (await dismissableViewController.Dismiss() == false)
                    return false;
            }

            PresentedViewController.DismissViewController(true, null);
            onDismissedCallback();
            return true;
        }

        private void resetPosition(UIGestureRecognizer recognizer)
        {
            UIView.Animate(returnAnimationDuration, () =>
            {
                dimmingView.Alpha = backgroundAlpha;
                recognizer.View.Center = originalCenter;
            });
        }

        private void onStatusBarFrameChanged(NSNotification notification)
        {
            ContainerView?.SetNeedsLayout();
        }

        private void keyboardVisibilityChanged(object sender, UIKeyboardEventArgs e)
        {
            keyboardHeight = e.FrameEnd.Size.Height;
        }
    }
}
