import customtkinter as ctk
import socketio
import threading
import asyncio
from crypto import CryptoManager

ctk.set_appearance_mode("System")
ctk.set_default_color_theme("blue")

class ChatApp(ctk.CTk):
    def __init__(self):
        super().__init__()

        self.title("Chatify - E2EE Python Chat")
        self.geometry("800x600")

        self.username = ""
        self.sio = socketio.AsyncClient()
        self.crypto = CryptoManager()
        self.connected_users = [] # List of dicts
        self.active_chat_sid = None
        self.chat_boxes = {} # sid -> Box
        self.message_history = {} # sid -> list of strings

        self.setup_ui()
        self.start_socket_thread()

    def setup_ui(self):
        # Login Frame
        self.login_frame = ctk.CTkFrame(self)
        self.login_frame.pack(fill="both", expand=True, padx=20, pady=20)

        self.login_label = ctk.CTkLabel(self.login_frame, text="Enter Username", font=("Arial", 20))
        self.login_label.pack(pady=20)
        
        self.username_entry = ctk.CTkEntry(self.login_frame, placeholder_text="Username")
        self.username_entry.pack(pady=10)
        
        self.login_btn = ctk.CTkButton(self.login_frame, text="Connect", command=self.login)
        self.login_btn.pack(pady=20)

        # Main Chat Interface (Hidden initially)
        self.chat_interface = ctk.CTkFrame(self)
        
        # Sidebar for Users
        self.sidebar = ctk.CTkScrollableFrame(self.chat_interface, width=200, label_text="Tavern")
        self.sidebar.pack(side="left", fill="y", padx=10, pady=10)

        # Chat Area
        self.chat_area = ctk.CTkFrame(self.chat_interface)
        self.chat_area.pack(side="right", fill="both", expand=True, padx=10, pady=10)

        self.chat_display = ctk.CTkTextbox(self.chat_area, state="disabled")
        self.chat_display.pack(fill="both", expand=True, padx=5, pady=5)

        self.input_frame = ctk.CTkFrame(self.chat_area)
        self.input_frame.pack(fill="x", padx=5, pady=5)

        self.msg_entry = ctk.CTkEntry(self.input_frame, placeholder_text="Type a message...")
        self.msg_entry.pack(side="left", fill="x", expand=True, padx=5)
        self.msg_entry.bind("<Return>", lambda e: self.send_message())

        self.send_btn = ctk.CTkButton(self.input_frame, text="Send", width=60, command=self.send_message)
        self.send_btn.pack(side="right", padx=5)

    def login(self):
        user = self.username_entry.get()
        if user:
            self.username = user
            self.login_frame.pack_forget()
            self.chat_interface.pack(fill="both", expand=True)
            # Trigger connection logic in the async loop
            # But since we can't easily jump threads, we set a flag or rely on the connected event logic if already running
            asyncio.run_coroutine_threadsafe(self.connect_to_server(), self.loop)

    def start_socket_thread(self):
        # Run asyncio loop in a separate thread
        self.loop = asyncio.new_event_loop()
        threading.Thread(target=self.start_async_loop, args=(self.loop,), daemon=True).start()

    def start_async_loop(self, loop):
        asyncio.set_event_loop(loop)
        
        # Register SocketIO events
        @self.sio.event
        async def connect():
            print("Connected to server")
            await self.sio.emit('register', {
                'username': self.username,
                'public_key': self.crypto.get_public_key_b64()
            })

        @self.sio.event
        async def user_list(data):
            self.connected_users = [u for u in data if u['username'] != self.username]
            self.update_sidebar()

        @self.sio.event
        async def private_message(data):
            from_sid = data['from_sid']
            from_user = data['from_username']
            encrypted_msg = data['encrypted_message']
            
            # Ensure we have a box
            if from_sid not in self.chat_boxes:
                # Find user public key
                user_info = next((u for u in self.connected_users if u['sid'] == from_sid), None)
                if user_info:
                    self.chat_boxes[from_sid] = self.crypto.create_box(user_info['public_key'])
            
            box = self.chat_boxes.get(from_sid)
            if box:
                try:
                    decrypted = self.crypto.decrypt(box, encrypted_msg)
                    display_text = f"[{from_user}]: {decrypted}\n"
                    self.add_message_to_history(from_sid, display_text)
                    if self.active_chat_sid == from_sid:
                         self.append_chat_display(display_text)
                except Exception as e:
                    print(f"Decryption error: {e}")

        loop.run_forever()

    async def connect_to_server(self):
        try:
            await self.sio.connect('http://localhost:8000')
        except Exception as e:
            print(f"Connection failed: {e}")

    def update_sidebar(self):
        # Clear existing buttons
        for widget in self.sidebar.winfo_children():
            widget.destroy()
            
        for user in self.connected_users:
            btn = ctk.CTkButton(self.sidebar, text=user['username'], 
                                command=lambda s=user['sid']: self.select_user(s))
            btn.pack(fill="x", pady=2)

    def select_user(self, sid):
        self.active_chat_sid = sid
        self.chat_display.configure(state="normal")
        self.chat_display.delete("1.0", "end")
        
        # Restore history
        if sid in self.message_history:
            for msg in self.message_history[sid]:
                self.chat_display.insert("end", msg)
                
        self.chat_display.configure(state="disabled")

    def add_message_to_history(self, sid, text):
        if sid not in self.message_history:
            self.message_history[sid] = []
        self.message_history[sid].append(text)

    def append_chat_display(self, text):
        self.chat_display.configure(state="normal")
        self.chat_display.insert("end", text)
        self.chat_display.configure(state="disabled")

    def send_message(self):
        text = self.msg_entry.get()
        if text and self.active_chat_sid:
            # Encrypt
            if self.active_chat_sid not in self.chat_boxes:
                user_info = next((u for u in self.connected_users if u['sid'] == self.active_chat_sid), None)
                if user_info:
                    self.chat_boxes[self.active_chat_sid] = self.crypto.create_box(user_info['public_key'])
            
            box = self.chat_boxes.get(self.active_chat_sid)
            if box:
                encrypted = self.crypto.encrypt(box, text)
                
                # Send
                asyncio.run_coroutine_threadsafe(
                    self.sio.emit('private_message', {
                        'to_sid': self.active_chat_sid,
                        'encrypted_message': encrypted,
                        'from_username': self.username
                    }), self.loop
                )
                
                # Update UI
                display_text = f"[Me]: {text}\n"
                self.append_chat_display(display_text)
                self.add_message_to_history(self.active_chat_sid, display_text)
                self.msg_entry.delete(0, "end")

if __name__ == "__main__":
    app = ChatApp()
    app.mainloop()
