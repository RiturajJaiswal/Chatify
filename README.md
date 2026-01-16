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

### Option 1: PythonAnywhere (Recommended - Always Free)

**Best for:** Always-on, 24/7 uptime with NO sleeping

1. Sign up at [pythonanywhere.com](https://pythonanywhere.com) (free account)
2. Go to **"Web"** tab → **"Add a new web app"**
3. Choose **Python 3.11** → **"Manual configuration"**
4. In **"Bash console"**, run:
   ```bash
   cd ~
   git clone https://github.com/RiturajJaiswal/Chatify.git mysite
   cd mysite
   python3.11 -m venv .venv
   source .venv/bin/activate
   pip install -r requirements.txt
   ```
5. Go back to **Web settings**:
   - **Source code:** `/home/yourusername/mysite`
   - **Virtualenv:** `/home/yourusername/mysite/.venv`
6. **WSGI configuration file** - Click to edit and replace with content from `pythonanywhere_wsgi.py`
7. Click **"Reload"** button (green)
8. Your server URL: `https://yourusername.pythonanywhere.com`

### Option 2: Railway (Sleeps after 15 min inactivity)

1. Push this project to **GitHub** (already done ✅).
2. Sign up at [railway.app](https://railway.app).
3. Click **New Project** → **Deploy from GitHub Repo**.
4. Select your `Chatify` repository.
5. Railway will auto-detect the Python project and deploy using `Procfile`.
6. Once deployed, copy the **Railway URL**.

### Option 3: Render (Free, Sleeps after inactivity)

1. Sign up at [render.com](https://render.com)
2. Click **New +** → **Web Service**
3. Connect your GitHub repo
4. Use `render.yaml` config
5. Deploy!

---

**Update your clients with the new server URL:**
- Edit [client-csharp/MainWindow.xaml.cs](client-csharp/MainWindow.xaml.cs) - Change `DEFAULT_SERVER_URL`
- Edit [client-mobile/MainPage.xaml.cs](client-mobile/MainPage.xaml.cs) - Change `DEFAULT_SERVER_URL`

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
