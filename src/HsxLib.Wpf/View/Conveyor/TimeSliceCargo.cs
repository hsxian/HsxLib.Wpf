using HsxLib.Wpf.View.Conveyor;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ConveyorApp.View
{
    public class TimeSliceCargo : VariableGridCargo
    {
        private DateTime _start;
        private DateTime _end;
        private int TimeInterval { get; set; } = 10 * 60 * 1000;
        public double TickPixel { get; set; } = 20;

        public event Action<TimeSliceCargo, DateTime> OnTrayMove;

        public Canvas MainCvs { get; set; }

        public TimeSliceCargo()
        {
            MainCvs = new Canvas { VerticalAlignment = System.Windows.VerticalAlignment.Top };
            MainGrid.Children.Add(MainCvs);
            OnLeftBorderChanged += TimeSliceCargo_OnLeftBorderChanged;
            OnRightBorderChanged += TimeSliceCargo_OnRightBorderChanged; ;
        }

        private void TimeSliceCargo_OnRightBorderChanged(VariableGridCargo arg1, double arg2)
        {
            _end = _end + PixelToTimeSpan(arg2);
            SetVisualTime(_start, _end);
        }

        private void TimeSliceCargo_OnLeftBorderChanged(VariableGridCargo arg1, double arg2)
        {
            _start = _start + PixelToTimeSpan(arg2);
            SetVisualTime(_start, _end);
        }

        public override void OnTrayMoving(double cursorRelativeLeft)
        {
            SetVisualTime(_start, _end);
            if (cursorRelativeLeft < 0 || cursorRelativeLeft > EffectiveWidthPixel) return;
            var time = _start + PixelToTimeSpan(cursorRelativeLeft);
            OnTrayMove?.Invoke(this, time);
        }

        private double PixelToMillisecond(double pixel)
        {
            return pixel / TickPixel * TimeInterval;
        }

        private TimeSpan PixelToTimeSpan(double pixel)
        {
            return TimeSpan.FromMilliseconds(PixelToMillisecond(pixel));
        }

        private double MillisecondToPixel(double mill)
        {
            return mill / TimeInterval * TickPixel;
        }

        private double TimeSpanToPixel(TimeSpan ts)
        {
            return MillisecondToPixel(ts.TotalMilliseconds);
        }

        private void SetVisualTime(DateTime start, DateTime end)
        {
            for (int i = 0; i < MainCvs.Children.Count; i++)
            {
                if (MainCvs.Children[i] is FrameworkElement framework && framework.Name == "tick")
                {
                    MainCvs.Children.RemoveAt(i--);
                }
            }
            var start_1 = DateTime.MinValue + TimeSpan.FromMilliseconds((long)((start - DateTime.MinValue).TotalMilliseconds / TimeInterval) * TimeInterval + TimeInterval);
            var startTick = TimeSpanToPixel(start_1 - start);
            var fontMaxHeight = FontSize * 1.5;
            var topMM = Height - 10 - fontMaxHeight;
            var topHH = Height - 20 - fontMaxHeight;
            var topText = Height - 20;

            #region 只显示窗体可视范围内的时间刻度（防止刻度过多爆内存）

            var minLeft = CanvasLeft + startTick;
            if (minLeft < 0)
            {
                minLeft += (int)((-CanvasLeft) / TickPixel) * TickPixel;
            }
            var maxLeft = CanvasLeft + EffectiveWidthPixel;
            var winW = _window?.ActualWidth ?? SystemParameters.VirtualScreenWidth;
            if (maxLeft > winW)
            {
                maxLeft = winW;
            }

            #endregion 只显示窗体可视范围内的时间刻度（防止刻度过多爆内存）

            for (double absLeft = minLeft; absLeft < maxLeft; absLeft += TickPixel)
            {
                var left = absLeft - CanvasLeft;
                var curr = start + PixelToTimeSpan(left);
                bool isHour = curr.Minute == 0;

                AddTick(isHour ? 20 : 10, isHour ? topHH : topMM, left, isHour);
                AddText(curr.ToString(isHour ? "HH" : "mm"), topText, left, isHour);
            }
        }

        public void SetTime(DateTime start, DateTime end)
        {
            _start = start;
            _end = end;
            EffectiveWidthPixel = TimeSpanToPixel(end - start);
            Width = EffectiveWidthPixel;
            SetVisualTime(_start, _end);
        }

        private void AddTick(double height, double top, double left, bool isHour)
        {
            var rect = new Rectangle
            {
                Name = "tick",
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
                Name = "tick",
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