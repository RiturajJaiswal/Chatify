import nacl.utils
from nacl.public import PrivateKey, Box, PublicKey
import base64

class CryptoManager:
    def __init__(self):
        self.private_key = PrivateKey.generate()
        self.public_key = self.private_key.public_key
        
    def get_public_key_bytes(self):
        return self.public_key.encode()
    
    def get_public_key_b64(self):
        return base64.b64encode(self.public_key.encode()).decode('utf-8')

    def create_box(self, peer_public_key_b64):
        peer_pk_bytes = base64.b64decode(peer_public_key_b64)
        peer_public_key = PublicKey(peer_pk_bytes)
        return Box(self.private_key, peer_public_key)
    
    def encrypt(self, box, message):
        encrypted = box.encrypt(message.encode('utf-8'))
        return base64.b64encode(encrypted).decode('utf-8')
    
    def decrypt(self, box, encrypted_message_b64):
        encrypted = base64.b64decode(encrypted_message_b64)
        plaintext = box.decrypt(encrypted)
        return plaintext.decode('utf-8')
