using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SocketIOClient;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ChatifyClient
{
    public partial class MainWindow : Window
    {
        private SocketIOClient.SocketIO client;
        private CryptoManager crypto;
        private string myUsername;
        private User selectedUser;
        
        // Chat History: UserSID -> List<ChatMessage>
        private Dictionary<string, ObservableCollection<ChatMessage>> chatHistory = new Dictionary<string, ObservableCollection<ChatMessage>>();
        
        public ObservableCollection<User> OnlineUsers { get; set; } = new ObservableCollection<User>();

        public MainWindow()
        {
            InitializeComponent();
            crypto = new CryptoManager();
            UsersList.ItemsSource = OnlineUsers;
        }

        private async void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameInput.Text.Trim();
            var serverUrl = ServerUrlInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                StatusText.Text = "Please enter a username";
                return;
            }
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                StatusText.Text = "Please enter a Server URL";
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
                    await Dispatcher.InvokeAsync(() =>
                    {
                        LoginView.Visibility = Visibility.Collapsed;
                        ChatView.Visibility = Visibility.Visible;
                        Title = $"Chatify - {myUsername}";
                    });

                    await client.EmitAsync("register", new
                    {
                        username = myUsername,
                        public_key = crypto.GetPublicKeyBase64()
                    });
                };

                client.On("user_list", response =>
                {
                    var users = response.GetValue<List<User>>();
                    Dispatcher.Invoke(() =>
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

                    // Find the user to get their public key
                    // If user is not in the list (e.g. race condition), we might have issues. 
                    // ideally we should look up in a map. For now search in OnlineUsers.
                    
                    Dispatcher.Invoke(() =>
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
                StatusText.Text = $"Connection failed: {ex.Message}";
                ConnectBtn.IsEnabled = true;
            }
        }

        private void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersList.SelectedItem is User user)
            {
                selectedUser = user;
                ChatHeader.Text = $"Chat with {user.Username}";
                
                if (!chatHistory.ContainsKey(user.Sid))
                {
                    chatHistory[user.Sid] = new ObservableCollection<ChatMessage>();
                }
                
                MessagesList.ItemsSource = chatHistory[user.Sid];
            }
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (selectedUser == null) return;
            var text = MessageInput.Text.Trim();
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
                MessageInput.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }

        private void AddMessage(string sid, string sender, string content)
        {
            if (!chatHistory.ContainsKey(sid))
            {
                chatHistory[sid] = new ObservableCollection<ChatMessage>();
            }

            // If the message is for the currently selected user, or we are adding to history
            var msg = new ChatMessage { Sender = sender, Content = content, Time = DateTime.Now.ToShortTimeString() };
            
            // Dispatcher is needed if this is called from non-UI thread
            Dispatcher.Invoke(() => 
            {
                chatHistory[sid].Add(msg);
                // Auto scroll
                if (selectedUser != null && selectedUser.Sid == sid)
                {
                    if (MessagesList.Items.Count > 0)
                    {
                        MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                    }
                }
            });
        }
    }

    public class User
    {
        [JsonPropertyName("sid")]
        public string Sid { get; set; }
        
        [JsonPropertyName("username")]
        public string Username { get; set; }
        
        [JsonPropertyName("public_key")]
        public string Public_Key { get; set; }
    }

    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public string Time { get; set; }
    }
}
