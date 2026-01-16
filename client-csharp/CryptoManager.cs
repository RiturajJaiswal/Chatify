using System;
using System.Text;
using Sodium;

namespace ChatifyClient
{
    public class CryptoManager
    {
        private KeyPair _keyPair;

        public CryptoManager()
        {
            _keyPair = PublicKeyBox.GenerateKeyPair();
        }

        public string GetPublicKeyBase64()
        {
            return Convert.ToBase64String(_keyPair.PublicKey);
        }

        // Encrypts a message for a specific recipient's public key
        public string Encrypt(string recipientPublicKeyB64, string message)
        {
            var recipientPk = Convert.FromBase64String(recipientPublicKeyB64);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var nonce = PublicKeyBox.GenerateNonce();
            
            var ciphertext = PublicKeyBox.Create(messageBytes, nonce, _keyPair.PrivateKey, recipientPk);
            
            // PyNaCl Box.encrypt prepends the 24-byte nonce to the ciphertext
            var combined = new byte[nonce.Length + ciphertext.Length];
            Array.Copy(nonce, 0, combined, 0, nonce.Length);
            Array.Copy(ciphertext, 0, combined, nonce.Length, ciphertext.Length);
            
            return Convert.ToBase64String(combined);
        }

        // Decrypts a message from a sender
        public string Decrypt(string senderPublicKeyB64, string encryptedMessageB64)
        {
            var senderPk = Convert.FromBase64String(senderPublicKeyB64);
            var combined = Convert.FromBase64String(encryptedMessageB64);
            
            if (combined.Length < 24) throw new ArgumentException("Message too short");

            var nonce = new byte[24];
            var ciphertext = new byte[combined.Length - 24];
            
            Array.Copy(combined, 0, nonce, 0, 24);
            Array.Copy(combined, 24, ciphertext, 0, ciphertext.Length);
            
            var messageBytes = PublicKeyBox.Open(ciphertext, nonce, _keyPair.PrivateKey, senderPk);
            return Encoding.UTF8.GetString(messageBytes);
        }
    }
}
