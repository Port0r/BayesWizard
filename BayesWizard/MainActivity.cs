using Android.App;
using Android.Widget;
using Android.OS;
using Java.Util;
using Android.Text;
using Java.Util.Regex;
using Android.Text.Style;
using Android.Graphics;
using Android.Views;
using System;
using Java.Lang;
using Android.Util;
using Android.Text.Method;
using Android.Views.InputMethods;

namespace BayesWizard
{
    enum CurrentEdit { Event, Test };

    [Activity(Label = "BayesWizard", MainLauncher = true, Theme = "@style/MyTheme", Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private string _assumption;
        private string _event;
        private string _test;
        private int _prior;
        private EditText _probEdit;
        private SeekBar _probSeek;
        private TextView _assumeTextView;
        private EditText _editor;
        private CurrentEdit _curEd = CurrentEdit.Event;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar1);
            //Toolbar will now take on default Action Bar characteristics
            SetActionBar(toolbar);
            //You can now use and reference the ActionBar
            ActionBar.Title = "Bayes' Wizard";

            _assumeTextView = FindViewById<TextView>(Resource.Id.theAssumption);
            _assumeTextView.SetBackgroundResource(Resource.Drawable.bubbleright);
            string[] theEvent = Resources.GetStringArray(Resource.Array.theEvent);
            string[] theTest = Resources.GetStringArray(Resource.Array.theTest);
            Java.Util.Random rand = new Java.Util.Random();
            int x = rand.NextInt(theEvent.Length);
            _event = theEvent[x];
            _test = theTest[x];
            _assumption = Resources.GetString(Resource.String.Iassume) + " [" + _event + "]" + Resources.GetString(Resource.String.because) + " [" + _test + "].";

            _editor = FindViewById<EditText>(Resource.Id.editText1);
            _editor.KeyPress += _editor_KeyPress;
            _editor.TextChanged += UpdateText;

            Button askButton = FindViewById<Button>(Resource.Id.askBayes);
            askButton.Click += AskButton_Click;

            Button contButton1 = FindViewById<Button>(Resource.Id.cont1);
            contButton1.Click += ContButton1_Click;

            _probSeek = FindViewById<SeekBar>(Resource.Id.seek1);
            _probSeek.ProgressChanged += SeekProb_ProgressChanged;
            _probEdit = FindViewById<EditText>(Resource.Id.prob1);
            _probEdit.KeyPress += EditProb_KeyPress;

            Refresh();
        }

        private void EditProb_KeyPress(object sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter)
            {
                InputMethodManager mgr = (InputMethodManager)GetSystemService(InputMethodService);
                mgr.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, 0);
                string t = ((EditText)sender).Text;
                SeekBar seekProb = _probSeek;

                if (t.Length > 0)
                {
                    _prior = int.Parse(t);
                    while (_prior > 99) { _prior = _prior / 10; };
                    _prior = System.Math.Max(System.Math.Min(_prior, 99), 1);
                    seekProb.Progress = _prior - 1;
                }
                else {
                    _prior = 1;
                    seekProb.Progress = _prior - 1;
                }
            }
            else
            {
                e.Handled = false;
            }
        }

        private void SeekProb_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            _prior = e.Progress+1;
            _probEdit.Text=_prior.ToString();
        }

        private string MirrorText(string text)
        {
            string[] words = text.Trim().Split(new char[] { ' ' });

            for(int i=0, len=words.Length;i<len;i++)
            {
                string w = words[i];
                switch (w)
                {
                    case "I": words[i] = "you"; break;
                    case "I'm": words[i] = "you're"; break;
                    case "am": words[i] = "are"; break;
                    case "mine": words[i] = "yours"; break;
                    case "my": words[i] = "your"; break;
                    case "me": words[i] = "you"; break;
                    case "I was": words[i] = "you were"; break;
                    case "wasn't": words[i] = "weren't"; break;
                }
            }

            return string.Join(" ", words);
        }

        private string PastText(string text)
        {
            string[] words = text.Trim().Split(new char[] { ' ' });

            for (int i = 0, len = words.Length; i < len; i++)
            {
                string w = words[i];
                switch (w)
                {
                    case "I'm": words[i] = "I was"; break;
                    case "am": words[i] = "was"; break;
                    case "is": words[i] = "was"; break;
                    case "has": words[i] = "had"; break;
                    case "have": words[i] = "had"; break;
                    case "haven't": words[i] = "had not"; break;
                    case "are": words[i] = "were"; break;
                    case "aren't": words[i] = "weren't"; break;
                }
            }

            return string.Join(" ", words);
        }

        private void ContButton1_Click(object sender, EventArgs e)
        {
            TextView w = FindViewById<TextView>(Resource.Id.withoutTest);
            TextView p = FindViewById<TextView>(Resource.Id.priorProbability);

            if (w.Visibility.Equals(ViewStates.Gone))
            {
                w.Visibility = ViewStates.Visible;
                _assumption = Resources.GetString(Resource.String.Iassume) + " " + _event + Resources.GetString(Resource.String.because) + " " + _test + ".";

                string question = Resources.GetString(Resource.String.withoutTest1) + " " + MirrorText(PastText(_test)) + Resources.GetString(Resource.String.withoutTest2) + " " + MirrorText(PastText(_event)) + " " + Resources.GetString(Resource.String.withoutTest3);
                w.Text = question;

                FindViewById<GridLayout>(Resource.Id.probability).Visibility = ViewStates.Visible;

                Refresh();
            }
            else if (p.Visibility.Equals(ViewStates.Gone))
            {
                p.Visibility = ViewStates.Visible;
                FindViewById<TextView>(Resource.Id.truePositive).Visibility = ViewStates.Visible;
                p.Text = Resources.GetString(Resource.String.withoutTestAnswer) + " " + _prior + " " + Resources.GetString(Resource.String.withoutTestAnswer2) + " " + PastText(_event) + ".";
                FindViewById<TextView>(Resource.Id.truePositive).Text= Resources.GetString(Resource.String.truePositive) + " " + MirrorText(_event) + Resources.GetString(Resource.String.truePositive2) + " " + MirrorText(PastText(_test)) + Resources.GetString(Resource.String.truePositive3);

                _probSeek.Progress = 49;
                _probEdit.Text = 50+"";
            }
        }

        private void AskButton_Click(object sender, EventArgs e)
        {
            FindViewById<TextView>(Resource.Id.heading1).Visibility=ViewStates.Gone;
            FindViewById<TextView>(Resource.Id.subHeading1).Visibility = ViewStates.Gone;
            FindViewById<Button>(Resource.Id.askBayes).Visibility = ViewStates.Gone;

            FindViewById<TextView>(Resource.Id.customize).Visibility = ViewStates.Visible;
            FindViewById<TextView>(Resource.Id.whatsup).Visibility = ViewStates.Visible;
            FindViewById<TextView>(Resource.Id.theAssumption).Visibility = ViewStates.Visible;
            FindViewById<Button>(Resource.Id.cont1).Visibility = ViewStates.Visible;
        }

        private void UpdateText(object sender, TextChangedEventArgs e)
        {
            if (_curEd.Equals(CurrentEdit.Event))
            {
                _event = _editor.Text;
            }
            else
            {
                _test = _editor.Text;
            }
            _assumption = Resources.GetString(Resource.String.Iassume) + " [" + _event + "]" + Resources.GetString(Resource.String.because) + " [" + _test + "].";

            Refresh();
        }

        private void _editor_KeyPress(object sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter)
            {
                InputMethodManager mgr = (InputMethodManager)GetSystemService(InputMethodService);
                mgr.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, 0);
                _editor.Visibility = ViewStates.Invisible;
            }
            else
            {
                e.Handled = false;
            }
        }

        private void Refresh()
        {
            initText(_assumption);
        }

        private void initText(string text)
        {
            Matcher matcher = Java.Util.Regex.Pattern.Compile("\\[(.*?)\\]").Matcher(text);
            int counter = 0, start=0, end=0;
            text = text.Replace("[", "");
            text = text.Replace("]", "");
            SpannableString spanString = new SpannableString(text);

            while (matcher.Find())
            {
                string tag = matcher.Group(1);

                start = matcher.Start() - counter;
                counter++;
                counter++;
                end = matcher.End() - counter;

                spanString.SetSpan(new ForegroundColorSpan(Color.ParseColor("#0000FF")),
                    start,
                    end,
                    0);

                MyClickableSpan clickableSpan = new MyClickableSpan();
                clickableSpan.Click += v =>
                {
                    Log.WriteLine(LogPriority.Debug, "click", "click: " + tag);
                    if (tag.Equals(_event)) _curEd = CurrentEdit.Event;
                    else _curEd = CurrentEdit.Test;

                    _editor.Text = tag;
                    _editor.Visibility = ViewStates.Visible;
                    _editor.SelectAll();
                    InputMethodManager mgr = (InputMethodManager)GetSystemService(InputMethodService);
                    mgr.ShowSoftInput(_editor, ShowFlags.Implicit);
                    _editor.Visibility = ViewStates.Visible;
                };
                spanString.SetSpan(clickableSpan, start, end, SpanTypes.ExclusiveExclusive);

            };

            _assumeTextView.TextFormatted = spanString;
            _assumeTextView.MovementMethod = new LinkMovementMethod();
        }
    }


    class MyClickableSpan : ClickableSpan
    {
        public Action<View> Click;

        public override void OnClick(View widget)
        {
            if (Click != null)
                Click(widget);
        }

    }
}


