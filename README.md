# API de Usuarios e Autenticação — Fase 3

API .NET 10 independente para cadastro de compradores e emissão de JWT. Ela usa MongoDB próprio e nunca compartilha collections com a API de Vendas.

## Papéis

`ADM` cadastra e edita veículos na API de Vendas. `COMPRADOR` lista veículos autenticado e inicia compras. Em `POST /api/v1/users`, `role` é opcional e assume `COMPRADOR`; os únicos valores aceitos são `ADM` e `COMPRADOR`. O JWT inclui o claim `role` usado pela API de Vendas.

## Endpoints

- `POST /api/v1/users`: cadastra nome, CPF, e-mail e senha.
- `POST /api/v1/auth/login`: retorna JWT para consumo da API de Vendas.
- `GET /health/live` e `/swagger`.

As senhas usam BCrypt; CPF e e-mail possuem índices únicos no MongoDB.

## Testes

```powershell
dotnet test Fiap.RevendaVeiculos.Usuarios.slnx
```

## Teste integrado com Vendas

O teste de integração de papéis fica no repositório de Vendas, pois valida a comunicação entre as duas APIs. Antes de executá-lo, os quatro serviços devem estar em execução a partir da raiz do repositório de Vendas:

```powershell
docker compose -f compose.yaml -f compose.local-ports.yaml up -d --build
dotnet test Fiap.RevendaVeiculos.slnx --filter "Category=Docker"
```

Nesse modo, a API de Usuários responde em `http://localhost:8181` e a API de Vendas em `http://localhost:8180`. O teste valida que `ADM` pode cadastrar veículos e que `COMPRADOR` não pode cadastrar, mas pode iniciar compras. Se o Compose não estiver ativo, o teste informa o comando necessário para iniciá-lo.

O ciclo também valida cadastro/login dos dois papéis, JWT, consulta/edição/listagens de veículos e os webhooks `PAID` e `CANCELED` na API de Vendas.

## Docker

Este repositório é dono de seu `compose.yaml`, que declara `api-usuarios` e `mongo-usuarios`, incluindo o volume do banco. Para executar os quatro serviços juntos, o `compose.yaml` do repositório `revenda-veiculos-fase3` inclui este arquivo.

## Documentação de API e banco

- [Design system da API de Usuarios](docs/api-design-system.md): convenções, contratos de cadastro/login e emissão de JWT.
- [Modelo de dados MongoDB de Usuarios](docs/database-model.md): coleção, campos, índices exclusivos e proteção de senha.

## CI próprio e integração

O workflow próprio [ci.yml](.github/workflows/ci.yml) é acionado em cada `push` ou Pull Request para `main`, além de poder ser executado manualmente. Ele restaura dependências, compila em Release, executa os testes desta solução e constrói a imagem Docker de Usuários.

O workflow central permanece no repositório de Vendas, em `.github/workflows/deploy.yml`, porque é ele que orquestra os quatro serviços. Ele é ativado por `push` na branch `main` do repositório de Vendas ou manualmente em **Actions > ci-cd-simulado > Run workflow**.

1. Faz checkout deste repositório junto ao de Vendas.
2. Executa `dotnet test` para as duas soluções.
3. Valida o Compose, constrói as duas imagens de API e obtém as imagens MongoDB.
4. Cria os quatro containers com `docker compose up --no-start`.
5. Valida que os quatro permanecem no estado `created` e remove os recursos ao final.

Para este repositório privado, o repositório de Vendas deve ter o secret `CLIENTES_REPOSITORY_TOKEN`, usado no checkout do GitHub Actions.
