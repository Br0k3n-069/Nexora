using System;
using System.Windows;
using System.Windows.Media;

namespace NEXORA.Services
{
    public static class ThemeService
    {
        public static void SetAccent(Color color)
        {
            SetBrush("AccentBrush", color);
            SetBrush("AccentSoftBrush", Color.FromArgb(51, color.R, color.G, color.B));
        }

        public static void SetPanelOpacity(double opacity)
        {
            byte alpha = (byte)Math.Max(90, Math.Min(245, opacity * 255));
            SetBrush("PanelBrush", Color.FromArgb(alpha, 26, 32, 36));
        }

        private static void SetBrush(string key, Color color)
        {
            if (Application.Current == null)
                return;

            // Replace the resource instead of mutating it: WPF may freeze shared brushes.
            Application.Current.Resources[key] = new SolidColorBrush(color);
        }
    }
}
