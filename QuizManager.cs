using System;
using System.Collections.Generic;

namespace CyberBotGUI
{
    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; }
        public bool IsMultipleChoice { get; set; }
    }

    public class QuizManager
    {
        private List<QuizQuestion> _questions;
        private int _currentIndex = 0;
        private int _score = 0;
        private bool _isActive = false;
        private Random _rand = new Random();

        public bool IsActive => _isActive;
        public int Score => _score;
        public int TotalQuestions => _questions.Count;

        public QuizManager()
        {
            InitializeQuestions();
        }

        private void InitializeQuestions()
        {
            _questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report it as phishing", "D) Ignore it" },
                    CorrectIndex = 2,
                    Explanation = "Always report phishing emails to help protect others!",
                    IsMultipleChoice = true
                },
                new QuizQuestion
                {
                    Question = "TRUE or FALSE: Using the same password for multiple accounts is safe.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectIndex = 1,
                    Explanation = "False! Always use unique passwords for each account.",
                    IsMultipleChoice = false
                },
                new QuizQuestion
                {
                    Question = "What does HTTPS mean in a website URL?",
                    Options = new List<string> { "A) The site is fast", "B) The site is secure", "C) The site is free", "D) The site is popular" },
                    CorrectIndex = 1,
                    Explanation = "HTTPS means the connection is encrypted and secure!",
                    IsMultipleChoice = true
                },
                new QuizQuestion
                {
                    Question = "TRUE or FALSE: Public Wi-Fi is always safe to use for banking.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectIndex = 1,
                    Explanation = "False! Never do banking on public Wi-Fi without a VPN.",
                    IsMultipleChoice = false
                },
                new QuizQuestion
                {
                    Question = "What is two-factor authentication (2FA)?",
                    Options = new List<string> { "A) Two passwords", "B) A backup email", "C) Extra security layer beyond password", "D) A type of virus" },
                    CorrectIndex = 2,
                    Explanation = "2FA adds an extra layer of security to your accounts!",
                    IsMultipleChoice = true
                },
                new QuizQuestion
                {
                    Question = "TRUE or FALSE: You should click links in emails from unknown senders.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectIndex = 1,
                    Explanation = "False! Never click links from unknown senders — it could be phishing!",
                    IsMultipleChoice = false
                },
                new QuizQuestion
                {
                    Question = "What is malware?",
                    Options = new List<string> { "A) A type of hardware", "B) Malicious software", "C) A secure website", "D) A password manager" },
                    CorrectIndex = 1,
                    Explanation = "Malware is malicious software designed to damage or gain unauthorised access!",
                    IsMultipleChoice = true
                },
                new QuizQuestion
                {
                    Question = "TRUE or FALSE: A strong password should be at least 12 characters long.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectIndex = 0,
                    Explanation = "True! Longer passwords are much harder to crack.",
                    IsMultipleChoice = false
                },
                new QuizQuestion
                {
                    Question = "What is phishing?",
                    Options = new List<string> { "A) A type of fishing", "B) Tricking users into revealing info", "C) A secure login method", "D) A firewall type" },
                    CorrectIndex = 1,
                    Explanation = "Phishing is when attackers trick you into revealing sensitive information!",
                    IsMultipleChoice = true
                },
                new QuizQuestion
                {
                    Question = "TRUE or FALSE: Antivirus software should be kept updated.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectIndex = 0,
                    Explanation = "True! Updated antivirus protects against the latest threats.",
                    IsMultipleChoice = false
                },
                new QuizQuestion
                {
                    Question = "Which of these is the safest password?",
                    Options = new List<string> { "A) password123", "B) John1990", "C) P@ssw0rd!", "D) BlueSky$Tree99!" },
                    CorrectIndex = 3,
                    Explanation = "Long passwords with mixed characters are the strongest!",
                    IsMultipleChoice = true
                }
            };

            // Shuffle questions
            for (int i = _questions.Count - 1; i > 0; i--)
            {
                int j = _rand.Next(i + 1);
                var temp = _questions[i];
                _questions[i] = _questions[j];
                _questions[j] = temp;
            }
        }

        public void StartQuiz()
        {
            _isActive = true;
            _currentIndex = 0;
            _score = 0;
        }

        public QuizQuestion GetCurrentQuestion()
        {
            if (_currentIndex < _questions.Count)
                return _questions[_currentIndex];
            return null;
        }

        public string SubmitAnswer(string answer)
        {
            var question = GetCurrentQuestion();
            if (question == null) return "Quiz complete!";

            answer = answer.ToLower().Trim();
            bool correct = false;

            if (answer == "a" && question.CorrectIndex == 0) correct = true;
            else if (answer == "b" && question.CorrectIndex == 1) correct = true;
            else if (answer == "c" && question.CorrectIndex == 2) correct = true;
            else if (answer == "d" && question.CorrectIndex == 3) correct = true;

            if (correct) _score++;
            _currentIndex++;

            string result = correct ? "✅ Correct! " : $"❌ Wrong! The correct answer was {GetCorrectLetter(question.CorrectIndex)}. ";
            result += question.Explanation;

            if (_currentIndex >= _questions.Count)
            {
                _isActive = false;
                result += GetFinalScore();
            }

            return result;
        }

        private string GetCorrectLetter(int index)
        {
            return index switch { 0 => "A", 1 => "B", 2 => "C", 3 => "D", _ => "A" };
        }

        private string GetFinalScore()
        {
            double percentage = (double)_score / _questions.Count * 100;
            string feedback = percentage >= 80 ? "\n\n🏆 Great job! You're a cybersecurity pro!" :
                             percentage >= 50 ? "\n\n👍 Good effort! Keep learning to stay safe online!" :
                             "\n\n📚 Keep learning! Cybersecurity is important for everyone!";
            return $"\n\n🎯 Quiz Complete! Your score: {_score}/{_questions.Count} ({percentage:F0}%){feedback}";
        }
    }
}