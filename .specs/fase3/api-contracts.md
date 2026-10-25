# Contratos

## Cadastro

`POST /api/v1/users`

```json
{ "name": "Maria Silva", "cpf": "52998224725", "email": "maria@example.com", "password": "senha-segura" }
```

## Login

`POST /api/v1/auth/login`

```json
{ "email": "maria@example.com", "password": "senha-segura" }
```

Resposta:

```json
{ "accessToken": "ey...", "expiresAt": "2026-10-25T13:00:00Z" }
```
