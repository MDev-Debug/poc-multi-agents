# Backend — Scaffold da API + Persistência de Usuário (Sprint 1)

## Contexto
Precisamos de uma API local em .NET 10 com SQL Server para suportar autenticação (cadastro/login).

## Escopo
- Inclui:
  - Criar projeto backend (Web API) e estrutura base.
  - Configurar SQL Server via connection string `DefaultConnection`.
  - Criar modelo de usuário e persistência (EF Core).
- Não inclui:
  - Funcionalidades de dashboard.
  - Recuperação de senha e confirmação de e-mail.

## Dependências
- .NET 10 SDK
- SQL Server local acessível em `localhost,1433`

## Requisitos (funcionais)
- Persistir usuários com pelo menos:
  - `Id` (GUID)
  - `Email` (único)
  - `PasswordHash`
  - `CreatedAt`
- Configurar EF Core para SQL Server e criar migration inicial.

## Requisitos UX/UI
- N/A

## Critérios de aceitação (Given/When/Then)
- Given o backend configurado, When eu rodo `dotnet build`, Then compila sem erros.
- Given SQL Server local disponível, When eu executo migrations/atualização do banco, Then as tabelas são criadas no DB `CHAT`.
- Given um e-mail já cadastrado, When eu tento cadastrar novamente, Then a API deve impedir duplicidade (regra de unicidade por e-mail).

## Definição de pronto (DoD)
- Build ok
- Migração criada e aplicável
- Erros tratados (mensagens e códigos HTTP consistentes)
