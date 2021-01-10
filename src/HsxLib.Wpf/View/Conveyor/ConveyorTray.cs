using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace HsxLib.Wpf.View.Conveyor
{
    public class ConveyorTray : UserControl
    {
        protected Canvas TrayCvs { get; set; }
        public double MinLeftPiexl { get; set; }
        public double MaxLeftPiexl { get; set; }
        public double TotalMovePiexl { get; private set; }
        private Point _previousCvsMousePosition;
        private bool _isTrayCnsMouseDown;
        private double _trayCnsDeltaHorizontal;
        private double _trayCnsDeltaHorizontalLatest;
        public const int DefaultInertialMoveMaxCount = 90;
        private int _inertialMoveCount = DefaultInertialMoveMaxCount;
        private DispatcherTimer _dispatcherTimer;

        public double Zero { get; private set; }

        public void SetZero(double zero, bool isFixCargo)
        {
            var delat = zero - Zero;
            if (isFixCargo)
            {
                MoveCargo(delat, Zero);
            }
            else
            {
                ValidMoveCargo(delat);
            }
            Zero = zero;
        }

        public double GetLeftOfBlank()
        {
            var ret = .0;
            foreach (CargoBase item in TrayCvs.Children)
            {
                ret = Math.Max(ret, item.CanvasLeft + item.EffectiveWidthPixel);
            }
            return ret;
        }

        public ConveyorTray()
        {
            InitTrayCvs();
            InitDispatcherTimer();
        }

        private void InitDispatcherTimer()
        {
            _dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(3000 / DefaultInertialMoveMaxCount)
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine(nameof(DispatcherTimer_Tick) + DateTime.Now);
            InertialMove();
        }

        private void InitTrayCvs()
        {
            TrayCvs = new Canvas
            {
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            var win = Application.Current.MainWindow;
            var mouseOwner = TrayCvs;
            mouseOwner.MouseLeftButtonDown += TrayCns_MouseLeftButtonDown;
            mouseOwner.MouseWheel += TrayCns_MouseWheel;
            mouseOwner.MouseMove += TrayCns_MouseMove;
            mouseOwner.MouseLeftButtonUp += TrayCns_MouseLeftButtonUp;
            win.MouseLeave += TrayCvs_MouseLeave;
            AddChild(TrayCvs);
        }

        private void TrayCvs_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isTrayCnsMouseDown = false;
            StartInertialMove();
        }

        private void TrayCns_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            _isTrayCnsMouseDown = false;
            ValidMoveCargo(e.Delta);
        }

        private void TrayCns_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isTrayCnsMouseDown && sender is IInputElement element)
            {
                var p = e.GetPosition(element);
                _trayCnsDeltaHorizontalLatest = _trayCnsDeltaHorizontal = p.X - _previousCvsMousePosition.X;
                //Debug.WriteLine(_trayCnsDeltaHorizontalLatest);
                _previousCvsMousePosition = p;
                ValidMoveCargo(_trayCnsDeltaHorizontal);
            }
        }

        private void TrayCns_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isTrayCnsMouseDown = false;
            _trayCnsDeltaHorizontalLatest = _trayCnsDeltaHorizontal = _trayCnsDeltaHorizontal * 3;
            StartInertialMove();
        }

        private void TrayCns_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine(nameof(TrayCns_MouseLeftButtonDown) + DateTime.Now);
            _isTrayCnsMouseDown = true;
            _trayCnsDeltaHorizontal = 0;
            if (sender is IInputElement element)
            {
                _previousCvsMousePosition = e.GetPosition(element);
            }
        }

        private void InertialMove()
        {
            if (_inertialMoveCount-- > 1 && _trayCnsDeltaHorizontal != 0)
            {
                _trayCnsDeltaHorizontal -= _trayCnsDeltaHorizontal / _inertialMoveCount * 2;
                if (false == ValidMoveCargo(_trayCnsDeltaHorizontal))
                {
                    _trayCnsDeltaHorizontal = _inertialMoveCount = 0;
                }
            }
            else
            {
                _dispatcherTimer.Stop();
            }
        }

        private void StartInertialMove()
        {
            if (Math.Abs(_trayCnsDeltaHorizontalLatest) > 5 && _isTrayCnsMouseDown == false)
            {
                _inertialMoveCount = 0;
                _dispatcherTimer.Stop();
                _inertialMoveCount = DefaultInertialMoveMaxCount;
                _dispatcherTimer.Start();
            }
        }

        public void AddCargos<T>(params T[] cargos) where T : CargoBase
        {
            if (cargos == null) return;
            foreach (var item in cargos)
            {
                Canvas.SetTop(item, item.CanvasTop);
                Canvas.SetLeft(item, item.CanvasLeft);
                TrayCvs.Children.Add(item);
            }
        }

        private bool CanMove(double delta, double zero)
        {
            var newLeft = TotalMovePiexl - delta + zero;
            //Debug.WriteLine(newLeft);
            if (newLeft >= MinLeftPiexl && newLeft <= MaxLeftPiexl) return true;
            return false;
        }

        private void MoveCargo(double delta, double zero)
        {
            TotalMovePiexl -= delta;
            //Debug.WriteLine(TotalMovePiexl);
            foreach (CargoBase item in TrayCvs.Children)
            {
                if (item.EnableMove)
                {
                    var left = Canvas.GetLeft(item);
                    Canvas.SetLeft(item, left + delta);
                    item.OnTrayMoving(zero - left);
                }
            }
        }

        private double ExamineBorder(double zero)
        {
            // move + zero
            if (TotalMovePiexl < MinLeftPiexl - zero)
            {
                return TotalMovePiexl - MinLeftPiexl + zero;
            }
            if (TotalMovePiexl > MaxLeftPiexl - zero)
            {
                return TotalMovePiexl - MaxLeftPiexl + zero;
            }
            return 0;
        }

        public bool ValidMoveCargo(double delta)
        {
            var ret = false;
            //Debug.WriteLine(delta);
            if (CanMove(delta, Zero))
            {
                MoveCargo(delta, Zero);
                ret = true;
            }
            else
            {
                var border = ExamineBorder(Zero);
                if (border != 0)
                {
                    _inertialMoveCount = 0;
                    MoveCargo(border, Zero);
                }
            }
            return ret;
        }
    }
}