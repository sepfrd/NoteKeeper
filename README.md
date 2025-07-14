# NoteKeeper

A modern note management application built with **.NET 9** and **PostgreSQL**, following **Clean Architecture** principles.

---

## ✨ Features

- ✅ **Clean Architecture**
- ✅ **PostgreSQL Database**
- ✅ **JWT Authentication with EdDSA (Ed25519)**
- ✅ **Native Authentication and Google OIDC**
- ✅ **Refresh Token Support**

---

## 🔐 Authentication Details

- **JWT Token Algorithm:** EdDSA (Ed25519)
- **Authentication Providers:**
  - Native (email/password)
  - Google OpenID Connect (OIDC)
- **Refresh Token Workflow:**
  - Access tokens have a short expiration time for security.
  - Refresh tokens are issued alongside access tokens and can be exchanged for new tokens without requiring re-authentication.
  - Refresh token expiration, storage, and revocation mechanisms are handled within the application’s secure infrastructure layer using Redis.

---

## 📐 Tech Stack

- **Backend:** ASP.NET Core (.NET 9)
- **Database:** PostgreSQL
- **Authentication:** JWT (EdDSA) + Google OpenID Connect
- **Architecture Style:** Clean Architecture

---

## ⚙️ Getting Started

### 1️⃣ Prerequisites

- .NET 9 SDK
- Docker & Docker Compose
- PostgreSQL (optional: managed by Docker)

---

## 🚀 Running with Docker Compose

### Step 1: Create an `.env` file

Create a file named `.env` in the root directory of the project with the following content:

```
POSTGRES_PASSWORD=your_postgres_password
REDIS_PASSWORD=your_redis_password
REDIS_USERNAME=default
PRIVATE_KEY=your_ed25519_private_key
PUBLIC_KEY=your_ed25519_public_key
ASPNETCORE_ENVIRONMENT=Development
APPLICATION_HTTP_PORT=8080

GOOGLE_REDIRECT_URI=your_google_redirect_uri
GOOGLE_CLIENT_ID=your_google_client_id
GOOGLE_CLIENT_SECRET=your_google_client_secret
GOOGLE_PROJECT_ID=your_google_project_id
GOOGLE_AUTH_URI=https://accounts.google.com/o/oauth2/auth
GOOGLE_TOKEN_URI=https://oauth2.googleapis.com/token
GOOGLE_REVOKE_URI=https://oauth2.googleapis.com/revoke
GOOGLE_AUTH_PROVIDER_X509_CERT_URL=https://www.googleapis.com/oauth2/v1/certs

NOTION_REDIRECT_URI=your_notion_redirect_uri
NOTION_CLIENT_ID=your_notion_client_id
NOTION_CLIENT_SECRET=your_notion_client_secret
NOTION_AUTH_URI=https://api.notion.com/v1/oauth/authorize
NOTION_TOKEN_URI=https://api.notion.com/v1/oauth/token
```

---

### Step 2: Build and Run

Run the following command:

```bash
docker-compose up --build
```

---

## 🗂️ Project Structure

```
NoteKeeper/
├── src
│   ├── NoteKeeper.Api
│   ├── NoteKeeper.Application
│   ├── NoteKeeper.Domain
│   ├── NoteKeeper.Infrastructure
│   └── NoteKeeper.Shared
├── docker-compose.yaml
├── NoteKeeper.sln.DotSettings.user
├── NoteKeeper.slnx
├──  README.md
└── .env  ➡️  # Your environment variables
```

---

## 🤝 Contributing

Contributions are welcome. Please fork the repository and submit a pull request.

---

## 📄 License

This project is licensed under the MIT License.
