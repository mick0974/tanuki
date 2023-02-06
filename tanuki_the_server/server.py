import random
import socket
from sympy import *
import json

def gcd(a, b):
    while b != 0:
        a, b = b, a % b
    return a

def mod_inverse(a, m):
    for i in range(1, m):
        if (a * i) % m == 1:
            return i
    return None

def generate_key_pair():
    g = 5
    
    p = random.getrandbits(1024)
    while not isprime(p):
       p = random.getrandbits(1024) 
    
    x = random.getrandbits(1024)
    
    y = pow(g, x, mod=p)
    return (p, g, x, y)

def generate_secret(p, g, x, y_client):
    s = (y_client ** x) % p
    return s

def start_server():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(("0.0.0.0", 5000))
    server_socket.listen(1)

    print(f"Server listening on 0.0.0.0:5000")
    
    client_socket, client_address = server_socket.accept()
    print(f"Accepted connection from {client_address[0]}:{client_address[1]}")

    key = None
    while true:
        request = json.loads(client_socket.recv(2048).decode("utf-8"))
        operation = request["Operation"]
        print(f"Operation: {operation}")
        
        if(operation == "keyExchange"):
            print(f"Received request diffie-hellman")
            p, g, x, gx = generate_key_pair()
            print(f"p: {p}")
            print(f"gx: {gx}")
            response = {"Operation": "keyExchange", "Prime": str(p), "Generator": str(g), "Gx_server": str(gx)}
            client_socket.sendall(str(response).encode("utf-8"))
            
        elif(operation == "paramsExchange"):
            print(f"Received params client diffie-hellman")
            key = pow(int(request["Gx_client"]), x, mod=p)
            print(f"Key computed: {key}")

        print("End request")
   
    
    
    exe_path = "C:\\Users\\miche\\source\\repos\\tanuki_the_changer\\tanuki_the_changer\\bin\\Debug\\tanuki_the_changer.exe"
    with open(exe_path, 'rb') as f:
        binary_data = f.read()
        client_socket.sendall(binary_data)

    # Clean up the connection
    client_socket.close()
    server_socket.close()

if __name__ == "__main__":
    start_server()
