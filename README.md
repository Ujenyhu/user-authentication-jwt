# User Authentication Service

The **User Authentication Service** is a scalable API built using **C#** designed to handle user authentication and management. 
It includes essential features such as account registration, secure login, OTP verification, password management, and detailed incident notification. 
The service uses advanced security measures, including **account locking** for suspicious activity and notifications with **device location details**.


## Features

### Core Features
- **User Registration**: Securely register users with validation and hashed passwords.
- **Sign In**: Authenticate users with JWT-based tokens.
- **JWT Management**: Issue, refresh, and validate access and refresh tokens.
- **Account Locking**: Lock user accounts after three failed attempts on sensitive actions like login or password changes.
- **Incident Notifications**: Send email alerts for suspicious activity with details like device location.

### Additional Features
- **OTP Management**: Generate and validate one-time passwords for sensitive actions.
- **Password Change/Reset**: Allow users to securely reset their passwords.
- **Request Validation**: Validate all incoming requests to ensure data integrity.
- **Event Notifications**: Notify users via **EMAIL** or **SMS** for account-related activities using services like MimeKit, SMTP, Twilio and TermiiSMS.
- **API Documentation**: Detailed API documentation using Swagger (Swashbuckle).
- **Lookup Service**: This is a service provision to retrieve or lookup values for constant or dynamic metadata/variable of the system.

### Security Features
- **Rate Limiting**: Prevents brute-force attacks using AspNetCoreRateLimit.
- **CORS Policy**: Restricts API access to trusted domains.
- **Data Encryption**: Secure encryption of stored passwords and authentication tokens.
- **Refresh Token Management**: Securely stores refresh tokens and implements token expiration policies.

## Technologies Used

- **.NET 7**
- **Entity Framework Core**: ORM for database interactions.
- **Swagger (Swashbuckle)**: For API documentation.
- **JWT (Json Web Token)**: For secure user authentication.
- **Twilio, Termii, MimeKit & SMTP**: For SMS and email notifications.
- **MSSQL**: Database Management System.
- **Redis**: Caching for increased API performance.

## Architecture
I used the Repository Pattern.

- **Controllers**: Handle HTTP requests and responses.
- **Services**: Business logic for authentication, user management, caching etc.
- **Repositories**: Database access and operations.
- **Models**: Data structures for requests, responses, and database entities.
- **Helpers**: For Static and Generic methods.

# USAGE
This code implementation can be ued as a guide to implement similars feature. Please do not copy code verbatim, endeavour to tailor your implemntation
to meet the requirements of your project or user experience.

