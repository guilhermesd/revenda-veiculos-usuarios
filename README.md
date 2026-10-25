# API de Usuarios e Autenticação — Fase 3

API .NET 10 independente para cadastro de compradores e emissão de JWT. Ela usa MongoDB próprio e nunca compartilha collections com a API de Vendas.

## Endpoints

- `POST /api/v1/users`: cadastra nome, CPF, e-mail e senha.
- `POST /api/v1/auth/login`: retorna JWT para consumo da API de Vendas.
- `GET /health/live` e `/swagger`.

As senhas usam BCrypt; CPF e e-mail possuem índices únicos no MongoDB.

## Testes

```powershell
dotnet test Fiap.RevendaVeiculos.Usuarios.slnx
```

## Docker

Este repositório é dono de seu `compose.yaml`, que declara `api-usuarios` e `mongo-usuarios`, incluindo o volume do banco. Para executar os quatro serviços juntos, o `compose.yaml` do repositório `revenda-veiculos-fase3` inclui este arquivo.

## Documentação de API e banco

- [Design system da API de Usuarios](docs/api-design-system.md): convenções, contratos de cadastro/login e emissão de JWT.
- [Modelo de dados MongoDB de Usuarios](docs/database-model.md): coleção, campos, índices exclusivos e proteção de senha.

## Etapas do CI/CD integrado

O workflow central está no repositório de Vendas, em `.github/workflows/deploy.yml`, porque é ele que orquestra os quatro serviços. Ele é ativado por `push` na branch `main` do repositório de Vendas ou manualmente em **Actions > ci-cd-simulado > Run workflow**.

1. Faz checkout deste repositório junto ao de Vendas.
2. Executa `dotnet test` para as duas soluções.
3. Valida o Compose, constrói as duas imagens de API e obtém as imagens MongoDB.
4. Cria os quatro containers com `docker compose up --no-start`.
5. Valida que os quatro permanecem no estado `created` e remove os recursos ao final.

Para este repositório privado, o repositório de Vendas deve ter o secret `CLIENTES_REPOSITORY_TOKEN`, usado no checkout do GitHub Actions.
