# task-backend-1 — Clean Architecture: reestruturar projeto em camadas

## Contexto
O backend atual tem tudo em um único projeto `Chat.Api`, misturando entities, services, repositórios e controllers. Refatorar para Clean Architecture com 4 projetos separados.

## Escopo
- Inclui:
  - Criar projetos: Chat.Domain, Chat.Application, Chat.Infrastructure
  - Mover código existente para as camadas corretas
  - Atualizar Chat.Api para depender das camadas
  - Atualizar Chat.slnx com todos os projetos
  - Garantir build e migrations funcionando
- Não inclui:
  - Novas features ou endpoints
  - Testes automatizados

## Estrutura alvo

```
backend/
  Chat.Domain/
    Entities/
      AppUser.cs
      RefreshToken.cs
    Exceptions/         (domínio puro, sem dependências externas)
  Chat.Application/
    Interfaces/
      IPresenceService.cs
      IJwtTokenService.cs
      IRefreshTokenService.cs
    DTOs/
      Auth/
        RegisterRequest.cs
        LoginRequest.cs
        RefreshRequest.cs
        AuthResponse.cs
      Presence/
        OnlineUserDto.cs
    Services/
      JwtTokenService.cs
      RefreshTokenService.cs
      PresenceService.cs
      PresenceEntry.cs
    Options/
      JwtOptions.cs
  Chat.Infrastructure/
    Data/
      ChatDbContext.cs
      Migrations/ (mover existentes)
  Chat.Api/
    Controllers/
      AuthController.cs
    Hubs/
      PresenceHub.cs
    Program.cs
    appsettings.json
    appsettings.Development.json
```

## Dependências entre projetos
- Chat.Domain: nenhuma dependência interna
- Chat.Application → Chat.Domain
- Chat.Infrastructure → Chat.Domain, Chat.Application (apenas interfaces para DI)
- Chat.Api → Chat.Application, Chat.Infrastructure

## Requisitos funcionais
1. Criar Chat.Domain.csproj: apenas classes de domínio (AppUser, RefreshToken), sem pacotes NuGet exceto System.*
2. Criar Chat.Application.csproj: interfaces + DTOs + services (JwtTokenService, RefreshTokenService, PresenceService, PresenceEntry, JwtOptions)
   - Referência: Chat.Domain
   - NuGet: Microsoft.AspNetCore.Authentication.JwtBearer (para JwtTokenService), BCrypt.Net-Next ou remover dependência de PasswordHasher para a camada Domain via interface
   - Atenção: PasswordHasher<AppUser> pode ficar em Infrastructure (implementação de interface IPasswordHasher)
3. Criar Chat.Infrastructure.csproj: DbContext + migrations
   - Referência: Chat.Domain, Chat.Application
   - NuGet: Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Design
4. Atualizar Chat.Api.csproj: remover packages que foram para outras camadas, adicionar referências aos projetos
5. Atualizar Chat.slnx para incluir todos os 4 projetos
6. Ajustar namespaces de todos os arquivos
7. Mover migrations existentes para Chat.Infrastructure/Data/Migrations/
8. Configurar `ef dbcontext` assembly em appsettings ou csproj para apontar para Chat.Infrastructure
9. Garantir `dotnet build` passa em todos os projetos

## Critérios de aceitação
- Given: solução com 4 projetos no Chat.slnx
- When: `dotnet build` executado na pasta backend/
- Then: todos os projetos compilam sem erros

- Given: API iniciada
- When: POST /api/auth/login com credenciais válidas
- Then: retorna 200 com token e refreshToken

- Given: API iniciada
- When: WebSocket connect em /hubs/presence com token válido
- Then: conexão aceita e usuário aparece como online

## Definição de pronto (DoD)
- [ ] 4 projetos criados com namespaces corretos
- [ ] Chat.slnx atualizado
- [ ] `dotnet build` passa
- [ ] API inicia sem erros
- [ ] Login e refresh token funcionando
- [ ] PresenceHub aceitando conexões
