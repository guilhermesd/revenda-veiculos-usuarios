# Discovery — API de Usuarios e Autenticação

Esta API é independente da API de Vendas. Ela administra dados de compradores, credenciais e emissão de JWT. Seu MongoDB é exclusivo e nunca é acessado pela API de Vendas.

Este repositório também é dono do `compose.yaml` que declara `api-usuarios`, `mongo-usuarios` e o volume de dados correspondente. A composição integrada é feita pelo repositório de Vendas por `include`.

Requisitos: cadastro de cliente, login por credenciais, senha com hash seguro, JWT, validação de entrada, tratamento de credenciais inválidas, configuração por ambiente e documentação Swagger.

Decisões: ASP.NET Core .NET 10, MongoDB, Clean Architecture, BCrypt para hash, JWT Bearer com chave, emissor e audiência em variáveis de ambiente. CPF é normalizado sem máscara e tem índice único; e-mail é normalizado e tem índice único.
