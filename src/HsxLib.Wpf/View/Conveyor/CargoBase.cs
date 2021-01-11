using System.Windows;
using System.Windows.Controls;

namespace HsxLib.Wpf.View.Conveyor
{
    public abstract class CargoBase : UserControl
    {
        public double CanvasTop { get; set; }
        public double CanvasLeft { get; set; }
        public bool EnableMove { get; set; } = true;

        public abstract double EffectiveWidthPixel { get; protected set; }

        public abstract void OnTrayMoving(double cursorRelativePosition);
    }
}