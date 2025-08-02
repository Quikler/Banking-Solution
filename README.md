# Banking Solution REST API

A simple REST API for basic banking operations: creating accounts, deposits, withdrawals, and transfers.

## Technology Stack and Design Choices

- **C# with ASP.NET Core Web API:** Fast, cross-platform framework for building REST APIs.
- **Entity Framework Core:** Simplifies database operations with PostgreSQL using ORM.
- **PostgreSQL:** Reliable open-source relational database suited for transactional data.
- **Testing tools:** xUnit, Shouldly, Moq, Faker, and AutoFixture ensure code quality with automated tests.
- **Microsoft.Identity:** Used for secure handling of JWT authentication and authorization.

### Design Choices

- **Layered architecture:** Separates API, business logic, and data access for maintainability.
- **JWT authentication with Refresh Tokens:** Secure, stateless authentication using access tokens with short lifespan and refresh tokens to obtain new access tokens without re-login.
- **RESTful principles:** Clear, resource-based routes with proper HTTP methods.
- **Docker:** Containerization for easy setup and deployment.

---

## Features

- Create new accounts with an initial balance
- Retrieve account details by account number
- List all accounts
- Deposit funds into an account
- Withdraw funds from an account
- Transfer funds between accounts

---

## Technology Stack

- **Language:** C#
- **Framework:** ASP.NET Core Web API
- **ORM:** Entity Framework Core
- **Testing:** xUnit, Shouldly, Moq, Faker, AutoFixt.
- **Database:** PostgreSQL
- **Authentication:** JWT, Refresh token, Microsoft.Identity

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- Local PostgreSQL installation or Docker

---

### Clone the Repository

```bash
git clone https://github.com/Quikler/Banking-Solution.git
cd Banking-Solution
```

### Running with Docker

1. Navigate to the project root:

    ```bash
    cd Banking-Solution
    ```

2. Run tests to ensure everything is working correctly:

    ```bash
    dotnet test
    ```

3. If tests pass successfully, build and start the application with Docker Compose:

    ```bash
    docker-compose up --build
    ```

4. Once the containers are running, try to open one of the following URLs to access the Swagger API documentation:

    - http://localhost:5039/swagger/index.html
    - http://0.0.0.0:5039/swagger/index.html
    - http://[::1]:5039/swagger/index.html

5. You can log in using the test user credentials:

    ```
    Email: test@test.com
    Password: test@test.com
    ```

    Or create your own user via the signup endpoint.

6. After login or signup, the JWT bearer token will automatically be included in the API key authorization—no need to manually copy/paste.

7. Explore endpoints for deposits, withdrawals, transfers, and more.

### Running Locally

1. Navigate to the project root:

    ```bash
    cd Banking-Solution
    ```

2. Run tests to ensure everything is working correctly (requires Docker for TestContrainers):

    ```bash
    dotnet test
    ```

3. Make sure PostgreSQL is installed and running on your machine.

4. Open the PostgreSQL CLI as the `postgres` user:

    ```bash
    sudo -u postgres psql
    ```

5. In the psql prompt, run the following commands to create a user and set database ownership:

    ```sql
    CREATE USER root WITH PASSWORD 'root';
    ALTER USER root WITH SUPERUSER;
    ALTER DATABASE banking_solution OWNER TO root;
    \q
    ```

6. Update the database schema using EF Core migrations (make sure EF Core CLI tools are installed):

    ```bash
    dotnet ef database update -p src/DAL/DAL.csproj -s src/WebApi/WebApi.csproj
    ```

7. Run the application:

    ```bash
    dotnet run --project src/WebApi/WebApi.csproj
    ```

8. Open Swagger UI to explore the API:

    - http://localhost:5039/swagger/index.html
    - http://0.0.0.0:5039/swagger/index.html

9. Use the test credentials or create your own user through the signup endpoint.

10. After authenticating, the JWT token will be automatically included in API requests.

## Congratulations!

You're now ready to interact with the banking API endpoints for deposits, withdrawals, transfers, and more.

---

If you have any questions or want to contribute, feel free to open an issue or submit a pull request.
