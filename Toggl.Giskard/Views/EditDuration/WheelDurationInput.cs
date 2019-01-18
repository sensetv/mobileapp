using System;
using System.Linq;
using System.Globalization;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using Toggl.Foundation;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Views.EditDuration.Shapes;

namespace Toggl.Giskard.Views.EditDuration
{
    public static class WheelDurationInputExtensions
    {
        public static string AsDurationString(this TimeSpan value)
            => $"{(int)value.TotalHours}:{value.Minutes.ToString("D2", CultureInfo.InvariantCulture)}:{value.Seconds.ToString("D2", CultureInfo.InvariantCulture)}";
    }

    [Register("toggl.giskard.views.WheelDurationInput")]
    public class WheelDurationInput : EditText
    {
        private TimeSpan originalDuration;

        private TimeSpan duration;

        private DurationFieldInfo input = DurationFieldInfo.Empty;

        private bool isEditing = true;

        public event EventHandler DurationChanged;

        public TimeSpan Duration
        {
            get => duration;
            set
            {
                if (duration == value)
                    return;

                duration = value;

                if (isEditing == false)
                    showCurrentDuration();
            }
        }

        public WheelDurationInput(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            setupInputMode();
        }

        public WheelDurationInput(Context context) : base(context)
        {
            setupInputMode();
        }

        public WheelDurationInput(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            setupInputMode();
        }

        public WheelDurationInput(Context context, IAttributeSet attrs, int defStyleRes) : base(context, attrs, defStyleRes)
        {
            setupInputMode();
        }

        public WheelDurationInput(Context context, IAttributeSet attrs, int defStyleAttrs, int defStyleRes) : base(context, attrs, defStyleAttrs, defStyleRes)
        {
            setupInputMode();
        }

        private void setupInputMode()
        {
            TransformationMethod = null;

            var filter = new InputFilter();
            filter.onDigit += onDigitEntered;
            filter.onDelete += onDeleteEntered;

            SetFilters(new IInputFilter[] { filter });
        }

        private void showCurrentDuration()
        {
            Text = duration.AsDurationString();
            System.Diagnostics.Debug.WriteLine($"showCurrentDuration {duration} : {Text}");
        }

        protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, Android.Graphics.Rect previouslyFocusedRect)
        {
            System.Diagnostics.Debug.WriteLine($"OnFocusChanged {gainFocus}");

            if (gainFocus)
            {
                System.Diagnostics.Debug.WriteLine($"originalDuration <-- {duration}");
                isEditing = true;
                originalDuration = duration;
                input = DurationFieldInfo.Empty;
                Text = input.ToString();
                System.Diagnostics.Debug.WriteLine($"Text <-- {Text}");
                moveCursorToEnd();
            }
            else
            {
                isEditing = false;
                showCurrentDuration();
                DurationChanged?.Invoke(this, null);
            }

            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
        }

        private void tryUpdate(DurationFieldInfo nextInput)
        {
            System.Diagnostics.Debug.WriteLine($"Trying update.");
            if (nextInput.Equals(input))
                return;
            System.Diagnostics.Debug.WriteLine($"input <-- {nextInput}");

            input = nextInput;

            Duration = input.IsEmpty ? originalDuration : input.ToTimeSpan();
            System.Diagnostics.Debug.WriteLine($"Duration <-- {Duration}");

            Text = input.ToString();
            System.Diagnostics.Debug.WriteLine($"Text <-- {Text}");
        }

        public override void OnEditorAction(ImeAction actionCode)
        {
            if (actionCode == ImeAction.Done || actionCode == ImeAction.Next)
            {
                System.Diagnostics.Debug.WriteLine($"DONE");
                this.RemoveFocus();
                DurationChanged?.Invoke(this, null);
            }
        }

        protected override void OnTextChanged(ICharSequence text, int start, int lengthBefore, int lengthAfter)
        {
            System.Diagnostics.Debug.WriteLine($"OnTextChanged");
            if (isEditing)
            {
                if (Text != input.ToString())
                {
                    Text = input.ToString();
                    System.Diagnostics.Debug.WriteLine($"Text <-- input ({input})");
                }
            }
            else
            {
                if (Text != text.ToString())
                {
                    Text = text.ToString();
                    System.Diagnostics.Debug.WriteLine($"Text <-- text ({text})");
                }
            }

            moveCursorToEnd();
        }

        private ICharSequence formatString(string text)
        {
            System.Diagnostics.Debug.WriteLine($"TEXT: {text}");

            var segments = text.Split(':');

            if (segments.Length == 2)
            {
                var length = text.TakeWhile(c => c == '0' || c == ':').Count();
                var partA = text.Substring(0, length);
                var partB = text.Substring(length, text.Length - length);

                System.Diagnostics.Debug.WriteLine($"[{partA} | {partB}]");

                var spannable = new SpannableStringBuilder();

                spannable.Append(
                    partA,
                    new ForegroundColorSpan(Color.Gray),
                    SpanTypes.InclusiveInclusive);

                spannable.Append(
                    partB,
                    new ForegroundColorSpan(Color.Gray),
                    SpanTypes.InclusiveInclusive);

                return spannable;
            }
            else if (segments.Length == 3)
            {
                var indexOfLastColon = text.LastIndexOf(':');
                var partA = text.Substring(0, indexOfLastColon);
                var partB = text.Substring(indexOfLastColon, text.Length - indexOfLastColon);

                System.Diagnostics.Debug.WriteLine($"[{partA} | {partB}]");

                var spannable = new SpannableStringBuilder();

                spannable.Append(
                    text.Substring(0, indexOfLastColon),
                    new ForegroundColorSpan(Color.Gray),
                     SpanTypes.InclusiveInclusive);

                spannable.Append(
                    text.Substring(indexOfLastColon, text.Length - indexOfLastColon),
                    new ForegroundColorSpan(Color.Gray),
                    SpanTypes.InclusiveInclusive);

                return spannable;
            }

            return text.AsCharSequence();
        }

        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (Text == value)
                    return;

                TextFormatted = formatString(value);
            }
        } 

        private void onDeleteEntered()
        {
            var nextInput = input.Pop();
            tryUpdate(nextInput);
        }

        private void onDigitEntered(int digit)
        {
            var nextInput = input.Push(digit);
            tryUpdate(nextInput);
        }

        protected override void OnSelectionChanged(int selStart, int selEnd)
        {
            moveCursorToEnd();
        }

        private void moveCursorToEnd()
        {
            SetSelection(Text.Length);
        }

        private class InputFilter : Java.Lang.Object, IInputFilter
        {
            public event Action<int> onDigit;
            public event Action onDelete;

            public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
            {
                var empty = string.Empty.AsJavaString();
                var sourceLength = source.Length();

                if (sourceLength > 1)
                    return source.ToString().AsJavaString();

                if (sourceLength == 0)
                {
                    onDelete?.Invoke();
                    return empty;
                }

                var lastChar = source.CharAt(sourceLength - 1);

                if (char.IsDigit(lastChar))
                {
                    int digit = int.Parse(lastChar.ToString());
                    onDigit?.Invoke(digit);

                    return digit.ToString().AsJavaString();
                }

                return empty;
            }
        }
    }
}
