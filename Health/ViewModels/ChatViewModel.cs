using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Health.Services;
using Health.Models;

namespace Health.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly GeminiService _geminiService;

        private string _userMessage;
        private bool _isBusy;

        public ObservableCollection<ChatMessage> Messages { get; set; } = new();

        public string UserMessage
        {
            get => _userMessage;
            set
            {
                _userMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                (SendCommand as Command)?.ChangeCanExecute();
            }
        }

        public ICommand SendCommand { get; }

        public MainViewModel()
        {
            _geminiService = new GeminiService();


            SendCommand = new Command(async () => await SendMessage(), () => !IsBusy);
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(UserMessage)) return;
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                string textToSend = UserMessage;
                UserMessage = string.Empty;

                Messages.Add(new ChatMessage
                {
                    Role = "user",
                    Text = textToSend
                });


                string responseText = await _geminiService.GetResponseAsync(textToSend);


                Messages.Add(new ChatMessage
                {
                    Role = "model",
                    Text = responseText
                });
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage
                {
                    Role = "model",
                    Text = $"Критическая ошибка: {ex.Message}"
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}