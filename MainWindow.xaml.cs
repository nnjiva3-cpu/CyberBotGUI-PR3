using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberBotGUI
{
    public partial class MainWindow : Window
    {
        // Delegates
        public delegate string ResponseHandler(string input);
        public delegate void MessageDisplay(string message);
        private ResponseHandler _responseHandler;
        private MessageDisplay _displayHandler;

        private string _userName = "";
        private string _lastTopic = "";
        private string _favoriteTopic = "";
        private Dictionary<string, List<string>> _responses;
        private bool _nameAsked = false;
        private Random _rand = new Random();
        private QuizManager _quiz;
        private List<string> _activityLog = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            _responseHandler = new ResponseHandler(GetResponse);
            _displayHandler = new MessageDisplay(AddBotMessage);
            InitializeResponses();
            DatabaseHelper.InitializeDatabase();
            _quiz = new QuizManager();
            PlayVoiceGreeting();
            AskForName();
        }

        private void PlayVoiceGreeting()
        {
            try
            {
                string audioPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
                if (File.Exists(audioPath))
                {
                    var player = new System.Media.SoundPlayer(audioPath);
                    player.Play();
                }
            }
            catch { }
        }

        private void InitializeResponses()
        {
            _responses = new Dictionary<string, List<string>>
            {
                ["password"] = new List<string>
                {
                    "Use at least 12 characters mixing uppercase, lowercase, numbers and symbols!",
                    "Never reuse passwords across different sites — use a password manager like Bitwarden!",
                    "Avoid using personal info like birthdays or names in your passwords.",
                    "Change your passwords every 3-6 months for important accounts.",
                    "A passphrase like 'PurpleCat$RunsFast99' is both strong and memorable!"
                },
                ["phishing"] = new List<string>
                {
                    "Always check the sender's email address carefully before clicking any links!",
                    "Legitimate companies will NEVER ask for your password via email.",
                    "Hover over links to preview the URL before clicking — look for misspellings!",
                    "If an email creates urgency like 'Act now or lose access', it's likely a scam.",
                    "When in doubt, go directly to the website instead of clicking email links."
                },
                ["browsing"] = new List<string>
                {
                    "Always look for HTTPS and the padlock icon before entering personal info!",
                    "Use a VPN when connected to public Wi-Fi networks.",
                    "Keep your browser and extensions updated to patch security vulnerabilities.",
                    "Use an ad blocker to reduce exposure to malicious advertisements.",
                    "Clear your cookies and cache regularly to protect your privacy."
                },
                ["malware"] = new List<string>
                {
                    "Install reputable antivirus software and keep it updated at all times!",
                    "Never download software from untrusted or unofficial sources.",
                    "Be cautious with email attachments — even from people you know!",
                    "Back up your data regularly to protect against ransomware attacks.",
                    "Ransomware can encrypt all your files — always have offline backups!"
                },
                ["privacy"] = new List<string>
                {
                    "Review your privacy settings on all social media accounts regularly!",
                    "Limit the personal information you share publicly online.",
                    "Use a VPN to encrypt your internet traffic and protect your identity.",
                    "Be careful what you post — once online, it can be very hard to remove.",
                    "Use two-factor authentication on all your important accounts!"
                },
                ["scam"] = new List<string>
                {
                    "Be suspicious of unsolicited calls or emails asking for personal info!",
                    "Scammers create urgency — slow down and verify before acting.",
                    "If something sounds too good to be true, it probably is!",
                    "Never send money or gift cards to someone you haven't met in person.",
                    "Verify the identity of anyone requesting sensitive data by calling them directly."
                },
                ["2fa"] = new List<string>
                {
                    "Two-factor authentication adds an extra layer of security beyond your password!",
                    "Use an authenticator app like Google Authenticator instead of SMS for 2FA.",
                    "Enable 2FA on all important accounts — email, banking, and social media.",
                    "Even if someone steals your password, 2FA stops them from logging in!"
                }
            };
        }

        private void AskForName()
        {
            _displayHandler("Welcome to the Cybersecurity Awareness Bot! 🔐");
            _displayHandler("I'm here to help you stay safe in the digital world.");
            _displayHandler("Before we begin, what is your name?");
            _nameAsked = true;
        }

        private void TopicButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                string topic = button.Content.ToString()
                    .Replace("🔑 ", "").Replace("🎣 ", "")
                    .Replace("🌐 ", "").Replace("🦠 ", "")
                    .Replace("🔒 ", "").Replace("⚠️ ", "")
                    .ToLower().Trim();

                if (topic == "passwords") topic = "password";
                if (topic == "safe browsing") topic = "browsing";
                if (topic == "scams") topic = "scam";

                InputBox.Text = topic;
                ProcessInput();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ProcessInput();
        }

        private void ProcessInput()
        {
            string input = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                _displayHandler("⚠️ Please type something! I didn't receive any input.");
                return;
            }

            AddUserMessage(input);
            InputBox.Clear();

            if (_nameAsked && _userName == "")
            {
                _userName = input;
                _nameAsked = false;
                LogActivity($"User {_userName} started a session");
                _displayHandler($"Nice to meet you, {_userName}! 😊");
                _displayHandler("I'm CyberBot, your personal cybersecurity guide.");
                _displayHandler("Click a topic button above or type a question. Type 'help' to see all options!");
                return;
            }

            // Handle quiz answers
            if (_quiz.IsActive)
            {
                string quizResult = _quiz.SubmitAnswer(input);
                _displayHandler(quizResult);
                LogActivity($"Quiz answer submitted");

                if (!_quiz.IsActive)
                    LogActivity($"Quiz completed with score {_quiz.Score}/{_quiz.TotalQuestions}");
                else
                {
                    var nextQ = _quiz.GetCurrentQuestion();
                    if (nextQ != null)
                    {
                        _displayHandler($"\nQuestion {_quiz.TotalQuestions - (_quiz.TotalQuestions - _quiz.Score - (_quiz.TotalQuestions - _quiz.Score))}:");
                        _displayHandler(nextQ.Question);
                        foreach (var opt in nextQ.Options)
                            _displayHandler(opt);
                    }
                }
                return;
            }

            string response = _responseHandler(input.ToLower());
            _displayHandler(response);
        }

        private string GetResponse(string input)
        {
            // Activity log
            if (input.Contains("show activity") || input.Contains("what have you done") || input.Contains("activity log"))
            {
                if (_activityLog.Count == 0)
                    return "No activities recorded yet!";
                string log = $"📋 Activity Log for {_userName}:\n\n";
                int start = Math.Max(0, _activityLog.Count - 10);
                for (int i = start; i < _activityLog.Count; i++)
                    log += $"• {_activityLog[i]}\n";
                return log;
            }

            // Task management
            if (input.Contains("add task") || input.Contains("new task") || input.Contains("create task"))
            {
                string taskTitle = input.Replace("add task", "").Replace("new task", "").Replace("create task", "").Trim();
                if (string.IsNullOrWhiteSpace(taskTitle))
                    taskTitle = "Cybersecurity Review";
                DatabaseHelper.AddTask(taskTitle, $"Task: {taskTitle}", "No reminder set");
                LogActivity($"Task added: '{taskTitle}'");
                return $"✅ Task added: '{taskTitle}'. Would you like to set a reminder? Type 'set reminder [days]'";
            }

            if (input.Contains("set reminder"))
            {
                string days = input.Replace("set reminder", "").Trim();
                if (string.IsNullOrWhiteSpace(days)) days = "7";
                LogActivity($"Reminder set for {days} days");
                return $"⏰ Got it! I'll remind you in {days} days. Stay on top of your cybersecurity!";
            }

            if (input.Contains("view tasks") || input.Contains("show tasks") || input.Contains("my tasks"))
            {
                var tasks = DatabaseHelper.GetAllTasks();
                if (tasks.Count == 0)
                    return $"You have no pending tasks, {_userName}! Type 'add task [name]' to add one.";
                string taskList = $"📝 Your pending tasks, {_userName}:\n\n";
                foreach (var task in tasks)
                    taskList += $"• [{task.Id}] {task.Title} - {task.Reminder}\n";
                taskList += "\nType 'complete task [id]' to mark as done or 'delete task [id]' to remove.";
                return taskList;
            }

            if (input.Contains("complete task"))
            {
                string idStr = input.Replace("complete task", "").Trim();
                if (int.TryParse(idStr, out int id))
                {
                    DatabaseHelper.CompleteTask(id);
                    LogActivity($"Task {id} marked as completed");
                    return $"✅ Task {id} marked as completed! Great work, {_userName}!";
                }
                return "Please specify the task ID. Type 'view tasks' to see your tasks.";
            }

            if (input.Contains("delete task"))
            {
                string idStr = input.Replace("delete task", "").Trim();
                if (int.TryParse(idStr, out int id))
                {
                    DatabaseHelper.DeleteTask(id);
                    LogActivity($"Task {id} deleted");
                    return $"🗑️ Task {id} deleted!";
                }
                return "Please specify the task ID. Type 'view tasks' to see your tasks.";
            }

            // Quiz
            if (input.Contains("quiz") || input.Contains("start quiz") || input.Contains("test me"))
            {
                _quiz.StartQuiz();
                LogActivity("Quiz started");
                var firstQ = _quiz.GetCurrentQuestion();
                string quizStart = $"🎮 Starting the Cybersecurity Quiz, {_userName}! Answer with A, B, C, or D.\n\n";
                quizStart += firstQ.Question + "\n";
                foreach (var opt in firstQ.Options)
                    quizStart += opt + "\n";
                return quizStart;
            }

            // Sentiment detection
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("anxious"))
            {
                LogActivity("Sentiment detected: worried");
                return $"I completely understand, {_userName}. Feeling worried about cybersecurity is very normal! Here's a tip to help:\n\n" + GetRandomResponse("scam");
            }

            if (input.Contains("frustrated") || input.Contains("confused") || input.Contains("don't understand") || input.Contains("lost"))
            {
                LogActivity("Sentiment detected: frustrated");
                return $"No worries at all, {_userName}! Let's slow down. Here's something simple:\n\n" + GetRandomResponse("password");
            }

            if (input.Contains("curious") || input.Contains("interested") || input.Contains("want to learn"))
            {
                if (_favoriteTopic == "") _favoriteTopic = "cybersecurity";
                LogActivity("Sentiment detected: curious");
                return $"I love the curiosity, {_userName}! Here's an interesting tip:\n\n" + GetRandomResponse("privacy");
            }

            if (input.Contains("thank") || input.Contains("thanks"))
                return $"You're welcome, {_userName}! Stay safe out there! 🔐 Is there anything else you'd like to know?";

            // Memory and recall
            if (input.Contains("tell me more") || input.Contains("explain more") || input.Contains("more info") || input.Contains("give me another") || input.Contains("another tip"))
            {
                if (_lastTopic != "")
                    return $"Here's another tip on {_lastTopic}:\n\n" + GetRandomResponse(_lastTopic);
                return $"What topic would you like more information about, {_userName}?";
            }

            if (input.Contains("what do i like") || input.Contains("my favourite") || input.Contains("remember me"))
            {
                if (_favoriteTopic != "")
                    return $"I remember you're interested in {_favoriteTopic}, {_userName}! Here's another tip:\n\n" + GetRandomResponse(_favoriteTopic);
                return $"I don't have a favourite topic saved for you yet, {_userName}. Ask me about any topic first!";
            }

            // Help
            if (input.Contains("help") || input.Contains("topics") || input.Contains("what can"))
                return $"Here's everything I can help you with, {_userName}:\n\n" +
                       "🔑 Passwords\n🎣 Phishing\n🌐 Safe Browsing\n" +
                       "🦠 Malware\n🔒 Privacy\n⚠️ Scams\n🔐 2FA\n\n" +
                       "📝 Task Management:\n" +
                       "• 'add task [name]' - Add a cybersecurity task\n" +
                       "• 'view tasks' - See your tasks\n" +
                       "• 'complete task [id]' - Mark task done\n\n" +
                       "🎮 'start quiz' - Test your knowledge!\n" +
                       "📋 'show activity log' - See recent actions";

            // Greetings
            if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey"))
                return $"Hey {_userName}! 👋 How can I help you stay cyber-safe today?";

            if (input.Contains("how are you"))
                return $"I'm running at full security capacity, {_userName}! 😄 Ready to help!";

            if (input.Contains("purpose") || input.Contains("what do you do") || input.Contains("who are you"))
                return $"I'm CyberBot, {_userName}! I educate you on cybersecurity, manage your tasks, quiz your knowledge, and help you stay safe online!";

            // Topic responses
            foreach (var topic in _responses.Keys)
            {
                if (input.Contains(topic))
                {
                    _lastTopic = topic;
                    if (_favoriteTopic == "") _favoriteTopic = topic;
                    LogActivity($"Topic accessed: {topic}");
                    return $"Here's a tip on {topic}, {_userName}:\n\n" + GetRandomResponse(topic) +
                           "\n\nType 'tell me more' for another tip!";
                }
            }

            // Exit
            if (input.Contains("bye") || input.Contains("exit") || input.Contains("goodbye"))
                return $"Goodbye {_userName}! Stay safe online! 🔐";

            // Default fallback
            return $"I didn't quite catch that, {_userName}. 🤔 Try asking about passwords, phishing, malware, or type 'help' for all options!";
        }

        private void LogActivity(string activity)
        {
            _activityLog.Add($"[{DateTime.Now:HH:mm}] {activity}");
        }

        private string GetRandomResponse(string topic)
        {
            var list = _responses[topic];
            return list[_rand.Next(list.Count)];
        }

        private void AddBotMessage(string message)
        {
            var para = new Paragraph();
            var run = new Run($"🤖 CyberBot: {message}\n");
            run.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 136));
            para.Inlines.Add(run);
            ChatBox.Document.Blocks.Add(para);
            ChatScrollViewer.ScrollToBottom();
        }

        private void AddUserMessage(string message)
        {
            var para = new Paragraph();
            var run = new Run($"👤 {_userName}: {message}\n");
            run.Foreground = new SolidColorBrush(Color.FromRgb(135, 206, 250));
            para.Inlines.Add(run);
            ChatBox.Document.Blocks.Add(para);
            ChatScrollViewer.ScrollToBottom();
        }
    }
}
