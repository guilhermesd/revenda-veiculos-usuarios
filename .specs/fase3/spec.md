# Spec — Usuarios e Autenticação

## Casos de uso

- Cadastrar cliente com nome, CPF, e-mail e senha.
- Autenticar cliente por e-mail e senha.
- Emitir JWT com `sub` igual ao identificador do cliente.

## Endpoints

| Método | Rota | Resposta |
|---|---|---|
| POST | `/api/v1/users` | `201` cliente sem senha |
| POST | `/api/v1/auth/login` | `200` access token JWT |
| GET | `/health/live` | `200` |

Senha tem mínimo de 8 caracteres e nunca aparece em resposta, log ou documento em texto puro. Credenciais inválidas retornam `401`; CPF/e-mail duplicados retornam `409`.
