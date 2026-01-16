using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using SocketIOClient;

namespace ChatifyMobile
{
    public partial class MainPage : ContentPage
    {
        private SocketIOClient.SocketIO client;
        private CryptoManager crypto;
        private string myUsername;
        private User selectedUser;

        // Chat History: UserSID -> List<ChatMessage>
        private Dictionary<string, ObservableCollection<ChatMessage>> chatHistory = new Dictionary<string, ObservableCollection<ChatMessage>>();

        public ObservableCollection<User> OnlineUsers { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<ChatMessage> CurrentMessages { get; set; } = new ObservableCollection<ChatMessage>();

        public MainPage()
        {
            InitializeComponent();
            crypto = new CryptoManager();
            UsersList.ItemsSource = OnlineUsers;
            MessagesList.ItemsSource = CurrentMessages;
        }

        private async void ConnectBtn_Clicked(object sender, EventArgs e)
        {
            var username = UsernameInput.Text?.Trim();
            var serverUrl = ServerUrlInput.Text?.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                StatusText.Text = "Please enter a username";
                return;
            }
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                StatusText.Text = "Please enter Server URL";
                return;
            }

            myUsername = username;
            StatusText.Text = "Connecting...";
            ConnectBtn.IsEnabled = false;

            try
            {
                client = new SocketIOClient.SocketIO(serverUrl);

                client.OnConnected += async (s, e) =>
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        LoginView.IsVisible = false;
                        ChatView.IsVisible = true;
                        
                        await client.EmitAsync("register", new
                        {
                            username = myUsername,
                            public_key = crypto.GetPublicKeyBase64()
                        });
                    });
                };

                client.On("user_list", response =>
                {
                    var users = response.GetValue<List<User>>();
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnlineUsers.Clear();
                        foreach (var user in users)
                        {
                            if (user.Username != myUsername)
                            {
                                OnlineUsers.Add(user);
                            }
                        }
                    });
                });

                client.On("private_message", response =>
                {
                    var data = response.GetValue<JsonObject>();
                    var fromSid = data["from_sid"].ToString();
                    var fromUsername = data["from_username"].ToString();
                    var encryptedMsg = data["encrypted_message"].ToString();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var senderUser = OnlineUsers.FirstOrDefault(u => u.Sid == fromSid);
                        if (senderUser != null)
                        {
                            try
                            {
                                var decrypted = crypto.Decrypt(senderUser.Public_Key, encryptedMsg);
                                AddMessage(fromSid, fromUsername, decrypted);
                            }
                            catch (Exception ex)
                            {
                                AddMessage(fromSid, "System", $"[Decryption Error]: {ex.Message}");
                            }
                        }
                    });
                });

                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusText.Text = $"Connection failed: {ex.Message}";
                    ConnectBtn.IsEnabled = true;
                });
            }
        }

        private void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is User user)
            {
                selectedUser = user;
                
                if (!chatHistory.ContainsKey(user.Sid))
                {
                    chatHistory[user.Sid] = new ObservableCollection<ChatMessage>();
                }

                // Switch message context
                CurrentMessages.Clear();
                foreach(var msg in chatHistory[user.Sid])
                {
                    CurrentMessages.Add(msg);
                }
            }
        }

        private async void SendBtn_Clicked(object sender, EventArgs e)
        {
            await SendMessage();
        }

        private async void MessageInput_Completed(object sender, EventArgs e)
        {
            await SendMessage();
        }

        private async Task SendMessage()
        {
            if (selectedUser == null) return;
            var text = MessageInput.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                var encrypted = crypto.Encrypt(selectedUser.Public_Key, text);
                
                await client.EmitAsync("private_message", new
                {
                    to_sid = selectedUser.Sid,
                    encrypted_message = encrypted,
                    from_username = myUsername
                });

                AddMessage(selectedUser.Sid, "Me", text);
                MessageInput.Text = "";
            }
            catch (Exception ex)
            {
                 await DisplayAlert("Error", $"Error sending message: {ex.Message}", "OK");
            }
        }

        private void AddMessage(string sid, string sender, string content)
        {
            if (!chatHistory.ContainsKey(sid))
            {
                chatHistory[sid] = new ObservableCollection<ChatMessage>();
            }

            var msg = new ChatMessage { Sender = sender, Content = content, Time = DateTime.Now.ToShortTimeString() };
            chatHistory[sid].Add(msg);

            // If we are currently viewing this chat, add to view source
            if (selectedUser != null && selectedUser.Sid == sid)
            {
               CurrentMessages.Add(msg);
               // Scroll to bottom (basic implementation)
               MessagesList.ScrollTo(msg, position: ScrollToPosition.End, animate: true);
            }
        }
    }
}

