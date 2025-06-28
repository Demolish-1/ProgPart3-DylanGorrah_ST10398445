using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace CyberSecurityChatbotUI
{
    public partial class MainWindow : Window
    {
        // === WINDOW SIZE CONSTANTS ===
        private readonly double menuWidth = 900;
        private readonly double menuHeight = 400;
        private readonly double originalWidth = 900;
        private readonly double originalHeight = 400;
        private readonly double taskPageWidth = 800;
        private readonly double taskPageHeight = 800;

        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        private void AnimateWindowSize(double targetWidth, double targetHeight, Action onCompleted = null)
        {
            var duration = TimeSpan.FromMilliseconds(800);
            var widthAnimation = new DoubleAnimation(this.Width, targetWidth, duration)
            {
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
            };
            var heightAnimation = new DoubleAnimation(this.Height, targetHeight, duration)
            {
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
            };

            if (onCompleted != null)
                widthAnimation.Completed += (s, e) => onCompleted();

            this.BeginAnimation(Window.WidthProperty, widthAnimation);
            this.BeginAnimation(Window.HeightProperty, heightAnimation);
        }

        public void ShowMainMenu()
        {
            AnimateWindowSize(menuWidth, menuHeight, () =>
            {
                if (NavigationFrame != null)
                {
                    NavigationFrame.Visibility = Visibility.Collapsed;
                    NavigationFrame.Opacity = 0;
                }
                MainMenuScreen.Visibility = Visibility.Visible;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                MainMenuScreen.BeginAnimation(OpacityProperty, fadeIn);
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, a) => this.Close();
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateWindowSize(menuWidth, menuHeight, () =>
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
                fadeOut.Completed += (s, a) =>
                {
                    EntryScreen.Visibility = Visibility.Collapsed;
                    MainMenuScreen.Visibility = Visibility.Visible;
                    var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                    MainMenuScreen.BeginAnimation(OpacityProperty, fadeIn);
                };
                EntryScreen.BeginAnimation(OpacityProperty, fadeOut);
            });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateWindowSize(originalWidth, originalHeight, () =>
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
                fadeOut.Completed += (s, a) =>
                {
                    MainMenuScreen.Visibility = Visibility.Collapsed;
                    EntryScreen.Visibility = Visibility.Visible;
                    var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                    EntryScreen.BeginAnimation(OpacityProperty, fadeIn);
                };
                MainMenuScreen.BeginAnimation(OpacityProperty, fadeOut);
            });
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, a) =>
            {
                MainMenuScreen.Visibility = Visibility.Collapsed;
                AnimateWindowSize(taskPageWidth, taskPageHeight, () =>
                {
                    try
                    {
                        NavigationFrame.Navigate(new TaskManagerPage());
                        NavigationFrame.Visibility = Visibility.Visible;
                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                        NavigationFrame.BeginAnimation(OpacityProperty, fadeIn);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"TaskManagerPage not found: {ex.Message}\n\nPlease create TaskManagerPage.xaml",
                                      "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Information);
                        ShowMainMenu();
                    }
                });
            };
            MainMenuScreen.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void TakeQuizButton_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, a) =>
            {
                MainMenuScreen.Visibility = Visibility.Collapsed;
                AnimateWindowSize(taskPageWidth, taskPageHeight, () =>
                {
                    try
                    {
                        NavigationFrame.Navigate(new QuizPage());
                        NavigationFrame.Visibility = Visibility.Visible;
                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                        NavigationFrame.BeginAnimation(OpacityProperty, fadeIn);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"QuizPage navigation error: {ex.Message}",
                                      "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ShowMainMenu();
                    }
                });
            };
            MainMenuScreen.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void ViewLogButton_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, a) =>
            {
                MainMenuScreen.Visibility = Visibility.Collapsed;
                AnimateWindowSize(taskPageWidth, taskPageHeight, () =>
                {
                    try
                    {
                        NavigationFrame.Navigate(new ActivityLogPage());
                        NavigationFrame.Visibility = Visibility.Visible;
                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                        NavigationFrame.BeginAnimation(OpacityProperty, fadeIn);
                        ActivityLogger.LogSystemActivity("Activity Log", "User viewed the activity log");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"ActivityLogPage navigation error: {ex.Message}",
                                      "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ShowMainMenu();
                    }
                });
            };
            MainMenuScreen.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, a) =>
            {
                MainMenuScreen.Visibility = Visibility.Collapsed;
                AnimateWindowSize(taskPageWidth, taskPageHeight, () =>
                {
                    try
                    {
                        NavigationFrame.Navigate(new ChatbotPage());
                        NavigationFrame.Visibility = Visibility.Visible;
                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                        NavigationFrame.BeginAnimation(OpacityProperty, fadeIn);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"ChatbotPage navigation error: {ex.Message}",
                                      "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ShowMainMenu();
                    }
                });
            };
            MainMenuScreen.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try { this.DragMove(); }
                catch (InvalidOperationException) { }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try { this.DragMove(); }
                catch (InvalidOperationException) { }
            }
        }

        private void NavigationFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is QuizPage)
            {
                // Quiz page specific logic
            }
            else if (e.Content is TaskManagerPage)
            {
                // Task manager page specific logic
            }
            else if (e.Content is ChatbotPage chatbotPage)
            {
                chatbotPage.Tag = this;
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized)
            {
                // Optional minimize logic
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        private void ShowPage<T>() where T : Page, new()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, a) =>
            {
                MainMenuScreen.Visibility = Visibility.Collapsed;
                AnimateWindowSize(taskPageWidth, taskPageHeight, () =>
                {
                    try
                    {
                        NavigationFrame.Navigate(new T());
                        NavigationFrame.Visibility = Visibility.Visible;
                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                        NavigationFrame.BeginAnimation(OpacityProperty, fadeIn);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error navigating to page: {ex.Message}",
                                      "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ShowMainMenu();
                    }
                });
            };
            MainMenuScreen.BeginAnimation(OpacityProperty, fadeOut);
        }

        public void NavigateToPage(Page page)
        {
            if (NavigationFrame != null && page != null)
            {
                NavigationFrame.Navigate(page);
            }
        }

        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}