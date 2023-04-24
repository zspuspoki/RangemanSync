using System.ComponentModel;

using Android.Widget;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Android.Text.Method;
using Android.Content;
using EntryDecimal.Droid;
using EntryDecimal;

[assembly: ExportRenderer(typeof(MyCustomEntry), typeof(AndroidCustomEntryRenderer))]
namespace EntryDecimal.Droid
{
    public class AndroidCustomEntryRenderer : EntryRenderer
    {
        private MyCustomEntry element;
        private EditText native;

        public AndroidCustomEntryRenderer(Context context) : base(context)
        { }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            //Java.Text.DecimalFormatSymbols.Instance.DecimalSeparator = '.';

            element = (MyCustomEntry)Element ?? null;
            native = Control as EditText;

            UpdateKeyboard();
        }


        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control == null) return;

            else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
                UpdateKeyboard();
        }

        private void UpdateKeyboard()
        {
            //Implementation of the numeric keyboard (we simply add the NumberFlagSigned)
            native = Control as EditText;

            var defaultNumericKeyboard = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
            var correnctNumericKeyboard = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagSigned | Android.Text.InputTypes.NumberFlagDecimal;

            if (native.InputType == defaultNumericKeyboard)
            {
                native.InputType =
                Android.Text.InputTypes.ClassNumber |
                Android.Text.InputTypes.NumberFlagSigned |
                Android.Text.InputTypes.NumberFlagDecimal;
                native.KeyListener = DigitsKeyListener.GetInstance(string.Format("7890.-"));
            }
            else if (native.InputType == correnctNumericKeyboard)
            {
                // Even though in the next line the InputType is set to the same it is already set, this seems to 
                // fix the problem with the decimal separator: Namely, a local other than english is set, the point 
                // does not have any effect in the numeric keyboard. Re-setting the InputType seems to fix this.
                native.InputType =
                Android.Text.InputTypes.ClassNumber |
                Android.Text.InputTypes.NumberFlagSigned |
                Android.Text.InputTypes.NumberFlagDecimal;
            }
        }

    }
}