using HsxLib.Wpf.View.Conveyor;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ConveyorApp.View
{
    public class TimeSliceCargo : CargoBase
    {
        private DateTime _start;
        private DateTime _end;
        private double _totalMilliseconds;
        private int TimeInterval { get; set; } = 10 * 60 * 1000;
        public double TickPixel { get; set; } = 20;
        public override double EffectiveWidthPixel { get; protected set; }

        public event Action<TimeSliceCargo, DateTime> OnTrayMove;

        public Canvas MainCvs { get; set; }

        public TimeSliceCargo()
        {
            MainCvs = new Canvas { VerticalAlignment = System.Windows.VerticalAlignment.Top };
            AddChild(MainCvs);
        }

        public override void OnTrayMoving(double cursorRelativeLeft)
        {
            if (cursorRelativeLeft < 0 || cursorRelativeLeft > EffectiveWidthPixel) return;
            var mins = cursorRelativeLeft / EffectiveWidthPixel * _totalMilliseconds;
            var time = _start + TimeSpan.FromMilliseconds(mins);
            OnTrayMove?.Invoke(this, time);
        }

        public void SetTime(DateTime start, DateTime end)
        {
            _start = start;
            _end = end;
            _totalMilliseconds = (end - start).TotalMilliseconds;
            var start_1 = DateTime.MinValue + TimeSpan.FromMilliseconds((long)((start - DateTime.MinValue).TotalMilliseconds / TimeInterval) * TimeInterval + TimeInterval);
            var end_1 = DateTime.MinValue + TimeSpan.FromMilliseconds((long)((end - DateTime.MinValue).TotalMilliseconds / TimeInterval) * TimeInterval);
            var secs = (end_1 - start_1).TotalMilliseconds;
            EffectiveWidthPixel = (end - start).TotalMilliseconds / TimeInterval * TickPixel;
            Width = EffectiveWidthPixel;
            var startTick = (start_1 - start).TotalMilliseconds * TickPixel / TimeInterval;
            var count = 0;
            var fontMaxHeight = FontSize * 1.5;
            var topMM = Height - 10 - fontMaxHeight;
            var topHH = Height - 20 - fontMaxHeight;
            var topText = Height - 20;
            for (int i = 0; i < secs + 1; i += TimeInterval)
            {
                var curr = start_1.AddMilliseconds(i);
                bool isHour = curr.Minute == 0;

                AddTick(isHour ? 20 : 10, isHour ? topHH : topMM, startTick + count * TickPixel, isHour);
                AddText(curr.ToString(isHour ? "HH" : "mm"), topText, startTick + count * TickPixel, isHour);
                count++;
            }

            AddTick(30, 0, 0, true);
            AddTick(30, 0, EffectiveWidthPixel - 1, true);
        }

        private void AddTick(double height, double top, double left, bool isHour)
        {
            var rect = new Rectangle
            {
                Width = 1,
                Height = height,
                Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
            };
            Canvas.SetTop(rect, top);
            Canvas.SetLeft(rect, left);
            MainCvs.Children.Add(rect);
        }

        private void AddText(string text, double top, double left, bool isHour)
        {
            var tbk = new TextBlock
            {
                Text = text
            };
            Canvas.SetTop(tbk, top);
            if (isHour)
            {
                tbk.FontSize *= 1.5;
                tbk.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4b5cc4"));
                Canvas.SetLeft(tbk, left - text.Length * 3 * 1.5);
            }
            else
                Canvas.SetLeft(tbk, left - text.Length * 3);
            MainCvs.Children.Add(tbk);
        }
    }
}