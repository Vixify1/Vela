# Vela HR Management System

A comprehensive Human Resources management web application built with ASP.NET Core MVC.

hrwebapp-vela.azurewebsites.net

## Features

- **Employee Management**: Complete employee lifecycle management
- **Time & Attendance**: Clock in/out system with detailed reporting
- **Payroll Processing**: Automated payroll calculations with tax deductions
- **Holiday Management**: Company holiday calendar and management
- **Department Management**: Organize employees by departments
- **User Authentication**: Role-based access control (Admin/Employee)
- **Dashboard Analytics**: Real-time insights and statistics

## Technology Stack

- **Backend**: ASP.NET Core MVC (.NET 8)
- **Database**: Entity Framework Core with SQL Server
- **Frontend**: Bootstrap 5, HTML5, CSS3, JavaScript
- **Authentication**: ASP.NET Core Identity
- **PDF Generation**: iTextSharp for salary letters

## 📸 Screenshots

### 🏠 Landing Page
![Landing Page](Screenshots/Landing-Page.PNG)
*Clean and modern landing page with user authentication and company branding*

### 📊 Dashboard
![Dashboard](Screenshots/Dashboard.PNG)
*Comprehensive dashboard overview with key metrics, quick actions, and activity tracking*

### 👥 Employee Management
![Employee Management](Screenshots/employee-management.PNG)
*Complete employee management system with detailed profiles, department assignments, and administrative controls*

### 📅 Holiday Calendar
![Holiday Calendar](Screenshots/holiday-calendar.PNG)
*Interactive holiday calendar for managing company holidays and employee time off*

### 💰 Payroll Management
![Payroll Management](Screenshots/payroll.PNG)
*Advanced payroll processing system with salary calculations, tax deductions, and payment tracking*


### Local Development Setup

1. **Clone the repository**
   
3. **Configure Database Connection**
   
   Update `appsettings.json` with your local SQL Server connection:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HRWebApp;Trusted_Connection=True;MultipleActiveResultSets=true;"
     }
   }
   ```

4. **Install Dependencies**
   dotnet restore

5. **Run Database Migrations**

6. **Access the Application**
   - Open your browser and navigate to `https://localhost:5001`
   - The application will automatically seed with initial data

### Default Login Credentials
After initial setup, you can use the seeded data (check `DbSeeder.cs` for default credentials).

Login: admin@hrwebapp.com
Admin123!

In authentication controller you can enable admin registration to register additional admin users.  

## Developer

*Developed by Uolter Ferhati* 
