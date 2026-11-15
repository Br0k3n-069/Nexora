using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NEXORA.Models;
using NEXORA.Services;

namespace NEXORA
{
    public partial class MainWindow : Window
    {
        private readonly ProfileStore _profileStore = new ProfileStore();
        private readonly SubscriptionStore _subscriptionStore = new SubscriptionStore();
        private readonly DemoConnectionEngine _connectionEngine = new DemoConnectionEngine();
        private readonly System.Collections.ObjectModel.ObservableCollection<Profile> _profiles;

        private readonly Dictionary<string, FrameworkElement> _pages;

        public MainWindow()
        {
            InitializeComponent();

            _profiles = _profileStore.Load();
            ProfilesList.ItemsSource = _profiles;
            SubscriptionsList.ItemsSource = _subscriptionStore.Sources;
            SubscriptionNodesList.ItemsSource = _subscriptionStore.Nodes;

            _pages = new Dictionary<string, FrameworkElement>
            {
                { "Dashboard", DashboardPage },
                { "Connections", ConnectionsPage },
                { "Subscriptions", SubscriptionsPage },
                { "Profiles", ProfilesPage },
                { "Theme", ThemePage },
                { "Layout", LayoutPage },
                { "Animations", AnimationsPage },
                { "Settings", SettingsPage }
            };

            var os = Environment.OSVersion;
            var label = "Windows " + os.Version.Major + "." + os.Version.Minor;
            OsBadge.Text = label + " • " + (Environment.Is64BitOperatingSystem ? "x64" : "x86");
            CompatibilityText.Text = label + ". The app targets .NET Framework 4.8 / WPF. Windows 7 requires SP1 and .NET Framework 4.8 installed.";
        }

        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            Navigate(button.Tag as string);
        }

        private void Navigate(string key)
        {
            foreach (var page in _pages.Values) page.Visibility = Visibility.Collapsed;
            FrameworkElement selected;
            if (!_pages.TryGetValue(key, out selected)) return;
            selected.Visibility = Visibility.Visible;
            selected.Opacity = 0;
            if (AnimationsEnabled != null && AnimationsEnabled.IsChecked == true && (ReducedMotion == null || ReducedMotion.IsChecked != true))
            {
                var duration = AnimationDuration == null ? 240 : (int)AnimationDuration.Value;
                var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(duration));
                animation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                selected.BeginAnimation(UIElement.OpacityProperty, animation);
            }
            else selected.Opacity = 1;

            var titles = new Dictionary<string, string>
            {
                { "Dashboard", "Dashboard" }, { "Connections", "Connections" },
                { "Profiles", "Profiles" }, { "Subscriptions", "Subscriptions" }, { "Theme", "Theme Studio" },
                { "Layout", "Layout Editor" }, { "Animations", "Animations" },
                { "Settings", "Settings" }
            };
            PageTitle.Text = titles[key];
            PageSubtitle.Text = key == "Dashboard" ? "Your workspace at a glance" : "NEXORA workspace";
        }

        private void OpenConnections_Click(object sender, RoutedEventArgs e) { Navigate("Connections"); }
        private void OpenTheme_Click(object sender, RoutedEventArgs e) { Navigate("Theme"); }
        private void OpenLayout_Click(object sender, RoutedEventArgs e) { Navigate("Layout"); }

        private void AddProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProfileDialog { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                _profiles.Add(new Profile { Name = dialog.ProfileName, Address = dialog.ProfileAddress });
                _profileStore.Save(_profiles);
                ProfilesList.SelectedIndex = _profiles.Count - 1;
            }
        }

        private void ProfilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var profile = ProfilesList.SelectedItem as Profile;
            if (profile != null)
            {
                ActiveProfileText.Text = profile.Name;
                PageSubtitle.Text = "Selected profile: " + profile.Name;
            }
        }

        private void AccentPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccentPicker == null || AccentPicker.SelectedIndex < 0) return;
            Color[] colors =
            {
                Color.FromRgb(121, 183, 168),
                Color.FromRgb(112, 153, 181),
                Color.FromRgb(112, 169, 145),
                Color.FromRgb(180, 126, 139),
                Color.FromRgb(184, 157, 111)
            };
            ThemeService.SetAccent(colors[Math.Min(AccentPicker.SelectedIndex, colors.Length - 1)]);
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OpacitySlider != null) ThemeService.SetPanelOpacity(OpacitySlider.Value);
        }

        private void LayoutPreset_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null) LayoutMessage.Text = "Current preset: " + button.Content;
        }
    }

    internal sealed class ProfileDialog : Window
    {
        private readonly TextBox _name = new TextBox();
        private readonly TextBox _address = new TextBox();

        public string ProfileName { get { return _name.Text.Trim(); } }
        public string ProfileAddress { get { return _address.Text.Trim(); } }

        public ProfileDialog()
        {
            Title = "Add profile";
            Width = 420;
            Height = 270;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(25, 26, 39));
            Foreground = Brushes.White;
            var panel = new StackPanel { Margin = new Thickness(22) };

            panel.Children.Add(new TextBlock { Text = "Profile name", Margin = new Thickness(0, 0, 0, 5) });
            _name.Margin = new Thickness(0, 0, 0, 14);
            panel.Children.Add(_name);

            panel.Children.Add(new TextBlock { Text = "Server / configuration address", Margin = new Thickness(0, 0, 0, 5) });
            _address.Margin = new Thickness(0, 0, 0, 18);
            panel.Children.Add(_address);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var cancel = new Button { Content = "Cancel", Margin = new Thickness(0, 0, 8, 0), MinWidth = 85 };
            cancel.Click += (s, e) => { DialogResult = false; };
            var save = new Button { Content = "Save profile", MinWidth = 100, Background = new SolidColorBrush(Color.FromRgb(115, 96, 205)) };
            save.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(ProfileName))
                {
                    MessageBox.Show(this, "Enter a profile name.", "NEXORA", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                DialogResult = true;
            };
            buttons.Children.Add(cancel);
            buttons.Children.Add(save);
            panel.Children.Add(buttons);
            Content = panel;
        }
    }
}
