import socketio
import uvicorn
from fastapi import FastAPI

app = FastAPI()
sio = socketio.AsyncServer(async_mode='asgi', cors_allowed_origins='*')
socket_app = socketio.ASGIApp(sio, app)

# Store connected clients: {sid: {username: str, public_key: str}}
clients = {}

@sio.event
async def connect(sid, environ):
    print(f"Client connected: {sid}")

@sio.event
async def disconnect(sid):
    if sid in clients:
        username = clients[sid]['username']
        del clients[sid]
        print(f"Client disconnected: {username} ({sid})")
        # Broadcast updated user list
        await broadcast_user_list()

@sio.event
async def register(sid, data):
    # data: {'username': str, 'public_key': str}
    username = data.get('username')
    public_key = data.get('public_key')
    
    clients[sid] = {'username': username, 'public_key': public_key}
    print(f"Registered user: {username}")
    
    await broadcast_user_list()

async def broadcast_user_list():
    users = [{'sid': sid, 'username': info['username'], 'public_key': info['public_key']} 
             for sid, info in clients.items()]
    await sio.emit('user_list', users)

@sio.event
async def private_message(sid, data):
    # data: {'to_sid': str, 'encrypted_message': str, 'from_username': str}
    to_sid = data.get('to_sid')
    encrypted_message = data.get('encrypted_message')
    
    if to_sid in clients:
        await sio.emit('private_message', {
            'from_sid': sid,
            'from_username': clients[sid]['username'],
            'encrypted_message': encrypted_message
        }, room=to_sid)

if __name__ == "__main__":
    uvicorn.run("main:socket_app", host="0.0.0.0", port=8000, reload=True)
