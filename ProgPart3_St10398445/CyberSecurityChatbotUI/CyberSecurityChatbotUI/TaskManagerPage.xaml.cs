using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CyberSecurityChatbotUI
{
    public partial class TaskManagerPage : Page
    {
        private ObservableCollection<SecurityTask> tasks;

        public TaskManagerPage()
        {
            InitializeComponent();
            InitializeTasks();
        }

        private void InitializeTasks()
        {
            tasks = new ObservableCollection<SecurityTask>();
            TaskListView.ItemsSource = tasks;
            AddSampleTasks();
        }

        private void AddSampleTasks()
        {
            tasks.Add(new SecurityTask
            {
                Title = "Enable Two-Factor Authentication",
                Description = "Set up 2FA on all important accounts (email, banking, social media)",
                Priority = TaskPriority.High,
                CreatedDate = DateTime.Now,
                ReminderDate = DateTime.Now.AddDays(1)
            });

            tasks.Add(new SecurityTask
            {
                Title = "Update Passwords",
                Description = "Change passwords for accounts that haven't been updated in 6+ months",
                Priority = TaskPriority.Medium,
                CreatedDate = DateTime.Now,
                ReminderDate = DateTime.Now.AddDays(3)
            });

            tasks.Add(new SecurityTask
            {
                Title = "Install Security Updates",
                Description = "Check and install latest security updates for Windows and applications",
                Priority = TaskPriority.High,
                CreatedDate = DateTime.Now,
                ReminderDate = DateTime.Now.AddDays(7)
            });

            tasks.Add(new SecurityTask
            {
                Title = "Review Privacy Settings",
                Description = "Audit privacy settings on social media platforms and online accounts",
                Priority = TaskPriority.Low,
                CreatedDate = DateTime.Now.AddDays(-2),
                ReminderDate = DateTime.Now.AddDays(14)
            });
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitleTextBox.Text))
            {
                MessageBox.Show("⚠️ Please enter a task title.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TaskPriority priority = TaskPriority.Medium;
            if (PriorityComboBox.SelectedIndex == 0) priority = TaskPriority.High;
            else if (PriorityComboBox.SelectedIndex == 1) priority = TaskPriority.Medium;
            else if (PriorityComboBox.SelectedIndex == 2) priority = TaskPriority.Low;

            var newTask = new SecurityTask
            {
                Title = TaskTitleTextBox.Text.Trim(),
                Description = TaskDescriptionTextBox.Text.Trim(),
                Priority = priority,
                CreatedDate = DateTime.Now,
                ReminderDate = ReminderDatePicker.SelectedDate
            };

            tasks.Add(newTask);
            ActivityLogger.LogTaskActivity("created", newTask.Title, priority.ToString());

            ClearForm();
            MessageBox.Show($"✅ Task '{newTask.Title}' added successfully!",
                "Task Added", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearFormButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            TaskTitleTextBox.Text = "";
            TaskDescriptionTextBox.Text = "";
            ReminderDatePicker.SelectedDate = null;
            PriorityComboBox.SelectedIndex = 1;
            TaskTitleTextBox.Focus();
        }

        private void TaskCompleted_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var task = checkBox?.DataContext as SecurityTask;

            if (task != null)
            {
                task.IsCompleted = true;
                task.CompletedDate = DateTime.Now;
                ActivityLogger.LogTaskActivity("completed", task.Title);

                MessageBox.Show($"✅ Great job! Task '{task.Title}' marked as completed!",
                    "Task Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TaskCompleted_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var task = checkBox?.DataContext as SecurityTask;

            if (task != null)
            {
                task.IsCompleted = false;
                task.CompletedDate = null;
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var task = button?.Tag as SecurityTask;

            if (task != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{task.Title}'?",
                    "Delete Task", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    tasks.Remove(task);
                    ActivityLogger.LogTaskActivity("deleted", task.Title);
                    MessageBox.Show("🗑️ Task deleted successfully!",
                        "Task Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ClearCompleted_Click(object sender, RoutedEventArgs e)
        {
            var completedTasks = tasks.Where(t => t.IsCompleted).ToList();

            if (completedTasks.Count == 0)
            {
                MessageBox.Show("No completed tasks to clear.", "No Tasks",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete {completedTasks.Count} completed task(s)?",
                "Clear Completed Tasks", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var task in completedTasks)
                {
                    tasks.Remove(task);
                    ActivityLogger.LogTaskActivity("cleared", task.Title);
                }

                MessageBox.Show($"🧹 {completedTasks.Count} completed task(s) cleared!",
                    "Tasks Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ShowMainMenu();
        }
    }

    public class SecurityTask : INotifyPropertyChanged
    {
        private bool _isCompleted;
        private string _title;
        private string _description;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(TitleColor));
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                _isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(TitleColor));
            }
        }

        public TaskPriority Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public string TitleColor => IsCompleted ? "#888888" : "White";

        public string PriorityIcon
        {
            get
            {
                switch (Priority)
                {
                    case TaskPriority.High: return "🔴 High";
                    case TaskPriority.Medium: return "🟡 Medium";
                    case TaskPriority.Low: return "🟢 Low";
                    default: return "🟡 Medium";
                }
            }
        }

        public string ReminderText
        {
            get
            {
                if (!ReminderDate.HasValue) return "";
                var days = (ReminderDate.Value.Date - DateTime.Now.Date).Days;
                if (days < 0) return "⚠️ Overdue!";
                if (days == 0) return "📅 Due Today!";
                if (days == 1) return "📅 Due Tomorrow";
                return $"📅 Due in {days} days";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }
}