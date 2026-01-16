# Chatify - Python E2EE Chat

A secure, End-to-End Encrypted chat application built with Python.

## Tech Stack
- **Server**: Python 3.10+, FastAPI, Python-SocketIO
- **Client (New)**: C# 10.0+ (WPF), SocketIOClient, Sodium.Core (Great UI)
- **Client (Legacy)**: Python 3.10+, CustomTkinter (UI), SocketIO-Client
- **Encryption**: PyNaCl (libsodium) - Curve25519, XSalsa20-Poly1305

## Setup

1. **Install Dependencies**
   ```bash
   pip install -r requirements.txt
   ```

2. **Run Server**
   ```bash
   cd server
   python main.py
   ```

3. **Run Client (C# WPF)**
   ```bash
   dotnet run --project client-csharp/ChatifyClient.csproj
   ```

4. **Run Client (Python Legacy)**
   Open a new terminal:
   ```bash
   cd client
   python main.py
   ```

## Deploying 24/7 (Free Cloud Hosting)

To keep your server online when your computer is off, deploy to **Render**:

1.  Push this project to **GitHub**.
2.  Sign up at [render.com](https://render.com).
3.  Click **New +** -> **Web Service**.
4.  Connect your GitHub repository.
5.  Render will automatically detect `render.yaml` and configure everything.
6.  Once deployed, copy the **Render URL** (e.g., `https://chatify-xyz.onrender.com`).
7.  Use this URL in your Mobile/Desktop app to connect!

## Android App (Mobile)
This project includes a .NET MAUI project configured for **Android**.

- **Project Location**: `client-mobile/`
- **Build Requirements**: .NET 8 SDK + Android Workload (`dotnet workload install android`)
- **Emulator Access**: Use `http://10.0.2.2:8000` to access localhost from Android Emulator.
- Real-time messaging
- End-to-End Encryption (Server cannot see messages)
- Modern Dark Mode UI
