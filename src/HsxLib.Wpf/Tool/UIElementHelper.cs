using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HsxLib.Wpf.Tool
{
    public static class UIElementHelper
    {
        public static List<T> Find<T>(this UIElementCollection collection)
        {
            var ret = new List<T>();
            foreach (var item in collection)
            {
                if (item is T t)
                {
                    ret.Add(t);
                }
            }
            return ret;
        }
    }
}