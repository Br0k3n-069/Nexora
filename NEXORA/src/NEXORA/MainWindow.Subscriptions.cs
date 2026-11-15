using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using NEXORA.Models;
using NEXORA.Services;

namespace NEXORA
{
    public partial class MainWindow
    {
        private readonly SubscriptionClient _subscriptionClient = new SubscriptionClient();

        private void AddSubscription_Click(object sender, RoutedEventArgs e)
        {
            AddSubscriptionAsync();
        }

        private async void AddSubscriptionAsync()
        {
            var url = SubscriptionUrlBox.Text.Trim();
            if (url.Length == 0) return;
            try
            {
                SubscriptionStatusText.Text = "Downloading subscription...";
                var source = new SubscriptionSource { Url = url, Name = GetSourceName(url) };
                var data = await _subscriptionClient.DownloadAsync(url);
                source.Nodes = SubscriptionParser.ParseText(data, source.Id);
                if (source.Nodes.Count == 0)
                {
                    MessageBox.Show(this, "No supported server links were found in this response.", "NEXORA", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                _subscriptionStore.AddSource(source);
                SubscriptionUrlBox.Clear();
                SubscriptionsList.SelectedItem = source;
                SubscriptionStatusText.Text = "Imported " + source.Nodes.Count + " nodes.";
            }
            catch (Exception ex)
            {
                SubscriptionStatusText.Text = "Import failed.";
                MessageBox.Show(this, ex.Message, "Subscription import", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void UpdateSubscription_Click(object sender, RoutedEventArgs e)
        {
            var source = SubscriptionsList.SelectedItem as SubscriptionSource;
            if (source == null || string.IsNullOrWhiteSpace(source.Url)) return;
            try
            {
                SubscriptionStatusText.Text = "Updating...";
                var data = await _subscriptionClient.DownloadAsync(source.Url);
                var nodes = SubscriptionParser.ParseText(data, source.Id);
                if (nodes.Count == 0) throw new InvalidOperationException("No supported server links found.");
                _subscriptionStore.ReplaceNodes(source, nodes);
                SubscriptionsList.Items.Refresh();
                SubscriptionNodesList.ItemsSource = _subscriptionStore.Nodes.Where(n => n.SourceId == source.Id).ToList();
                SubscriptionStatusText.Text = "Updated " + nodes.Count + " nodes.";
            }
            catch (Exception ex)
            {
                source.LastError = ex.Message;
                _subscriptionStore.Save();
                SubscriptionStatusText.Text = "Update failed: " + ex.Message;
                MessageBox.Show(this, ex.Message, "Subscription update", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import VPN configuration",
                Filter = "VPN configurations|*.ovpn;*.conf;*.txt;*.json|All files|*.*",
                Multiselect = true
            };
            if (dialog.ShowDialog(this) != true) return;

            var source = new SubscriptionSource { Name = "Local import " + DateTime.Now.ToString("yyyy-MM-dd") };
            foreach (var path in dialog.FileNames)
            {
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".txt" || ext == ".json")
                    source.Nodes.AddRange(SubscriptionParser.ParseText(_subscriptionClient.ReadConfigFile(path), source.Id));
                else
                    source.Nodes.Add(_subscriptionClient.ImportConfigFile(path, source.Id));
            }

            if (source.Nodes.Count == 0)
            {
                MessageBox.Show(this, "No supported configuration was found.", "NEXORA", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _subscriptionStore.AddSource(source);
            SubscriptionsList.SelectedItem = source;
            SubscriptionStatusText.Text = "Imported " + source.Nodes.Count + " configuration(s).";
        }

        private void RemoveSubscription_Click(object sender, RoutedEventArgs e)
        {
            var source = SubscriptionsList.SelectedItem as SubscriptionSource;
            if (source == null) return;
            if (MessageBox.Show(this, "Remove this source and all its nodes?", "NEXORA", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            _subscriptionStore.RemoveSource(source);
            SubscriptionNodesList.ItemsSource = _subscriptionStore.Nodes;
            SubscriptionStatusText.Text = "Source removed.";
        }

        private void SubscriptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var source = SubscriptionsList.SelectedItem as SubscriptionSource;
            SubscriptionNodesList.ItemsSource = source == null ? _subscriptionStore.Nodes :
                _subscriptionStore.Nodes.Where(n => n.SourceId == source.Id).ToList();
            if (source != null)
                SubscriptionStatusText.Text = source.Nodes.Count + " nodes · " +
                    (source.LastUpdatedAt.HasValue ? source.LastUpdatedAt.Value.ToLocalTime().ToString("g") : "Not refreshed yet");
        }

        private void ExportLinks_Click(object sender, RoutedEventArgs e)
        {
            PreviewExport("Subscription links", SubscriptionExporter.ExportLinks(_subscriptionStore.Sources.ToArray()), "Text files|*.txt");
        }

        private void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            PreviewExport("Subscription JSON", SubscriptionExporter.ExportJson(_subscriptionStore.Sources.ToArray()), "JSON files|*.json");
        }

        private void PreviewExport(string title, string data, string filter)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                MessageBox.Show(this, "There is no exportable URI data yet.", "NEXORA", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var preview = new Window
            {
                Owner = this, Title = title + " preview", Width = 720, Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new SolidColorBrush(Color.FromRgb(22, 28, 31))
            };
            var panel = new DockPanel { Margin = new Thickness(14) };
            var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var close = new Button { Content = "Close", MinWidth = 80, Margin = new Thickness(0, 0, 8, 0) };
            close.Click += (s, e) => preview.Close();
            var save = new Button { Content = "Save file...", MinWidth = 95 };
            save.Click += (s, e) =>
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = filter,
                    FileName = title.Contains("JSON") ? "nexora-subscriptions.json" : "nexora-subscriptions.txt"
                };
                if (saveDialog.ShowDialog(preview) == true)
                    File.WriteAllText(saveDialog.FileName, data, System.Text.Encoding.UTF8);
            };
            actions.Children.Add(close);
            actions.Children.Add(save);
            DockPanel.SetDock(actions, Dock.Bottom);
            panel.Children.Add(actions);
            panel.Children.Add(new TextBox
            {
                Text = data, IsReadOnly = true, AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas")
            });
            preview.Content = panel;
            preview.ShowDialog();
        }

        private static string GetSourceName(string url)
        {
            Uri uri;
            return Uri.TryCreate(url, UriKind.Absolute, out uri) ? uri.Host : "Subscription";
        }
    }
}