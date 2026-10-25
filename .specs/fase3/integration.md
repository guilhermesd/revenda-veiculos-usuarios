# Integração

A API de Usuarios não chama a API de Vendas. Ela emite JWT; o consumidor entrega esse token à API de Vendas. O contrato criptográfico compartilhado é `Jwt__Key`, `Jwt__Issuer` e `Jwt__Audience` configurados por ambiente.
