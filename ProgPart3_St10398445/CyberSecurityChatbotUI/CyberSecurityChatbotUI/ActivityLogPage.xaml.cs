using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace CyberSecurityChatbotUI
{
    public partial class ActivityLogPage : Page
    {
        private ObservableCollection<ActivityLogEntry> activities;
        private static ActivityLogPage instance;

        public ActivityLogPage()
        {
            InitializeComponent();
            instance = this;
            InitializeActivityLog();
        }

        private void InitializeActivityLog()
        {
            activities = new ObservableCollection<ActivityLogEntry>();
            ActivityListBox.ItemsSource = activities;
            LoadActivitiesFromLogger();
            UpdateUI();
        }

        private void LoadActivitiesFromLogger()
        {
            var loggedActivities = ActivityLogger.GetRecentActivities();
            activities.Clear();
            foreach (var activity in loggedActivities)
            {
                activities.Add(activity);
            }
        }

        private void UpdateUI()
        {
            var totalActivities = activities.Count;
            var chatActivities = activities.Count(a => a.ActivityType == ActivityType.Chat);
            var quizActivities = activities.Count(a => a.ActivityType == ActivityType.Quiz);
            var taskActivities = activities.Count(a => a.ActivityType == ActivityType.Task);

            TotalActivitiesText.Text = totalActivities.ToString();
            ChatActivitiesText.Text = chatActivities.ToString();
            QuizActivitiesText.Text = quizActivities.ToString();
            TaskActivitiesText.Text = taskActivities.ToString();

            ActivityCountText.Text = $"({totalActivities} activities)";

            if (totalActivities == 0)
            {
                EmptyStatePanel.Visibility = Visibility.Visible;
                ActivityListBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyStatePanel.Visibility = Visibility.Collapsed;
                ActivityListBox.Visibility = Visibility.Visible;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadActivitiesFromLogger();
            UpdateUI();
            MessageBox.Show("🔄 Activity log refreshed!", "Refresh Complete",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (activities.Count == 0)
            {
                MessageBox.Show("No activities to clear.", "Nothing to Clear",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to clear all {activities.Count} activities?\n\nThis action cannot be undone.",
                "Clear Activity Log", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ActivityLogger.ClearAllActivities();
                activities.Clear();
                UpdateUI();
                MessageBox.Show("🗑️ Activity log cleared successfully!", "Log Cleared",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (activities.Count == 0)
            {
                MessageBox.Show("No activities to export.", "Nothing to Export",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv",
                DefaultExt = "txt",
                FileName = $"CyberSecurity_ActivityLog_{DateTime.Now:yyyy-MM-dd}.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExportToFile(saveFileDialog.FileName);
                    MessageBox.Show($"💾 Activity log exported successfully to:\n{saveFileDialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Error exporting log:\n{ex.Message}", "Export Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToFile(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("=== CYBERSECURITY ASSISTANT ACTIVITY LOG ===");
                writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Total Activities: {activities.Count}");
                writer.WriteLine();

                foreach (var activity in activities.OrderByDescending(a => a.DateTime))
                {
                    writer.WriteLine($"[{activity.DateTime:yyyy-MM-dd HH:mm:ss}] {activity.Icon} {activity.Title}");
                    writer.WriteLine($"  Description: {activity.Description}");
                    if (!string.IsNullOrEmpty(activity.Details))
                        writer.WriteLine($"  Details: {activity.Details}");
                    writer.WriteLine();
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ShowMainMenu();
        }

        public static void RefreshActivityLog()
        {
            instance?.LoadActivitiesFromLogger();
            instance?.UpdateUI();
        }
    }

    public class ActivityLogEntry : INotifyPropertyChanged
    {
        public DateTime DateTime { get; set; }
        public ActivityType ActivityType { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }

        public string TimeStamp => DateTime.ToString("HH:mm:ss");
        public string DateStamp => DateTime.ToString("MM/dd/yyyy");

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ActivityType
    {
        Chat,
        Quiz,
        Task,
        System
    }

    public static class ActivityLogger
    {
        private static ObservableCollection<ActivityLogEntry> allActivities = new ObservableCollection<ActivityLogEntry>();
        private const int MAX_ACTIVITIES = 100;

        public static void LogActivity(ActivityType type, string title, string description, string details = "")
        {
            var activity = new ActivityLogEntry
            {
                DateTime = DateTime.Now,
                ActivityType = type,
                Icon = GetIconForActivityType(type),
                Title = title,
                Description = description,
                Details = details
            };

            allActivities.Insert(0, activity);
            while (allActivities.Count > MAX_ACTIVITIES)
            {
                allActivities.RemoveAt(allActivities.Count - 1);
            }

            ActivityLogPage.RefreshActivityLog();
        }

        private static string GetIconForActivityType(ActivityType type)
        {
            switch (type)
            {
                case ActivityType.Chat: return "💬";
                case ActivityType.Quiz: return "🧠";
                case ActivityType.Task: return "📝";
                case ActivityType.System: return "⚙️";
                default: return "📋";
            }
        }

        public static ObservableCollection<ActivityLogEntry> GetRecentActivities(int count = 50)
        {
            var recentActivities = new ObservableCollection<ActivityLogEntry>();
            foreach (var activity in allActivities.Take(count))
            {
                recentActivities.Add(activity);
            }
            return recentActivities;
        }

        public static void ClearAllActivities()
        {
            allActivities.Clear();
        }

        public static void LogChatActivity(string userName, string topic, string lastMessage = "")
        {
            var title = string.IsNullOrEmpty(userName) ? "Chat Session Started" : $"Chat with {userName}";
            var description = string.IsNullOrEmpty(topic) ? "General conversation" : $"Discussed: {topic}";
            var details = string.IsNullOrEmpty(lastMessage) ? "" : $"Last message: {lastMessage}";

            LogActivity(ActivityType.Chat, title, description, details);
        }

        public static void LogQuizActivity(string userName, int score, int totalQuestions)
        {
            var title = string.IsNullOrEmpty(userName) ? "Quiz Completed" : $"{userName} completed quiz";
            var description = $"Score: {score}/{totalQuestions} ({(score * 100 / totalQuestions)}%)";
            var details = GetQuizPerformanceMessage(score, totalQuestions);

            LogActivity(ActivityType.Quiz, title, description, details);
        }

        public static void LogTaskActivity(string action, string taskTitle, string priority = "")
        {
            var title = $"Task {action}";
            var description = taskTitle;
            var details = string.IsNullOrEmpty(priority) ? "" : $"Priority: {priority}";

            LogActivity(ActivityType.Task, title, description, details);
        }

        public static void LogSystemActivity(string action, string description, string details = "")
        {
            LogActivity(ActivityType.System, action, description, details);
        }

        private static string GetQuizPerformanceMessage(int score, int total)
        {
            var percentage = (score * 100) / total;
            if (percentage >= 90) return "🌟 Outstanding performance!";
            if (percentage >= 70) return "🔥 Great job!";
            if (percentage >= 50) return "👍 Good work!";
            return "📚 Keep studying!";
        }
    }
}