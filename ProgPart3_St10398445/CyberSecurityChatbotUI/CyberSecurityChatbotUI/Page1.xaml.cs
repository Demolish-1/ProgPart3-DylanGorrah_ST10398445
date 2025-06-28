using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CyberSecurityChatbotUI
{
    public partial class QuizPage : Page
    {
        public class QuizQuestion
        {
            public string Question { get; set; }
            public string[] Options { get; set; }
            public int CorrectAnswer { get; set; }
            public string Explanation { get; set; }
        }

        public class LeaderboardEntry
        {
            public string Rank { get; set; }
            public string Name { get; set; }
            public string Score { get; set; }
        }

        private List<QuizQuestion> questions;
        private int currentQuestionIndex = 0;
        private int score = 0;
        private List<LeaderboardEntry> leaderboard;
        private bool hasAnswered = false;

        public QuizPage()
        {
            InitializeComponent();
            InitializeQuiz();
            LoadLeaderboard();
        }

        private void InitializeQuiz()
        {
            questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What is the most common type of cyber attack?",
                    Options = new[] { "Phishing attacks", "Malware attacks", "DDoS attacks", "SQL injection" },
                    CorrectAnswer = 0,
                    Explanation = "Phishing attacks are the most common, targeting users through deceptive emails and websites."
                },
                new QuizQuestion
                {
                    Question = "What does a strong password typically include?",
                    Options = new[] { "Only uppercase letters", "Your birthday", "Mix of letters, numbers & symbols", "Your pet's name" },
                    CorrectAnswer = 2,
                    Explanation = "Strong passwords combine uppercase, lowercase, numbers, and special characters."
                },
                new QuizQuestion
                {
                    Question = "What should you do if you receive a suspicious email?",
                    Options = new[] { "Click all links to investigate", "Forward it to friends", "Delete it immediately", "Reply asking for verification" },
                    CorrectAnswer = 2,
                    Explanation = "Suspicious emails should be deleted immediately without clicking any links or attachments."
                },
                new QuizQuestion
                {
                    Question = "What is two-factor authentication (2FA)?",
                    Options = new[] { "Using two passwords", "Additional security layer", "Two different browsers", "Logging in twice" },
                    CorrectAnswer = 1,
                    Explanation = "2FA adds an extra security layer beyond just your password, like a phone code."
                },
                new QuizQuestion
                {
                    Question = "How often should you update your software?",
                    Options = new[] { "Never", "Once a year", "When prompted/regularly", "Only when it breaks" },
                    CorrectAnswer = 2,
                    Explanation = "Regular software updates patch security vulnerabilities and should be installed promptly."
                },
                new QuizQuestion
                {
                    Question = "What is a VPN used for?",
                    Options = new[] { "Faster internet", "Secure connection", "Free downloads", "More storage" },
                    CorrectAnswer = 1,
                    Explanation = "VPNs create secure, encrypted connections especially important on public Wi-Fi."
                },
                new QuizQuestion
                {
                    Question = "What should you avoid on public Wi-Fi?",
                    Options = new[] { "Checking weather", "Online banking", "Reading news", "Playing games" },
                    CorrectAnswer = 1,
                    Explanation = "Avoid accessing sensitive accounts like banking on public Wi-Fi networks."
                },
                new QuizQuestion
                {
                    Question = "What is malware?",
                    Options = new[] { "Good software", "Malicious software", "Mail software", "Management software" },
                    CorrectAnswer = 1,
                    Explanation = "Malware is malicious software designed to harm or gain unauthorized access to systems."
                },
                new QuizQuestion
                {
                    Question = "How can you identify a secure website?",
                    Options = new[] { "It has many ads", "URL starts with HTTPS", "It loads quickly", "It has bright colors" },
                    CorrectAnswer = 1,
                    Explanation = "Secure websites use HTTPS (with padlock icon) to encrypt data transmission."
                },
                new QuizQuestion
                {
                    Question = "What should you do before downloading software?",
                    Options = new[] { "Download from any site", "Verify the source", "Click first result", "Skip reading details" },
                    CorrectAnswer = 1,
                    Explanation = "Always verify software sources and download from official, trusted websites."
                }
            };

            LoadQuestion();
        }

        private void LoadQuestion()
        {
            if (currentQuestionIndex >= questions.Count)
            {
                ShowResults();
                return;
            }

            hasAnswered = false;
            var question = questions[currentQuestionIndex];

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s1, e1) =>
            {
                QuestionText.Text = question.Question;
                Option1.Content = $"A) {question.Options[0]}";
                Option2.Content = $"B) {question.Options[1]}";
                Option3.Content = $"C) {question.Options[2]}";
                Option4.Content = $"D) {question.Options[3]}";

                ResetButtonStyles();
                NextButton.Visibility = Visibility.Collapsed;

                QuestionNumberText.Text = $"{currentQuestionIndex + 1}/10";
                ProgressBar.Value = ((currentQuestionIndex + 1) * 10);

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                QuestionContent.BeginAnimation(OpacityProperty, fadeIn);
            };

            QuestionContent.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void ResetButtonStyles()
        {
            Option1.Style = (Style)FindResource("QuizButtonStyle");
            Option2.Style = (Style)FindResource("QuizButtonStyle");
            Option3.Style = (Style)FindResource("QuizButtonStyle");
            Option4.Style = (Style)FindResource("QuizButtonStyle");

            Option1.IsEnabled = true;
            Option2.IsEnabled = true;
            Option3.IsEnabled = true;
            Option4.IsEnabled = true;
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (hasAnswered) return;

            hasAnswered = true;
            var clickedButton = sender as Button;
            var selectedAnswer = int.Parse(clickedButton.Tag.ToString());
            var correctAnswer = questions[currentQuestionIndex].CorrectAnswer;

            Option1.IsEnabled = false;
            Option2.IsEnabled = false;
            Option3.IsEnabled = false;
            Option4.IsEnabled = false;

            var buttons = new[] { Option1, Option2, Option3, Option4 };

            if (selectedAnswer == correctAnswer)
            {
                score++;
                ScoreText.Text = score.ToString();
                clickedButton.Style = (Style)FindResource("CorrectAnswerStyle");

                var scaleUp = new DoubleAnimation(1, 1.1, TimeSpan.FromMilliseconds(200));
                var scaleDown = new DoubleAnimation(1.1, 1, TimeSpan.FromMilliseconds(200));
                scaleUp.Completed += (s2, e2) =>
                {
                    clickedButton.BeginAnimation(FrameworkElement.WidthProperty, scaleDown);
                };
                clickedButton.BeginAnimation(FrameworkElement.WidthProperty, scaleUp);
            }
            else
            {
                clickedButton.Style = (Style)FindResource("WrongAnswerStyle");
                buttons[correctAnswer].Style = (Style)FindResource("CorrectAnswerStyle");
            }

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1500);
            timer.Tick += (s3, e3) =>
            {
                timer.Stop();
                NextButton.Visibility = Visibility.Visible;

                var slideIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                NextButton.BeginAnimation(OpacityProperty, slideIn);
            };
            timer.Start();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex++;
            LoadQuestion();
        }

        private void ShowResults()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s4, e4) =>
            {
                QuizScreen.Visibility = Visibility.Collapsed;
                FinalScoreText.Text = $"Your Score: {score}/10";

                if (score >= 9) PerformanceMessage.Text = "🌟 Outstanding! You're a cybersecurity expert!";
                else if (score >= 7) PerformanceMessage.Text = "🔥 Great job! You have solid cybersecurity knowledge!";
                else if (score >= 5) PerformanceMessage.Text = "👍 Good work! Keep learning about cybersecurity!";
                else PerformanceMessage.Text = "📚 Keep studying! Cybersecurity knowledge is important!";

                ResultsScreen.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                ResultsScreen.BeginAnimation(OpacityProperty, fadeIn);
            };

            QuizScreen.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void LoadLeaderboard()
        {
            leaderboard = new List<LeaderboardEntry>
            {
                new LeaderboardEntry { Rank = "1.", Name = "CyberPro", Score = "10/10" },
                new LeaderboardEntry { Rank = "2.", Name = "SecureUser", Score = "9/10" },
                new LeaderboardEntry { Rank = "3.", Name = "SafetyFirst", Score = "8/10" },
                new LeaderboardEntry { Rank = "4.", Name = "GuardianX", Score = "8/10" },
                new LeaderboardEntry { Rank = "5.", Name = "NetDefender", Score = "7/10" }
            };

            UpdateLeaderboardDisplay();
        }

        private void UpdateLeaderboardDisplay()
        {
            LeaderboardList.ItemsSource = leaderboard.Take(10);
        }

        private void SaveScoreButton_Click(object sender, RoutedEventArgs e)
        {
            var playerName = string.IsNullOrWhiteSpace(PlayerNameTextBox.Text) ? "Anonymous" : PlayerNameTextBox.Text.Trim();

            leaderboard.Add(new LeaderboardEntry
            {
                Name = playerName,
                Score = $"{score}/10"
            });

            leaderboard = leaderboard
                .OrderByDescending(entry => int.Parse(entry.Score.Split('/')[0]))
                .ThenBy(entry => entry.Name)
                .Select((entry, index) => new LeaderboardEntry
                {
                    Rank = $"{index + 1}.",
                    Name = entry.Name,
                    Score = entry.Score
                })
                .ToList();

            UpdateLeaderboardDisplay();

            SaveScoreButton.Content = "✅ Saved!";
            SaveScoreButton.IsEnabled = false;

            var successAnimation = new DoubleAnimation(1, 0.7, TimeSpan.FromMilliseconds(200));
            var restoreAnimation = new DoubleAnimation(0.7, 1, TimeSpan.FromMilliseconds(200));
            successAnimation.Completed += (s5, e5) =>
            {
                SaveScoreButton.BeginAnimation(OpacityProperty, restoreAnimation);
            };
            SaveScoreButton.BeginAnimation(OpacityProperty, successAnimation);
        }

        private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex = 0;
            score = 0;
            hasAnswered = false;
            ScoreText.Text = "0";

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s6, e6) =>
            {
                ResultsScreen.Visibility = Visibility.Collapsed;
                QuizScreen.Visibility = Visibility.Visible;

                LoadQuestion();

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                QuizScreen.BeginAnimation(OpacityProperty, fadeIn);
            };

            ResultsScreen.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowMainMenu();
        }
    }
}
