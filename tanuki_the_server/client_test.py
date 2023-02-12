import hashlib
import json
import socket
import random

from cryptography.hazmat.primitives import hashes
from pbkdf2 import pbkdf2
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes

from tanuki_the_server.server import unpad_data

HOST = "127.0.0.1"
PORT = 65432
MAX_BUFFER_SIZE = 4096
GEN_KEY_LENGTH = 1024
SALT = b'\x00\x03\x90\xfc\xbfm\xe4\x9e\x9a\xe2K\x04\x0cFaV'
KEY_LENGTH = 256
IV = b'9\x97\x8fb\xa0\x82\xe1?\xadb\x89\x11,\xcc\xff\t'


def generate_key_pair(g, p, b):
    return pow(g, b, mod=p)


def generate_random_num():
    return random.getrandbits(GEN_KEY_LENGTH)


with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.connect((HOST, PORT))

    request = {"Operation": "keyExchangeGen"}

    s.sendall(bytes(json.dumps(request), encoding="utf-8"))

    prime = 0
    generator = 0
    gx = 0

    data = b""
    request = json.loads(s.recv(MAX_BUFFER_SIZE).decode("utf-8"))

    operation = request["Operation"]

    if operation == "keyExchange":
        prime = int(request["Prime"])
        generator = int(request["Generator"])
        y_server = int(request["Gx_server"])

        dh_secret = generate_random_num()

        y_client = generate_key_pair(generator, prime, dh_secret)

        dh_key = pow(int(y_server), dh_secret, mod=prime)

        request = {"Operation": "keyExchangeAns", "Gx_client": f"{y_client}"}
        s.sendall(bytes(json.dumps(request), encoding="utf-8"))

        size = int((GEN_KEY_LENGTH + 7) / 8)
        aes_key = pbkdf2(hashlib.sha256, b"".fromhex(hex(dh_key)[2:]), SALT, 480000, 32)

        print(f"Key: {dh_key}")
        print(f"AES_key: {aes_key}")

        cipher_text = b''
        while True:
            new_data = s.recv(MAX_BUFFER_SIZE)
            print(new_data)

            if not new_data:
                break

            cipher_text += new_data

        print(cipher_text)

        cipher = Cipher(algorithms.AES(aes_key), modes.CBC(IV))
        decryptor = cipher.decryptor()
        clear_text = decryptor.update(cipher_text) + decryptor.finalize()

        print(unpad_data(clear_text))






print(f"Received {data}")
