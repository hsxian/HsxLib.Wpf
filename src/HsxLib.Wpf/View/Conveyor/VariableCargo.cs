﻿using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HsxLib.Wpf.View.Conveyor
{
    public class VariableCargo : CargoBase
    {
        public override double EffectiveWidthPixel { get; protected set; }
        private Grid _mainGrid;
        private Rectangle _leftRectangle;
        private Rectangle _rightRectangle;
        private Rectangle _rectangleMouseDown;
        private Window _window;
        private Point _previousMousePoint;

        public VariableCargo()
        {
            _mainGrid = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            AddChild(_mainGrid);
            _leftRectangle = CreatCommonRectangle();
            _leftRectangle.HorizontalAlignment = HorizontalAlignment.Left;
            _rightRectangle = CreatCommonRectangle();
            _rightRectangle.HorizontalAlignment = HorizontalAlignment.Right;
            _mainGrid.Children.Add(_leftRectangle);
            _mainGrid.Children.Add(_rightRectangle);

            _ = TryFindWin();
        }

        private async Task TryFindWin()
        {
            await Task.Delay(100);
            _window = EMA.ExtendedWPFVisualTreeHelper.WPFVisualFinders.FindParent<Window>(this);
            _window.MouseMove += Window_MouseMove;
            _window.MouseUp += Window_MouseUp;
            _window.MouseLeave += Window_MouseLeave;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            _rectangleMouseDown = null;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _rectangleMouseDown = null;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_rectangleMouseDown != null)
            {
                var p = Mouse.GetPosition(_window);
                var delta = p.X - _previousMousePoint.X;

                if (_rectangleMouseDown == _leftRectangle)
                {
                    var w = Width - delta;
                    if (w <= 0) return;
                    Width = w;
                    var tray = EMA.ExtendedWPFVisualTreeHelper.WPFVisualFinders.FindParent<ConveyorTray>(this);
                    if (tray != null)
                    {
                        tray.MoveCargo(this, delta, tray.OriginPosition);
                    }
                }
                else if (_rectangleMouseDown == _rightRectangle)
                {
                    var w = Width + delta;
                    if (w <= 0) return;
                    Width = w;
                }
                _previousMousePoint = p;
            }
        }

        private Rectangle CreatCommonRectangle()
        {
            var ret = new Rectangle
            {
                Width = 2,
                VerticalAlignment = VerticalAlignment.Stretch,
                Cursor = Cursors.SizeWE,
                Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0))
            };
            ret.MouseLeftButtonDown += Ret_MouseLeftButtonDown;
            return ret;
        }

        private void Ret_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rectangle)
            {
                e.Handled = true;
                _rectangleMouseDown = rectangle;
                _previousMousePoint = Mouse.GetPosition(_window);
            }
        }

        public override void OnTrayMoving(double cursorRelativePosition)
        {
        }
    }
}