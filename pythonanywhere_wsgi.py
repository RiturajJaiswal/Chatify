# PythonAnywhere WSGI configuration
# Copy this to /var/www/yourusername_pythonanywhere_com_wsgi.py

import sys
import os

# Add the project directory to the sys.path
project_home = os.path.expanduser('~/mysite')  # or wherever your project is
if project_home not in sys.path:
    sys.path.insert(0, project_home)

# Activate virtual environment
activate_this = os.path.join(project_home, '.venv', 'bin', 'activate_this.py')
with open(activate_this) as f:
    exec(f.read(), {'__file__': activate_this})

# Import the ASGI app
from server.main import app

# WSGI application
application = app
