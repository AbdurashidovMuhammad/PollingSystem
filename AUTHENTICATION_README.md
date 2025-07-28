# Authentication System Documentation

## Overview
This authentication system provides comprehensive user authentication and authorization features including sign up, login, email verification, password reset, and JWT token management. The system uses direct database access in services instead of repository pattern.

## Features

### 1. User Registration (Sign Up)
- **Endpoint**: `POST /api/auth/signup`
- **Request Body**:
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "Password123",
  "confirmPassword": "Password123"
}
```
- **Response**: Returns JWT access token, refresh token, and user information
- **Features**:
  - Password hashing with salt
  - Email verification OTP generation
  - Welcome email sending
  - Duplicate email validation
  - Refresh token storage in database

### 2. User Login
- **Endpoint**: `POST /api/auth/login`
- **Request Body**:
```json
{
  "email": "john@example.com",
  "password": "Password123"
}
```
- **Response**: Returns JWT access token, refresh token, and user information

### 3. Email Verification
- **Endpoint**: `POST /api/auth/verify-email`
- **Request Body**:
```json
{
  "email": "john@example.com",
  "otpCode": "123456"
}
```
- **Features**: 6-digit OTP validation with expiry

### 4. Resend Verification Email
- **Endpoint**: `POST /api/auth/resend-verification`
- **Request Body**:
```json
{
  "email": "john@example.com"
}
```

### 5. Password Reset
- **Forgot Password**: `POST /api/auth/forgot-password`
- **Reset Password**: `POST /api/auth/reset-password`
- **Request Body**:
```json
{
  "email": "john@example.com",
  "otpCode": "123456",
  "newPassword": "NewPassword123",
  "confirmPassword": "NewPassword123"
}
```

### 6. Token Management
- **Refresh Token**: `POST /api/auth/refresh-token`
- **Revoke Token**: `POST /api/auth/revoke-token`

### 7. User Profile
- **Get Profile**: `GET /api/user/profile` (Requires Authorization header)
- **Get Current User**: `GET /api/user/me` (Requires Authorization header)
- **Get All Users**: `GET /api/user/all` (Requires Authorization header)

## Architecture

### Layers
1. **API Layer** (`Polling.Api`)
   - Controllers handling HTTP requests
   - Middleware for JWT authentication
   - Request/response handling

2. **Application Layer** (`Polling.Application`)
   - Business logic services with direct database access
   - DTOs for data transfer
   - Validation using FluentValidation
   - Interfaces for dependency injection

3. **Data Access Layer** (`Polling.DataAccess`)
   - Entity Framework context
   - Database operations
   - No repository pattern - direct database access

4. **Core Layer** (`Polling.Core`)
   - Entity models
   - Enums
   - Domain logic

### Key Components

#### Services (Direct Database Access)
- **AuthService**: Main authentication business logic with direct AppDbContext usage
- **TokenService**: JWT token generation and validation with database access
- **EmailService**: SMTP email sending functionality

#### DTOs
- **SignUpRequest**: User registration data
- **LoginRequest**: User login credentials
- **AuthResponse**: Authentication response with tokens
- **UserDto**: User information transfer object

## Database Access Pattern

The system uses **direct database access** in services instead of repository pattern:

```csharp
// Service constructor with AppDbContext
public AuthService(AppDbContext context, ITokenService tokenService, IEmailService emailService)
{
    _context = context;
    _tokenService = tokenService;
    _emailService = emailService;
}

// Direct database operations
var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
_context.Users.Add(newUser);
await _context.SaveChangesAsync();
```

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-with-at-least-32-characters-for-jwt-signing",
    "Issuer": "PollingSystem",
    "Audience": "PollingSystem"
  }
}
```

### SMTP Settings (appsettings.json)
```json
{
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com"
  }
}
```

## Security Features

### Password Security
- HMAC-SHA512 hashing with salt
- Minimum 6 characters with complexity requirements
- Secure password validation

### JWT Security
- HMAC-SHA256 signing
- Configurable expiry times
- Refresh token mechanism with database storage
- Token revocation capability

### Email Security
- 6-digit OTP codes
- 10-minute expiry for verification codes
- Secure SMTP configuration

## Database Entities

### User Entity
```csharp
public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    public UserRole Role { get; set; }
    public bool IsEmailVerified { get; set; }
    public List<Vote>? Votes { get; set; }
}
```

### EmailVerification Entity
```csharp
public class EmailVerification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OtpCode { get; set; }
    public DateTime ExpiryTime { get; set; }
    public bool IsUsed { get; set; }
    public User User { get; set; }
}
```

### RefreshToken Entity
```csharp
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Token { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public User User { get; set; }
}
```

## Usage Examples

### 1. User Registration
```bash
curl -X POST "https://localhost:7001/api/auth/signup" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "email": "john@example.com",
    "password": "Password123",
    "confirmPassword": "Password123"
  }'
```

### 2. User Login
```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123"
  }'
```

### 3. Get User Profile (Authenticated)
```bash
curl -X GET "https://localhost:7001/api/user/profile" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Get All Users
```bash
curl -X GET "https://localhost:7001/api/user/all" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Error Handling

The system provides comprehensive error handling:
- Validation errors with detailed messages
- Authentication errors
- Database errors
- Email sending errors

All errors are returned in a consistent format:
```json
{
  "message": "Error description"
}
```

## Dependencies

### NuGet Packages
- `Microsoft.AspNetCore.Identity`
- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.IdentityModel.Tokens`
- `FluentValidation.AspNetCore`
- `AutoMapper.Extensions.Microsoft.DependencyInjection`

## Setup Instructions

1. **Configure Database**: Update connection string in `appsettings.json`
2. **Configure JWT**: Set your JWT secret key in `appsettings.json`
3. **Configure SMTP**: Update SMTP settings for email functionality
4. **Run Migrations**: Ensure database is up to date
5. **Start Application**: Run the API project

## Key Changes from Repository Pattern

### Before (Repository Pattern)
```csharp
// Service with repository dependency
public AuthService(IUserRepository userRepository, ...)
{
    _userRepository = userRepository;
}

// Using repository
var user = await _userRepository.GetByEmailAsync(email);
await _userRepository.AddAsync(user);
```

### After (Direct Database Access)
```csharp
// Service with direct AppDbContext dependency
public AuthService(AppDbContext context, ...)
{
    _context = context;
}

// Direct database operations
var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
_context.Users.Add(user);
await _context.SaveChangesAsync();
```

## Notes

- The system uses Entity Framework Core with PostgreSQL
- JWT tokens have 1-hour expiry for access tokens and 7-day expiry for refresh tokens
- Email verification OTP codes expire after 10 minutes
- All passwords are hashed using HMAC-SHA512 with salt
- The system supports multiple user roles (User, Admin, SuperAdmin)
- **No repository pattern** - direct database access in services
- Refresh tokens are stored in database for proper management 