#!/bin/bash
# PythonAnywhere Setup Script
# Run this in PythonAnywhere's Bash console after signing up

echo "Setting up Chatify on PythonAnywhere..."

# Navigate to home directory
cd ~

# Clone the repository
git clone https://github.com/RiturajJaiswal/Chatify.git mysite
cd mysite

# Create virtual environment
python3.11 -m venv .venv

# Activate virtual environment
source .venv/bin/activate

# Install dependencies
pip install -r requirements.txt

echo ""
echo "Setup complete! Now:"
echo "1. Go to Web app settings in PythonAnywhere"
echo "2. Set Source code to: /home/yourusername/mysite"
echo "3. Set Virtualenv to: /home/yourusername/mysite/.venv"
echo "4. Copy the WSGI configuration from pythonanywhere_wsgi.py to the WSGI config file"
echo "5. Click 'Reload' button"
echo ""
echo "Your server will be available at: https://yourusername.pythonanywhere.com"
