using ConveyorApp.View;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ConveyorApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _ = Init();
        }

        private async Task Init()
        {
            AddTimeSlice(DateTime.Parse("2010/1/1 2:9:4"), DateTime.Parse("2010/1/1 5:9:4"), (Color)ColorConverter.ConvertFromString("#ffa631"));
            AddTimeSlice(DateTime.Parse("2010/1/10 22:59:4"), DateTime.Parse("2010/1/11 1:3:4"), (Color)ColorConverter.ConvertFromString("#afdd22"), 20);
            AddTimeSlice(DateTime.Parse("2011/1/10 7:59:4"), DateTime.Parse("2011/1/10 10:3:4"), (Color)ColorConverter.ConvertFromString("#ed5736"), 20);
            ConveyorTry.MinLeftPiexl = 20;
            ConveyorTry.MaxLeftPiexl = ConveyorTry.GetLeftOfBlank() - 30;
            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
            await Task.CompletedTask;
        }

        private void SetConveyorZero()
        {
            ConveyorTry.SetZero(ConveyorTry.ActualWidth / 4, false);
            Pointers.Margin = new Thickness(ConveyorTry.Zero, 0, 0, 0);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetConveyorZero();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetConveyorZero();
            ConveyorTry.ValidMoveCargo(ConveyorTry.Zero);
        }

        private void AddTimeSlice(DateTime start, DateTime end, Color color, double marginLeft = 0)
        {
            var time = new TimeSliceCargo { Background = new SolidColorBrush(color), Height = 100, CanvasLeft = ConveyorTry.GetLeftOfBlank() + marginLeft };
            time.SetTime(start, end);
            time.OnTrayMove += Time_OnTrayMove;
            ConveyorTry.AddCargos(time);
        }

        private async void Time_OnTrayMove(TimeSliceCargo sender, DateTime obj)
        {
            TimeTbk.Text = obj.ToString();

            await Task.CompletedTask;
        }
    }
}