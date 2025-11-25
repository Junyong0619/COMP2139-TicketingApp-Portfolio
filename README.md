# GBC Ticketing System - Documentation

## Project Overview

This is a group assignment project for COMP2139 - GBC Ticketing System.

## Group1 Team Members

- **Joosung Ahn (101539659)**
- **Kiana Sepasian (101475855)**
- **Junyong Choi (101539862)**

## Team Collaboration Guidelines

### Project Structure

```
GBC_Ticketing/
├── Controllers/     # MVC Controllers
├── Models/         # Data models and entities
├── Views/          # Razor views and UI components
├── Data/           # Database context and migrations
├── Services/       # Business logic services
└── wwwroot/        # Static files (CSS, JS, images)
```

## Getting Started

1. Clone the repository
2. Open project in Rider (NuGet packages will restore automatically)
3. Create a PostgreSQL database named `GbcTicketingDB`
4. Copy `appsettings.json` to `appsettings.Development.json` and update with your database settings:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=GbcTicketingDB;Username=postgres;Password=your_password"
     }
   }
   ```
   (appsettings.Development.json is gitignored, so each team member can have their own settings)
5. Set up the database (First time setup):

   ```bash
   # Build the project first
   dotnet build

   # Create initial migration (only needed once)
   dotnet ef migrations add InitialCreate

   # Apply migrations to create database
   dotnet ef database update
   ```

6. Run the application in Rider

### Database Migration Commands (Reference)

- **Create a new migration**: `dotnet ef migrations add <MigrationName>`
- **Apply migrations to database**: `dotnet ef database update`
- **Remove last migration**: `dotnet ef migrations remove`
- **View migration history**: `dotnet ef migrations list`
