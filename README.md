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

To keep your server online when your computer is off, deploy to **Railway** (free):

1. Push this project to **GitHub** (already done ✅).
2. Sign up at [railway.app](https://railway.app).
3. Click **New Project** → **Deploy from GitHub Repo**.
4. Select your `Chatify` repository.
5. Railway will auto-detect the Python project and deploy using `railway.json`.
6. Once deployed, copy the **Railway URL** (visible in dashboard).
7. Use this URL in your Mobile/Desktop app to connect!

## Automatic APK Build (GitHub Actions)

The APK is automatically built whenever you push code to GitHub:

1. Go to your GitHub repository
2. Click **"Actions"** tab
3. Select **"Build APK"** workflow
4. Click the latest successful build
5. Download the **`chatify-apk`** artifact (the APK file)
6. Share this APK with your friends to install on their Android phones!

**Automatic Release:** Tag your commit with a version (e.g., `v1.0.0`) to create an automatic release with the APK downloadable from the Releases page.
- Real-time messaging
- End-to-End Encryption (Server cannot see messages)
- Modern Dark Mode UI
