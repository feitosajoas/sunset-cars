# 🚗 Sunset Cars - Sistema de Gestão de Concessionárias

Sistema web moderno para gestão completa de concessionárias de veículos, desenvolvido com **ASP.NET Core 9 MVC** e **Entity Framework Core**.

## 🛠️ Tecnologias

| Categoria        | Tecnologia                                          |
| ---------------- | --------------------------------------------------- |
| **Backend**      | ASP.NET Core 9 MVC, Entity Framework Core 9, SQLite |
| **Frontend**     | Bootstrap 5, JavaScript ES6+, FontAwesome           |
| **Autenticação** | ASP.NET Core Identity                               |
| **APIs**         | Swagger/OpenAPI, REST APIs                          |
| **Integrações**  | ViaCEP API                                          |
| **Cache**        | Memory Cache / Redis                                |

## ⚡ Início Rápido

### Pré-requisitos

- **.NET 9 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **Visual Studio Code** ou **Visual Studio 2022**

### Execução Local

```bash
# 1. Clone o repositório
git clone https://github.com/feitosajoas/sunset-cars.git
cd sunset-cars

# 2. Restaure as dependências
dotnet restore

# 3. Execute a aplicação
dotnet run

# 4. Acesse no navegador
# http://localhost:5285
```

### 👤 Acesso Inicial

- **Email:** `admin@sunsetcars.com`
- **Senha:** `Admin123!`
- **Perfil:** Administrador

## 🎯 Funcionalidades Principais

### 🔐 Sistema de Autenticação

- **3 Perfis de Usuário:** Administrador, Gerente, Vendedor
- **Controle de Acesso:** Baseado em roles
- **Interface de Login:** Moderna e responsiva

### 📊 Gestão Completa

- **Fabricantes:** CRUD com validações personalizadas
- **Veículos:** Gestão com relacionamentos e status
- **Concessionárias:** Integração automática com ViaCEP
- **Clientes:** Validação de CPF e dados pessoais
- **Vendas:** Sistema completo com protocolo único

### 🏠 Dashboard Moderno

- **Página inicial** profissional com hero section
- **Estatísticas** em tempo real
- **Navegação** intuitiva e responsiva

### � APIs REST Seguras

- **Endpoints protegidos** com autorização
- **Documentação Swagger** automática
- **Validações robustas** de entrada

## 🏗️ Arquitetura

```
SunsetCars/
├── Controllers/         # MVC Controllers + API Controllers
├── Models/             # Entidades de domínio
├── Views/              # Razor Views responsivas
├── Services/
│   ├── Domain/         # Serviços de negócio
│   └── Infrastructure/ # Cache, Integrações
├── Data/
│   ├── Repositories/   # Padrão Repository + Unit of Work
│   └── Migrations/     # Entity Framework Migrations
├── DTOs/               # Data Transfer Objects
└── Extensions/         # Extensões e configurações
```

## 🗃️ Modelo de Dados

### Entidades Principais

- **ApplicationUser** - Usuário com perfis
- **Manufacturer** - Fabricantes de veículos
- **Vehicle** - Veículos com status ativo/inativo
- **Dealership** - Concessionárias com endereço
- **Customer** - Clientes com validação CPF
- **Sale** - Vendas com protocolo único

### Relacionamentos

- Manufacturer **1:N** Vehicle
- Dealership **1:N** Sale
- Vehicle **1:N** Sale
- Customer **1:N** Sale
- ApplicationUser **1:N** Sale

## 🎨 Interface e UX

### Design Moderno

- **Bootstrap 5** com componentes personalizados
- **Design responsivo** para todos os dispositivos
- **Animações suaves** e feedback visual
- **Navegação intuitiva** com breadcrumbs

### Funcionalidades UX

- **Dropdown dependentes** (Fabricante → Veículos)
- **Preenchimento automático** de endereço via CEP
- **Validação em tempo real** nos formulários
- **Loading states** para operações assíncronas

## � Segurança Implementada

### Autenticação e Autorização

- **ASP.NET Core Identity** para gestão de usuários
- **Role-based authorization** em controllers e APIs
- **Senhas hasheadas** com salt

### Validações

- **Server-side validation** rigorosa
- **Client-side validation** para UX
- **Proteção CSRF** em formulários
- **Sanitização** de entrada de dados

### Controle de Acesso

| Perfil            | Permissões                         |
| ----------------- | ---------------------------------- |
| **Administrador** | Acesso total, incluindo exclusões  |
| **Gerente**       | CRUD completo, vendas e relatórios |
| **Vendedor**      | Apenas vendas próprias             |

## � APIs REST

### Endpoints Principais

```
GET    /api/manufacturers     # Listar fabricantes
GET    /api/vehicles         # Listar veículos
GET    /api/customers        # Listar clientes
GET    /api/sales            # Listar vendas
POST   /api/sales            # Criar venda
```

### Documentação

- **Swagger UI:** `/swagger`
- **Autenticação:** Required em todos os endpoints
- **Autorização:** Role-based por endpoint

## 🚀 Performance e Otimização

### Cache Strategy

- **Memory Cache** para dados frequentes
- **Redis** configurado (opcional)
- **Cache keys** centralizados

### Database

- **SQLite** para desenvolvimento
- **Migrations** automáticas
- **Indexes** otimizados
- **Lazy loading** configurado

## 🔧 Configuração

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=sunsetcars.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Variáveis de Ambiente

- `ASPNETCORE_ENVIRONMENT` - Development/Production
- `ConnectionStrings__DefaultConnection` - String de conexão

---

🎯 **Sistema completo e funcional com arquitetura moderna e práticas de segurança implementadas.**
