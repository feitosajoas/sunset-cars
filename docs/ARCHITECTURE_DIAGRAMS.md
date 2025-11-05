# 🏗️ Diagramas de Arquitetura - Sunset Cars

## 📊 Arquitetura Geral do Sistema

```
┌─────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                       │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   Razor Views   │  │ API Controllers │  │   Static Files  │  │
│  │                 │  │                 │  │                 │  │
│  │ • Login.cshtml  │  │ • /api/sales    │  │ • Bootstrap 5   │  │
│  │ • Index.cshtml  │  │ • /api/vehicles │  │ • Custom CSS    │  │
│  │ • Create.cshtml │  │ • /api/customers│  │ • JavaScript    │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                      APPLICATION LAYER                          │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ MVC Controllers │  │   DTOs & VMs    │  │   Extensions    │  │
│  │                 │  │                 │  │                 │  │
│  │ • SalesCtrl     │  │ • CreateSaleReq │  │ • ServiceColl   │  │
│  │ • VehiclesCtrl  │  │ • SaleDto       │  │ • WebApp        │  │
│  │ • CustomersCtrl │  │ • Result<T>     │  │ • AuthConfig    │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                       BUSINESS LAYER                            │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ Domain Services │  │Infrastructure   │  │   Validators    │  │
│  │                 │  │   Services      │  │                 │  │
│  │ • SaleService   │  │ • CacheService  │  │ • Business      │  │
│  │ • VehicleService│  │ • CepService    │  │   Rules         │  │
│  │ • CustomerSvc   │  │ • CurrentUser   │  │ • Input Valid   │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                      PERSISTENCE LAYER                          │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  Repositories   │  │  Unit of Work   │  │  Domain Models  │  │
│  │                 │  │                 │  │                 │  │
│  │ • SaleRepo      │  │ • UnitOfWork    │  │ • Sale          │  │
│  │ • VehicleRepo   │  │ • Transaction   │  │ • Vehicle       │  │
│  │ • CustomerRepo  │  │ • SaveChanges   │  │ • Customer      │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                       DATABASE LAYER                            │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ Entity Framework│  │    SQLite DB    │  │    Identity     │  │
│  │                 │  │                 │  │                 │  │
│  │ • DbContext     │  │ • sunsetcars.db │  │ • AspNetUsers   │  │
│  │ • Migrations    │  │ • Tables        │  │ • AspNetRoles   │  │
│  │ • Configurations│  │ • Indexes       │  │ • Claims        │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## 🔄 Fluxo de Dados - Criar Venda

```
[User] → [Login Page] → [Dashboard] → [Create Sale Form]
                                            │
                                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    REQUEST FLOW                                 │
└─────────────────────────────────────────────────────────────────┘

1. USER INPUT
   ┌─────────────────┐
   │ Manufacturer    │ ──┐
   │ Vehicle         │   │
   │ Customer        │   │ POST /api/sales
   │ Price           │   │
   │ Date            │ ──┘
   └─────────────────┘
            │
            ▼
2. CONTROLLER VALIDATION
   ┌─────────────────┐
   │ SalesController │
   │ • ModelState    │
   │ • Authorization │
   │ • Input Binding │
   └─────────────────┘
            │
            ▼
3. SERVICE LAYER
   ┌─────────────────┐
   │  SaleService    │
   │ • Business Rules│
   │ • Validation    │
   │ • Protocol Gen  │
   └─────────────────┘
            │
            ▼
4. REPOSITORY LAYER
   ┌─────────────────┐
   │  UnitOfWork     │
   │ • Add Sale      │
   │ • Update Vehicle│
   │ • Save Changes  │
   └─────────────────┘
            │
            ▼
5. DATABASE
   ┌─────────────────┐
   │   SQLite DB     │
   │ • INSERT Sale   │
   │ • UPDATE Vehicle│
   │ • COMMIT Trans  │
   └─────────────────┘
```

## 🔐 Arquitetura de Segurança

```
┌─────────────────────────────────────────────────────────────────┐
│                       SECURITY LAYERS                           │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   FRONTEND      │    │   MIDDLEWARE    │    │    BACKEND      │
│                 │    │                 │    │                 │
│ • HTTPS Only    │───▶│ • Authentication│───▶│ • Authorization │
│ • CSRF Tokens   │    │ • Session Mgmt  │    │ • Role Checks   │
│ • Input Valid   │    │ • Cookies       │    │ • Data Valid    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                 │
                                 ▼
                    ┌─────────────────┐
                    │ IDENTITY STORE  │
                    │                 │
                    │ • Users         │
                    │ • Roles         │
                    │ • Claims        │
                    │ • Passwords     │
                    └─────────────────┘

ROLE HIERARCHY:
Administrador
    │
    ├── Full CRUD Access
    ├── User Management
    └── System Configuration
        │
        ▼
    Gerente
        │
        ├── Sales Management
        ├── Reports Access
        └── Customer Management
            │
            ▼
        Vendedor
            │
            ├── Create Sales Only
            └── View Own Sales
```

## 🚀 Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     PRODUCTION ENVIRONMENT                     │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   LOAD BALANCER │    │   WEB SERVER    │    │    DATABASE     │
│                 │    │                 │    │                 │
│ • SSL Term      │───▶│ • IIS/Kestrel   │───▶│ • SQL Server    │
│ • Health Check  │    │ • ASP.NET Core  │    │ • Backup        │
│ • Rate Limiting │    │ • Static Files  │    │ • Monitoring    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │                       │
                                ▼                       ▼
                    ┌─────────────────┐    ┌─────────────────┐
                    │   CACHE LAYER   │    │   LOGGING       │
                    │                 │    │                 │
                    │ • Redis         │    │ • Structured    │
                    │ • Memory        │    │ • ELK Stack     │
                    │ • Distributed   │    │ • Metrics       │
                    └─────────────────┘    └─────────────────┘

DEPLOYMENT PROCESS:
1. Build & Test Pipeline
2. Docker Container Creation
3. Blue-Green Deployment
4. Health Checks
5. Traffic Switch
6. Monitoring & Rollback
```

## 📊 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                      DATA RELATIONSHIPS                        │
└─────────────────────────────────────────────────────────────────┘

ApplicationUser ──┬──► Sale
                  │
Manufacturer ────┬┴──► Vehicle ──┬──► Sale
                 │               │
                 └──────────────┬┴──► Stock Status
                                │
Customer ──────────────────────┬┴──► Sale History
                               │
Dealership ────────────────────┴───► Sales Performance

┌─────────────────────────────────────────────────────────────────┐
│                      CACHE STRATEGY                            │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   LEVEL 1       │    │    LEVEL 2      │    │    LEVEL 3      │
│   Browser       │    │   Memory        │    │    Redis        │
│                 │    │                 │    │                 │
│ • Static Assets │    │ • Active Data   │    │ • Session       │
│ • API Responses │    │ • User Sessions │    │ • Shared Cache  │
│ • 5min TTL      │    │ • 30min TTL     │    │ • 24h TTL       │
└─────────────────┘    └─────────────────┘    └─────────────────┘

CACHE INVALIDATION:
• Write-through for critical data
• Time-based expiration
• Event-based invalidation
• Manual purge capability
```

## 🔄 API Request Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                    REQUEST LIFECYCLE                           │
└─────────────────────────────────────────────────────────────────┘

HTTP Request
     │
     ▼
┌─────────────────┐
│  MIDDLEWARE     │
│  PIPELINE       │
│                 │
│ 1. HTTPS Redir  │
│ 2. Security     │
│ 3. Auth/Author  │
│ 4. Routing      │
│ 5. CORS         │
└─────────────────┘
     │
     ▼
┌─────────────────┐
│   CONTROLLER    │
│                 │
│ • Model Binding │
│ • Validation    │
│ • Action Filter │
└─────────────────┘
     │
     ▼
┌─────────────────┐
│    SERVICE      │
│                 │
│ • Business      │
│   Logic         │
│ • Validation    │
│ • Caching       │
└─────────────────┘
     │
     ▼
┌─────────────────┐
│  REPOSITORY     │
│                 │
│ • Data Access   │
│ • Query Build   │
│ • Transaction   │
└─────────────────┘
     │
     ▼
┌─────────────────┐
│   DATABASE      │
│                 │
│ • SQL Execution │
│ • Data Return   │
│ • Connection    │
│   Management    │
└─────────────────┘
     │
     ▼
HTTP Response
```

## 🎯 Component Interaction Matrix

```
┌─────────────────────────────────────────────────────────────────┐
│              COMPONENT DEPENDENCIES                            │
└─────────────────────────────────────────────────────────────────┘

              │ Cont │ Serv │ Repo │ Cache│ Auth │ Valid│
──────────────┼──────┼──────┼──────┼──────┼──────┼──────┤
Controllers   │  -   │  ✓   │  ✗   │  ✗   │  ✓   │  ✓   │
Services      │  ✗   │  -   │  ✓   │  ✓   │  ✓   │  ✓   │
Repositories  │  ✗   │  ✗   │  -   │  ✗   │  ✗   │  ✗   │
Cache Service │  ✗   │  ✗   │  ✗   │  -   │  ✗   │  ✗   │
Auth Service  │  ✗   │  ✗   │  ✗   │  ✗   │  -   │  ✗   │
Validators    │  ✗   │  ✗   │  ✗   │  ✗   │  ✗   │  -   │

Legend:
✓ = Direct Dependency
✗ = No Dependency
- = Self

DEPENDENCY RULES:
• Controllers depend on Services only
• Services can use Repositories and Infrastructure
• Repositories are isolated (no business logic)
• Infrastructure services are reusable
• Validators are pure functions
```

---

**Diagramas de Arquitetura v1.0** - Sistema Sunset Cars
**Última atualização**: Novembro 2025
