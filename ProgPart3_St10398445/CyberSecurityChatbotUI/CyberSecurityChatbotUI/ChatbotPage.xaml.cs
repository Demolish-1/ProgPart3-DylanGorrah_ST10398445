using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace CyberSecurityChatbotUI
{
    public partial class ChatbotPage : Page
    {
        private List<ChatMessage> chatHistory = new List<ChatMessage>();
        private DispatcherTimer typingTimer;
        private string currentTypingText = "";
        private int typingIndex = 0;
        private Random rand = new Random();
        private bool isFirstInteraction = true;

        private ConversationMemory memory = new ConversationMemory();
        private SentimentAnalyzer sentimentAnalyzer = new SentimentAnalyzer();
        private DynamicResponseSystem responseSystem = new DynamicResponseSystem();

        public ChatbotPage()
        {
            InitializeComponent();
            InitializeChatbot();
        }

        private void InitializeChatbot()
        {
            typingTimer = new DispatcherTimer();
            typingTimer.Interval = TimeSpan.FromMilliseconds(50);
            typingTimer.Tick += TypingTimer_Tick;

            PlayWelcomeSound();
            ShowWelcomeMessage();
        }

        private void PlayWelcomeSound()
        {
            Task.Run(() =>
            {
                try
                {
                    string path = FindAudioFile();
                    if (path != null)
                    {
                        using (var player = new SoundPlayer(path))
                        {
                            player.Load();
                            player.Play();
                        }
                    }
                }
                catch { }
            });
        }

        private string FindAudioFile()
        {
            string[] possiblePaths = {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", "BOT Voice.wav"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BOT Voice.wav"),
                @"Audio\BOT Voice.wav"
            };
            return possiblePaths.FirstOrDefault(File.Exists);
        }

        private void ShowWelcomeMessage()
        {
            string welcomeMessage = @"
   ___      _                 __        __             _           _     _              _   
  / __\   _| |__   ___ _ __  / _\ __ _ / _| ___       /_\  ___ ___(_)___| |_ __ _ _ __ | |_ 
 / / | | | | '_ \ / _ \ '__| \ \ / _` | |_ / _ \     //_\\/ __/ __| / __| __/ _` | '_ \| __|
/ /__| |_| | |_) |  __/ |    _\ \ (_| |  _|  __/    /  _  \__ \__ \ \__ \ || (_| | | | | |_ 
\____/\__, |_.__/ \___|_|    \__/\__,_|_|  \___|    \_/ \_/___/___/_|___/\__\__,_|_| |_|\__|
      |___/                                                                                 

------------------------------------------------
    Welcome to the Cyber Security Chatbot!
------------------------------------------------

";
            AppendToChat(welcomeMessage);
            AppendToChat("\nChatBot: Please enter your name to begin...\n");
        }

        private void AppendToChat(string text)
        {
            ChatDisplay.AppendText(text);
            ChatDisplay.ScrollToEnd();
        }

        private void TypeEffect(string message)
        {
            currentTypingText = message;
            typingIndex = 0;
            typingTimer.Start();
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (typingIndex < currentTypingText.Length)
            {
                ChatDisplay.AppendText(currentTypingText[typingIndex].ToString());
                ChatDisplay.ScrollToEnd();
                typingIndex++;
            }
            else
            {
                typingTimer.Stop();
                ChatDisplay.AppendText("\n");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => ProcessUserInput();
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ProcessUserInput();
        }

        private void ProcessUserInput()
        {
            string input = UserInput.Text.Trim();
            UserInput.Clear();

            if (string.IsNullOrEmpty(input)) return;

            AppendToChat($"You: {input}\n");

            if (isFirstInteraction && !input.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                HandleFirstInteraction(input);
                ActivityLogger.LogChatActivity(input, "introduction");
            }
            else
            {
                var sentiment = sentimentAnalyzer.AnalyzeSentiment(input);
                var responseData = responseSystem.GetResponse(input, sentiment, memory);
                memory.AddInteraction(input, responseData.Topic);
                memory.UpdateSentimentHistory(sentiment);
                chatHistory.Add(new ChatMessage("You", input));
                chatHistory.Add(new ChatMessage("ChatBot", responseData.Response));
                ActivityLogger.LogChatActivity(memory.UserName, responseData.Topic, input);

                AppendToChat("ChatBot: ");
                TypeEffect(responseData.Response + "\n");
            }
        }

        private void HandleFirstInteraction(string input)
        {
            string userName = string.IsNullOrEmpty(input) ? "User" : input;
            memory.UserName = userName;
            memory.AddInteraction(input, "name_introduction");

            string welcome = $"ChatBot: Nice to meet you, {userName}! I'm excited to help you learn about cyber security. How can I assist you today?\n";
            TypeEffect(welcome);

            string questionsPrompt = @"
--------------------------------------------------
    Here are some questions you can ask:
--------------------------------------------------
    How are you?
    What is phishing?
    What is a strong password?
    What is public Wi-Fi?
    How to avoid malware?
    What is two-factor authentication?
    How to secure my home network?
    What are common cyber threats?
--------------------------------------------------
";
            AppendToChat(questionsPrompt);
            isFirstInteraction = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Tag is MainWindow mainWindow)
                {
                    mainWindow.ShowMainMenu();
                }
                else if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    var mainWin = Application.Current.MainWindow as MainWindow;
                    mainWin?.ShowMainMenu();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Navigation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService?.GoBack();
            }
        }

        public void ClearChatHistory()
        {
            chatHistory.Clear();
            ChatDisplay.Clear();
            memory.Reset();
            isFirstInteraction = true;
            ShowWelcomeMessage();
        }

        private class ChatMessage
        {
            public string Sender { get; }
            public string Text { get; }

            public ChatMessage(string sender, string text)
            {
                Sender = sender;
                Text = text;
            }
        }

        private class SentimentAnalyzer
        {
            private readonly Dictionary<string, SentimentType> sentimentKeywords = new Dictionary<string, SentimentType>
            {
                {"good", SentimentType.Positive}, {"great", SentimentType.Positive}, {"awesome", SentimentType.Positive},
                {"excellent", SentimentType.Positive}, {"love", SentimentType.Positive}, {"like", SentimentType.Positive},
                {"amazing", SentimentType.Positive}, {"wonderful", SentimentType.Positive}, {"fantastic", SentimentType.Positive},
                {"perfect", SentimentType.Positive}, {"thank", SentimentType.Positive}, {"thanks", SentimentType.Positive},
                {"bad", SentimentType.Negative}, {"terrible", SentimentType.Negative}, {"awful", SentimentType.Negative},
                {"hate", SentimentType.Negative}, {"worst", SentimentType.Negative}, {"horrible", SentimentType.Negative},
                {"sucks", SentimentType.Negative}, {"stupid", SentimentType.Negative}, {"useless", SentimentType.Negative},
                {"frustrated", SentimentType.Frustrated}, {"annoyed", SentimentType.Frustrated}, {"confused", SentimentType.Frustrated},
                {"difficult", SentimentType.Frustrated}, {"hard", SentimentType.Frustrated}, {"complicated", SentimentType.Frustrated},
                {"help", SentimentType.Frustrated}, {"stuck", SentimentType.Frustrated}, {"problem", SentimentType.Frustrated},
                {"worried", SentimentType.Worried}, {"scared", SentimentType.Worried}, {"afraid", SentimentType.Worried},
                {"concern", SentimentType.Worried}, {"dangerous", SentimentType.Worried}, {"risk", SentimentType.Worried},
                {"security", SentimentType.Worried}, {"safe", SentimentType.Worried}, {"protect", SentimentType.Worried},
                {"how", SentimentType.Curious}, {"what", SentimentType.Curious}, {"why", SentimentType.Curious},
                {"when", SentimentType.Curious}, {"where", SentimentType.Curious}, {"explain", SentimentType.Curious},
                {"tell", SentimentType.Curious}, {"learn", SentimentType.Curious}, {"understand", SentimentType.Curious}
            };

            public UserSentiment AnalyzeSentiment(string text)
            {
                var words = text.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var sentimentScores = new Dictionary<SentimentType, int>();

                foreach (var word in words)
                {
                    var cleanWord = Regex.Replace(word, @"[^\w]", "");
                    if (sentimentKeywords.ContainsKey(cleanWord))
                    {
                        var sentiment = sentimentKeywords[cleanWord];
                        sentimentScores[sentiment] = sentimentScores.ContainsKey(sentiment) ? sentimentScores[sentiment] + 1 : 1;
                    }
                }

                if (!sentimentScores.Any())
                    return new UserSentiment(SentimentType.Neutral, 0.5f);

                var dominantSentiment = sentimentScores.OrderByDescending(x => x.Value).First();
                float confidence = (float)dominantSentiment.Value / words.Length;
                return new UserSentiment(dominantSentiment.Key, Math.Min(confidence * 2, 1.0f));
            }
        }

        private class DynamicResponseSystem
        {
            private readonly Dictionary<string, TopicResponses> topicResponses = new Dictionary<string, TopicResponses>
            {
                ["phishing"] = new TopicResponses
                {
                    BaseResponses = new[]
                    {
                        "Phishing is a cyber attack where criminals impersonate legitimate organizations to steal your personal information.",
                        "Phishing attacks use deceptive emails, websites, or messages to trick you into revealing sensitive data.",
                        "These attacks often create fake websites that look identical to real ones to steal your login credentials."
                    },
                    SentimentVariations = new Dictionary<SentimentType, string[]>
                    {
                        [SentimentType.Worried] = new[]
                        {
                            "I understand your concern about phishing - it's a real threat! Here's how to protect yourself:",
                            "Your worry about phishing is completely valid. Let me help you stay safe:"
                        },
                        [SentimentType.Curious] = new[]
                        {
                            "Great question about phishing! It's important to understand this threat:",
                            "I'm glad you're asking about phishing - knowledge is your best defense:"
                        },
                        [SentimentType.Frustrated] = new[]
                        {
                            "I know phishing can be frustrating to deal with. Let me break it down simply:",
                            "Phishing attacks can be annoying, but understanding them helps you avoid them:"
                        }
                    },
                    Tips = new[]
                    {
                        "Always verify the sender before clicking links or providing information!",
                        "Look for suspicious URLs, spelling errors, and urgent language in emails.",
                        "When in doubt, contact the organization directly through official channels."
                    }
                },
                ["password"] = new TopicResponses
                {
                    BaseResponses = new[]
                    {
                        "A strong password is your first line of defense against cyber attacks.",
                        "Password security is crucial - weak passwords are like leaving your front door unlocked!",
                        "Creating strong, unique passwords for each account is essential for online security."
                    },
                    SentimentVariations = new Dictionary<SentimentType, string[]>
                    {
                        [SentimentType.Frustrated] = new[]
                        {
                            "I know managing passwords can be overwhelming, but I'll make it simple for you:",
                            "Password management doesn't have to be complicated! Here's an easy approach:"
                        },
                        [SentimentType.Curious] = new[]
                        {
                            "Excellent question about passwords! Here's what makes them strong:",
                            "I love that you're thinking about password security! Let me explain:"
                        }
                    },
                    Tips = new[]
                    {
                        "Use 12+ characters with uppercase, lowercase, numbers, and symbols.",
                        "Make each password unique for every account you have.",
                        "Consider using a password manager like Bitwarden or LastPass to help!"
                    }
                },
                ["malware"] = new TopicResponses
                {
                    BaseResponses = new[]
                    {
                        "Malware is malicious software designed to damage or gain unauthorized access to your computer.",
                        "These harmful programs can steal your data, spy on you, or damage your system.",
                        "Malware comes in many forms: viruses, trojans, spyware, and more."
                    },
                    SentimentVariations = new Dictionary<SentimentType, string[]>
                    {
                        [SentimentType.Worried] = new[]
                        {
                            "Your concern about malware is understandable - it's a serious threat. Here's how to protect yourself:",
                            "I can help ease your worries about malware with some solid protection strategies:"
                        }
                    },
                    Tips = new[]
                    {
                        "Keep your software and operating system updated!",
                        "Use reputable antivirus software and keep it running.",
                        "Don't download files from suspicious websites or unknown sources."
                    }
                }
            };

            private readonly string[] generalResponses = new[]
            {
                "I don't have specific information about that topic yet, but I'm here to help with cyber security!",
                "That's an interesting question! While I don't have details on that specific topic, I can help with other security matters.",
                "I'm still learning about that area, but I'd love to help you with other cyber security questions!"
            };

            public ResponseData GetResponse(string input, UserSentiment sentiment, ConversationMemory memory)
            {
                var keywords = ExtractKeywords(input);
                var topic = IdentifyTopic(keywords);

                if (topicResponses.ContainsKey(topic))
                {
                    return GenerateTopicResponse(topic, sentiment, memory);
                }

                return GenerateGeneralResponse(input, sentiment, memory);
            }

            private string[] ExtractKeywords(string input)
            {
                return input.ToLower()
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => Regex.Replace(w, @"[^\w]", ""))
                    .Where(w => w.Length > 2)
                    .ToArray();
            }

            private string IdentifyTopic(string[] keywords)
            {
                var topicScores = new Dictionary<string, int>();
                var topicKeywords = new Dictionary<string, string[]>
                {
                    ["phishing"] = new[] { "phishing", "phish", "email", "fake", "scam", "fraud", "suspicious", "link" },
                    ["password"] = new[] { "password", "pass", "strong", "weak", "secure", "authentication", "login", "credential" },
                    ["malware"] = new[] { "malware", "virus", "trojan", "spyware", "antivirus", "infection", "harmful" },
                    ["wifi"] = new[] { "wifi", "wireless", "network", "public", "hotspot", "connection" },
                    ["2fa"] = new[] { "2fa", "two", "factor", "authentication", "verification", "token" }
                };

                foreach (var keyword in keywords)
                {
                    foreach (var topic in topicKeywords)
                    {
                        if (topic.Value.Contains(keyword))
                        {
                            topicScores[topic.Key] = topicScores.ContainsKey(topic.Key) ? topicScores[topic.Key] + 1 : 1;
                        }
                    }
                }

                return topicScores.Count > 0 ? topicScores.OrderByDescending(x => x.Value).First().Key : "general";
            }

            private ResponseData GenerateTopicResponse(string topic, UserSentiment sentiment, ConversationMemory memory)
            {
                var topicData = topicResponses[topic];
                var random = new Random();
                string response = topicData.BaseResponses[random.Next(topicData.BaseResponses.Length)];

                if (topicData.SentimentVariations.ContainsKey(sentiment.Type))
                {
                    var variations = topicData.SentimentVariations[sentiment.Type];
                    response = variations[random.Next(variations.Length)];
                }

                if (!string.IsNullOrEmpty(memory.UserName) && memory.InteractionCount > 3)
                {
                    response = $"{memory.UserName}, " + response.ToLower();
                }

                if (topicData.Tips.Any())
                {
                    var tip = topicData.Tips[random.Next(topicData.Tips.Length)];
                    response += $" {tip}";
                }

                if (memory.GetTopicFrequency(topic) > 2)
                {
                    response += " Since you're particularly interested in this topic, feel free to ask me more specific questions!";
                }

                return new ResponseData(response, topic, sentiment.Type);
            }

            private ResponseData GenerateGeneralResponse(string input, UserSentiment sentiment, ConversationMemory memory)
            {
                var random = new Random();
                string response = generalResponses[random.Next(generalResponses.Length)];

                if (!string.IsNullOrEmpty(memory.UserName))
                {
                    response = $"{memory.UserName}, " + response.ToLower();
                }

                var favoriteTopics = memory.GetFavoriteTopics();
                if (favoriteTopics.Any())
                {
                    response += $" You seemed interested in {string.Join(" and ", favoriteTopics)} before - would you like to know more?";
                }
                else
                {
                    response += " Try asking about: phishing, passwords, malware, public Wi-Fi, or two-factor authentication!";
                }

                return new ResponseData(response, "general", sentiment.Type);
            }
        }

        private class ConversationMemory
        {
            public string UserName { get; set; } = "";
            public List<InteractionRecord> Interactions { get; } = new List<InteractionRecord>();
            public List<SentimentType> SentimentHistory { get; } = new List<SentimentType>();
            public int InteractionCount => Interactions.Count;

            public void AddInteraction(string userInput, string topic)
            {
                Interactions.Add(new InteractionRecord
                {
                    UserInput = userInput,
                    Topic = topic,
                    Timestamp = DateTime.Now
                });
            }

            public void UpdateSentimentHistory(UserSentiment sentiment)
            {
                SentimentHistory.Add(sentiment.Type);
                if (SentimentHistory.Count > 20)
                {
                    SentimentHistory.RemoveAt(0);
                }
            }

            public int GetTopicFrequency(string topic)
            {
                return Interactions.Count(i => i.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase));
            }

            public string[] GetFavoriteTopics()
            {
                return Interactions
                    .Where(i => i.Topic != "general" && i.Topic != "name_introduction")
                    .GroupBy(i => i.Topic)
                    .OrderByDescending(g => g.Count())
                    .Take(2)
                    .Select(g => g.Key)
                    .ToArray();
            }

            public SentimentType GetDominantSentiment()
            {
                if (!SentimentHistory.Any()) return SentimentType.Neutral;
                return SentimentHistory
                    .GroupBy(s => s)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;
            }

            public void Reset()
            {
                UserName = "";
                Interactions.Clear();
                SentimentHistory.Clear();
            }
        }

        private class TopicResponses
        {
            public string[] BaseResponses { get; set; } = new string[0];
            public Dictionary<SentimentType, string[]> SentimentVariations { get; set; } = new Dictionary<SentimentType, string[]>();
            public string[] Tips { get; set; } = new string[0];
        }

        private class InteractionRecord
        {
            public string UserInput { get; set; } = "";
            public string Topic { get; set; } = "";
            public DateTime Timestamp { get; set; }
        }

        private class UserSentiment
        {
            public SentimentType Type { get; }
            public float Confidence { get; }

            public UserSentiment(SentimentType type, float confidence)
            {
                Type = type;
                Confidence = confidence;
            }
        }

        private class ResponseData
        {
            public string Response { get; }
            public string Topic { get; }
            public SentimentType UserSentiment { get; }

            public ResponseData(string response, string topic, SentimentType sentiment)
            {
                Response = response;
                Topic = topic;
                UserSentiment = sentiment;
            }
        }

        private enum SentimentType
        {
            Positive,
            Negative,
            Neutral,
            Frustrated,
            Curious,
            Worried
        }
    }
}