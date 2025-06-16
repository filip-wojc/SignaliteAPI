# Signalite API
![.NET](https://img.shields.io/badge/.NET-9.0-blue) ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue) ![Redis](https://img.shields.io/badge/Redis-7-red) ![Docker](https://img.shields.io/badge/Docker-Enabled-blue) ![SignalR](https://img.shields.io/badge/SignalR-Real--time-green)

A real-time messaging API built with ASP.NET Core, featuring WebRTC voice/video calls, file sharing, and presence tracking across multiple instances. Created to use with [SignaliteClient](https://github.com/Signalite-org/SignaliteClient).


## Authors
- [DarknesoPirate](https://github.com/DarknessoPirate)
- [filip_wojc](https://github.com/filip-wojc)
- [RobertPintera](https://github.com/RobertPintera)
## Key Features

- **Real-time Messaging** - Instant messaging with file attachment support
- **WebRTC Voice/Video Calls** - Peer-to-peer audio and video communication
- **Multi-Instance Presence Tracking** - User presence management across distributed deployments
- **Friend System** - Send, accept, and manage friend requests
- **Group Management** - Create and manage group conversations
- **File Support** - Images, videos, audio files, and documents with automatic storage routing
- **Real-time Notifications** - Live updates for all user interactions
- **Health Monitoring** - Built-in health checks for all dependencies

## Architecture

Built using **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────┐
│   API Layer     │  ← Controllers, Middleware
├─────────────────┤
│ Application     │  ← MediatR Commands/Queries, Validation,  Mappers
├─────────────────┤
│ Infrastructure  │  ← SignalR Hubs, Services, Database, External APIs, Repositories
├─────────────────┤
│   Domain        │  ← Entities, DTOs , Enums,
└─────────────────┘

Each layer has it's own extension functions to register/configure their respective services.
Each layer has it's own specific exceptions defined.
```

## Tech Stack

### Core Technologies

- **ASP.NET Core 9.0** - Web API framework
- **SignalR** - Real-time web functionality
- **Entity Framework Core** - ORM with PostgreSQL
- **MediatR** - CQRS pattern implementation
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation

### Storage & Caching

- **PostgreSQL** - Primary database 
- **Redis** - Connection tracking
- **Cloudinary** - Media storage (images, videos, audio)
- **Static File Storage** - Other file type storage 

### DevOps & Monitoring
- **Docker & Docker Compose** - Containerization
- **Serilog** - Structured logging with custom themes
- **Scalar** - Interactive API documentation

### Authentication & Security

- **JWT Bearer** - Token-based authentication
- **Refresh Tokens** - Secure token renewal
- **Password Hashing** - ASP.NET Core Identity
## Quick Start

### Prerequisites

- Docker & Docker Compose
- .NET 9.0 SDK (for development)

### Environment Setup

1. **Clone the repository**

```bash
git clone <repository-url>
cd signalite-api
```

2. **Configure environment variables** Edit the `.env` file with your configuration:

```env
# Database Configuration
POSTGRES_PASSWORD=your_strong_password

# API Configuration  
TOKEN_KEY=your_jwt_secret_key_min_64_chars

# Cloudinary Settings
CLOUDINARY_CLOUD_NAME=your_cloudinary_name
CLOUDINARY_API_KEY=your_api_key
CLOUDINARY_API_SECRET=your_api_secret

# Docker Image
DOCKER_IMAGE=your_dockerhub_username/signalite-api:latest

# Port Configuration (optional)
API_PORT=5000
POSTGRES_PORT=5432
REDIS_PORT=6379
```

3. **Deploy with Docker Compose**

```bash
docker-compose up -d
```

The API will be available at `http://localhost:5000` with:

- **API Documentation**: `http://localhost:5000/scalar/v1`

##  SignalR Hubs

The API features three specialized SignalR hubs for real-time functionality:

### PresenceHub (`/hubs/presence`)

- User online/offline status tracking
- Multi-instance connection management
- Automatic cleanup of stale connections

### NotificationsHub (`/hubs/notifications`)

- Real-time message notifications
- Friend request notifications
- Group update notifications

### SignalingHub (`/hubs/signaling`)

- WebRTC signaling for voice/video calls
- Peer-to-peer connection establishment
- Call management (offers, answers, ICE candidates)

## Notable Technical Features

### File Handling

The API automatically routes different file types to storage solutions:

- **Images/Videos/Audio** → Cloudinary (with chunked upload for large files)
- **Documents/Other files** → Local static file storage

### Multi-Instance Presence Tracking

Redis-based system that:

- Tracks user connections across multiple API instances
- Performs automatic cleanup of dead instances
- Validates connections with keep-alive mechanisms
- Handles shutdown and cleanup

### Transaction Safety

Error handling with automatic rollback:

```csharp
// Example from SendMessageHandler
try {
    await unitOfWork.BeginTransactionAsync();
    // ... operations
    await unitOfWork.CommitTransactionAsync();
} catch {
    await unitOfWork.RollbackTransactionAsync();
    // Cleanup external resources (Cloudinary, etc.)
    throw;
}
```

### Custom Logging

Console logging with custom color themes and structured file logging for critical errors.

## Development

### Building and publishing the Docker Image

```bash
docker build -t your_username/signalite-api:latest .
docker push your_username/signalite-api:latest
```

### Running in Development Mode

```bash
dotnet restore
dotnet run --project SignaliteWebAPI
```

### Database Migrations

```bash
dotnet ef migrations add MigrationName --project SignaliteWebAPI.Infrastructure
dotnet ef database update --project SignaliteWebAPI
```

## API Endpoints

### Authentication

- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh-token` - Token refresh
- `GET /api/auth/user-exists-by-username` - Check if username exists
- `GET /api/auth/user-exists-by-email` - Check if email exists

### Users 🔒

- `PUT /api/user/modify-user` - Update user profile
- `GET /api/user/get-user-info` - Get user information (own or by userId)
- `GET /api/user/{username}` - Get user by username
- `PUT /api/user/change-password` - Change password
- `POST /api/user/profile-photo` - Upload profile photo
- `DELETE /api/user/profile-photo` - Delete profile photo
- `POST /api/user/bg-photo` - Upload background photo
- `DELETE /api/user/bg-photo` - Delete background photo
- `GET /api/user/user-exists/{username}` - Check if user exists by username

### Friends 🔒

- `POST /api/friends/friend-request/{username}` - Send friend request
- `GET /api/friends/friend-requests` - Get pending friend requests
- `POST /api/friends/friend-request/accept/{friendRequestId}` - Accept friend request
- `DELETE /api/friends/friend-request/decline/{friendRequestId}` - Decline friend request
- `GET /api/friends` - Get user's friends list

### Groups 🔒

- `POST /api/groups` - Create group (with groupName query parameter)
- `GET /api/groups` - Get user's groups
- `PUT /api/groups/{groupId}` - Modify group name
- `POST /api/groups/photo/{groupId}` - Upload group photo
- `GET /api/groups/{groupId}/basic-info` - Get group basic information
- `GET /api/groups/{groupId}/members` - Get group members
- `DELETE /api/groups/{groupId}` - Delete group (owner only)
- `POST /api/groups/{groupId}/users/{userId}` - Add user to group
- `DELETE /api/groups/{groupId}/users/{userId}` - Remove user from group

### Messages 🔒

- `POST /api/message` - Send message with optional attachment
- `GET /api/message/{groupId}` - Get message thread (paginated)
- `PUT /api/message/{messageId}` - Edit/modify message
- `DELETE /api/message/{messageId}` - Delete message
- `DELETE /api/message/{messageId}/attachment` - Delete message attachment

### WebRTC 🔒

- `GET /api/webrtc/ice-servers` - Get ICE server configuration

🔒 = Requires JWT authentication

## Logging

Structured logging with Serilog featuring:

- **Console logging** with custom color themes
- **File logging** for critical errors
- **Request/Response logging** for API calls
- **Structured logging** for easy parsing

## License

This project is part of an academic project by [DarknesoPirate](https://github.com/DarknessoPirate), [filip_wojc](https://github.com/filip-wojc), [RobertPintera](https://github.com/RobertPintera) 
