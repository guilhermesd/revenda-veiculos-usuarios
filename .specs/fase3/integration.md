# Integração

A API de Usuarios não chama a API de Vendas. Ela emite JWT; o consumidor entrega esse token à API de Vendas. O contrato criptográfico compartilhado é `Jwt__Key`, `Jwt__Issuer` e `Jwt__Audience` configurados por ambiente.
# Integração com a API de Vendas

O JWT emitido por Usuários contém `sub`, `email` e `role`. Os papéis aceitos são `ADM` e `COMPRADOR`; Vendas usa o claim `role` para autorizar operações.

O teste integrado está no repositório de Vendas e requer os quatro serviços Docker em execução. Ele cobre cadastro/login de Usuários, emissão e consumo dos tokens por papel, operações de veículos, compra e os webhooks `PAID`/`CANCELED`.

```powershell
docker compose -f compose.yaml -f compose.local-ports.yaml up -d --build
dotnet test Fiap.RevendaVeiculos.slnx --filter "Category=Docker"
```

Se as APIs de Usuários (`8181`) e Vendas (`8180`) não responderem aos health checks, o teste apresenta a instrução para iniciar o Compose.

