"""Exercise connected provider status using a local, signed OIDC test issuer."""

import base64
import json
import os
import subprocess
import tempfile
import threading
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen


API = "http://127.0.0.1:5082"
CLIENT_ID = "ci-only-client"
PROFILES = {}


def encode(value):
    return base64.urlsafe_b64encode(value).rstrip(b"=").decode()


def encode_json(value):
    return encode(json.dumps(value, separators=(",", ":")).encode())


class IssuerHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        issuer = f"http://127.0.0.1:{self.server.server_port}"
        if self.path == "/.well-known/openid-configuration":
            data = {
                "issuer": issuer,
                "jwks_uri": f"{issuer}/jwks",
                "authorization_endpoint": f"{issuer}/authorize",
                "token_endpoint": f"{issuer}/token",
                "response_types_supported": ["code"],
                "subject_types_supported": ["public"],
                "id_token_signing_alg_values_supported": ["RS256"],
            }
        elif self.path == "/jwks":
            data = {"keys": [self.server.jwk]}
        elif self.path == "/api/get-account":
            data = PROFILES.get(self.headers.get("Authorization", ""))
            if data is None:
                self.send_error(401)
                return
        else:
            self.send_error(404)
            return
        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        self.end_headers()
        self.wfile.write(json.dumps(data).encode())

    def log_message(self, *_):
        pass


def request(path, token=None, body=None, expected=200, casdoor_token=None):
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    if casdoor_token:
        headers["X-Casdoor-Token"] = casdoor_token
    req = Request(API + path, headers=headers,
                  data=None if body is None else json.dumps(body).encode())
    try:
        with urlopen(req, timeout=15) as response:
            assert response.status == expected, (path, response.status, expected)
            raw = response.read()
            return json.loads(raw) if raw else None
    except HTTPError as error:
        assert error.code == expected, (path, error.code, expected)


def mint(key, issuer, subject, email, google, github, profile_subject=None):
    now = int(time.time())
    payload = {
        "iss": issuer, "aud": CLIENT_ID, "sub": subject,
        "email": email, "email_verified": True, "name": "CI provider user",
        "tokenType": "access-token", "iat": now, "exp": now + 600,
    }
    unsigned = ".".join([
        encode_json({"alg": "RS256", "kid": "test", "typ": "JWT"}),
        encode_json(payload),
    ])
    signature = subprocess.check_output(
        ["openssl", "dgst", "-sha256", "-sign", str(key)], input=unsigned.encode()
    )
    token = unsigned + "." + encode(signature)
    identity = profile_subject or subject
    PROFILES[f"Bearer {token}"] = {
        "status": "ok", "sub": identity,
        "data": {"id": identity, "google": google, "github": github},
    }
    return token


def check_status(token, google, github):
    account = request("/api/accounts/me", token)
    assert account["googleConnected"] is google, "Unexpected Google status"
    assert account["githubConnected"] is github, "Unexpected GitHub status"
    return account["id"]


def run_tests(key, issuer):
    local_email = "ci.connected.local@example.com"
    request("/api/auth/register", body={"email": local_email,
            "displayName": "Local user", "kataSandi": "Testing123!"}, expected=201)
    local = request("/api/auth/login", body={"email": local_email,
                    "kataSandi": "Testing123!"})["accessToken"]
    local_id = check_status(local, False, False)

    def exchange(subject, email, google, github, profile_subject=None):
        token = mint(key, issuer, subject, email, google, github, profile_subject)
        return request("/api/auth/casdoor/exchange", token, body={})["accessToken"]

    email = "ci.connected.external@example.com"
    first = exchange("ci-subject", email, "google-id", "")
    account_id = check_status(first, True, False)
    second = exchange("ci-subject", email, "google-id", "github-id")
    assert check_status(second, True, True) == account_id

    mismatch = exchange("ci-subject", email, "", "", "other-subject")
    assert check_status(mismatch, True, True) == account_id
    unknown = exchange("ci-other", "ci.connected.other@example.com", "id", "id", "wrong")
    assert check_status(unknown, None, None) != account_id

    request(f"/api/accounts/{account_id}", local, expected=403)
    request("/api/accounts/me", expected=401)
    linked = mint(key, issuer, "ci-local", local_email, "", "github-id")
    request("/api/auth/casdoor/link", local, body={"kataSandi": "Testing123!"},
            casdoor_token=linked, expected=204)
    assert check_status(local, False, True) == local_id
    conflict = mint(key, issuer, "ci-subject", email, "google-id", "github-id")
    request("/api/auth/casdoor/link", local, body={"kataSandi": "Testing123!"},
            casdoor_token=conflict, expected=409)
    assert check_status(local, False, True) == local_id
    assert check_status(second, True, True) == account_id
    print("Connected provider status, ownership, secure linking and failure handling passed")


def main():
    with tempfile.TemporaryDirectory() as directory:
        key = Path(directory) / "issuer.pem"
        subprocess.run(["openssl", "genrsa", "-out", str(key), "2048"], check=True)
        modulus = subprocess.check_output(["openssl", "rsa", "-in", str(key),
                                           "-noout", "-modulus"], text=True).strip()
        with ThreadingHTTPServer(("127.0.0.1", 0), IssuerHandler) as server:
            server.jwk = {"kty": "RSA", "kid": "test", "use": "sig", "alg": "RS256",
                          "n": encode(bytes.fromhex(modulus.split("=", 1)[1])), "e": "AQAB"}
            issuer = f"http://127.0.0.1:{server.server_port}"
            thread = threading.Thread(target=server.serve_forever, daemon=True)
            thread.start()
            log = Path(directory) / "api.log"
            env = dict(os.environ, ASPNETCORE_ENVIRONMENT="Development",
                       CASDOOR_ISSUER=issuer, CASDOOR_CLIENT_ID=CLIENT_ID)
            with log.open("w") as output:
                process = subprocess.Popen([
                    "dotnet", "backend/AccessConsole.Api/bin/Release/net10.0/AccessConsole.Api.dll",
                    "--urls", API,
                ], env=env, stdout=output, stderr=subprocess.STDOUT)
                try:
                    for _ in range(30):
                        try:
                            with urlopen(API + "/health/ready", timeout=2) as response:
                                if response.status == 200:
                                    break
                        except (URLError, TimeoutError):
                            time.sleep(1)
                    else:
                        raise RuntimeError("Backend did not become ready")
                    run_tests(key, issuer)
                except Exception:
                    print(log.read_text())
                    raise
                finally:
                    process.terminate()
                    process.wait(timeout=10)
                    server.shutdown()


if __name__ == "__main__":
    main()
