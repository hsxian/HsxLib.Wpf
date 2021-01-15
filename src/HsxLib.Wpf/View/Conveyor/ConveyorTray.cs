using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
        public UIElementCollection Cargos => TrayCvs.Children;
        public double TotalMovePiexl { get; private set; }
        private Point _previousCvsMousePosition;
        private bool _isTrayCnsMouseDown;
        private double _trayCnsDeltaHorizontal;
        private double _trayCnsDeltaHorizontalLatest;
        public const int DefaultInertialMoveMaxCount = 90;
        private int _inertialMoveCount = DefaultInertialMoveMaxCount;
        private DispatcherTimer _dispatcherTimer;

        public double OriginPosition { get; private set; }

        public void SetZero(double origin, bool isFixCargo)
        {
            var delat = origin - OriginPosition;
            if (isFixCargo)
            {
                MoveCargos(delat, OriginPosition);
            }
            else
            {
                ValidMoveCargos(delat);
            }
            OriginPosition = origin;
        }

        public double GetPositionOfRightBlank()
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
            var mouseOwner = TrayCvs;
            mouseOwner.MouseLeftButtonDown += TrayCns_MouseLeftButtonDown;
            mouseOwner.MouseWheel += TrayCns_MouseWheel;
            mouseOwner.MouseMove += TrayCns_MouseMove;
            mouseOwner.MouseLeftButtonUp += TrayCns_MouseLeftButtonUp;
            AddChild(TrayCvs);
            _ = TryFindWinThenInit();
        }

        private async Task TryFindWinThenInit()
        {
            await Task.Delay(100);
            var win = EMA.ExtendedWPFVisualTreeHelper.WPFVisualFinders.FindParent<Window>(this) as FrameworkElement ?? this;
            win.MouseLeave += TrayCvs_MouseLeave;
        }

        private void TrayCvs_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isTrayCnsMouseDown = false;
            StartInertialMove();
        }

        private void TrayCns_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            _isTrayCnsMouseDown = false;
            ValidMoveCargos(e.Delta);
        }

        private void TrayCns_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isTrayCnsMouseDown && sender is IInputElement element)
            {
                var p = e.GetPosition(element);
                _trayCnsDeltaHorizontalLatest = _trayCnsDeltaHorizontal = p.X - _previousCvsMousePosition.X;
                //Debug.WriteLine(_trayCnsDeltaHorizontalLatest);
                _previousCvsMousePosition = p;
                ValidMoveCargos(_trayCnsDeltaHorizontal);
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
                if (false == ValidMoveCargos(_trayCnsDeltaHorizontal))
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
                item.Tray = this;
                Canvas.SetTop(item, item.CanvasTop);
                Canvas.SetLeft(item, item.CanvasLeft);
                TrayCvs.Children.Add(item);
            }
        }

        private bool CanMove(double delta, double origin)
        {
            var newLeft = TotalMovePiexl - delta + origin;
            //Debug.WriteLine(newLeft);
            if (newLeft >= MinLeftPiexl && newLeft <= MaxLeftPiexl) return true;
            return false;
        }

        private void MoveCargos(double delta, double origin)
        {
            TotalMovePiexl -= delta;
            //Debug.WriteLine(TotalMovePiexl);
            foreach (CargoBase item in TrayCvs.Children)
            {
                if (item.EnableMove)
                {
                    MoveCargo(item, delta, origin);
                }
            }
        }

        public void MoveCargo(CargoBase cargo, double delta, double origin)
        {
            var left = Canvas.GetLeft(cargo);
            Canvas.SetLeft(cargo, left + delta);
            cargo.CanvasLeft += delta;
            cargo.OnTrayMoving(origin - left);
        }

        private double ExamineBorder(double origin)
        {
            if (TotalMovePiexl < MinLeftPiexl - origin)
            {
                return TotalMovePiexl - MinLeftPiexl + origin;
            }
            if (TotalMovePiexl > MaxLeftPiexl - origin)
            {
                return TotalMovePiexl - MaxLeftPiexl + origin;
            }
            return 0;
        }

        public bool ValidMoveCargos(double delta)
        {
            var ret = false;
            //Debug.WriteLine(delta);
            if (CanMove(delta, OriginPosition))
            {
                MoveCargos(delta, OriginPosition);
                ret = true;
            }
            else
            {
                var border = ExamineBorder(OriginPosition);
                if (border != 0)
                {
                    _inertialMoveCount = 0;
                    MoveCargos(border, OriginPosition);
                }
            }
            return ret;
        }
    }
}