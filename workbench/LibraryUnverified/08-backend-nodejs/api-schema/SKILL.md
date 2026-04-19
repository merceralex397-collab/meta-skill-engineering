---
name: api-schema
description: Defines request/response shapes, versioning, validation, and compatibility rules for API-first work. Trigger on 'design API', 'OpenAPI spec', 'REST schema', 'API versioning', 'generate client SDK'. DO NOT USE for GraphQL schemas, gRPC/protobuf definitions (use stack-standards), auth endpoint logic (use auth-patterns), or external API client wrappers (use external-api-client).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: api-schema
  maturity: draft
  risk: low
  tags: [api, schema]
---

# Purpose
Design API schemas using OpenAPI 3.1 and JSON Schema for type-safe contracts between services. Define versioning strategy, distinguish breaking from non-breaking changes, and enable code generation from specs. The schema IS the contract—disagreement about the schema means disagreement about the API.

# When to use this skill
Use when:
- Designing new REST/HTTP API
- Adding endpoints to existing API
- Generating client SDKs or server stubs
- Documenting API for external consumers

Do NOT use when:
- GraphQL APIs (different schema language)
- Internal RPC/gRPC (use protobuf)
- Simple scripts with no API consumers

# Operating procedure
1. **Create OpenAPI 3.1 specification**:
   ```yaml
   openapi: 3.1.0
   info:
     title: User Service API
     version: 1.0.0
     description: API for user management
   
   servers:
     - url: https://api.example.com/v1
       description: Production
     - url: https://staging-api.example.com/v1
       description: Staging
   
   paths:
     /users/{userId}:
       get:
         operationId: getUser
         summary: Get user by ID
         parameters:
           - name: userId
             in: path
             required: true
             schema:
               type: string
               format: uuid
         responses:
           '200':
             description: User found
             content:
               application/json:
                 schema:
                   $ref: '#/components/schemas/User'
           '404':
             description: User not found
   ```

2. **Define reusable schemas in components**:
   ```yaml
   components:
     schemas:
       User:
         type: object
         required:
           - id
           - email
           - createdAt
         properties:
           id:
             type: string
             format: uuid
             description: Unique user identifier
           email:
             type: string
             format: email
           displayName:
             type: string
             maxLength: 100
           createdAt:
             type: string
             format: date-time
         additionalProperties: false  # Strict schema
       
       Error:
         type: object
         required:
           - code
           - message
         properties:
           code:
             type: string
             enum: [NOT_FOUND, VALIDATION_ERROR, INTERNAL_ERROR]
           message:
             type: string
           details:
             type: object
   ```

3. **Classify changes as breaking or non-breaking**:
   ```
   BREAKING (requires major version bump):
   ─────────────────────────────────────
   - Removing endpoint or method
   - Removing required request field
   - Adding required request field without default
   - Changing field type
   - Removing response field clients depend on
   - Changing error response structure
   
   NON-BREAKING (minor or patch version):
   ─────────────────────────────────────
   - Adding new endpoint
   - Adding optional request field
   - Adding response field
   - Adding new enum value (if clients ignore unknown)
   - Relaxing validation (wider accepted range)
   ```

4. **Choose versioning strategy**:
   ```yaml
   # URL path versioning (most common, clearest)
   servers:
     - url: https://api.example.com/v1
     - url: https://api.example.com/v2
   
   # Header versioning (cleaner URLs)
   # Client sends: Accept: application/vnd.api+json;version=1
   
   # Query parameter (easy for debugging)
   # GET /users?api-version=2024-01-15
   ```

5. **Generate code from spec**:
   ```bash
   # Generate TypeScript client
   npx openapi-typescript openapi.yaml -o types.ts
   
   # Generate Python FastAPI server stub
   pip install fastapi-code-generator
   fastapi-codegen --input openapi.yaml --output app/
   
   # Validate spec
   npx @redocly/cli lint openapi.yaml
   ```

6. **Add request validation**:
   ```python
   # FastAPI automatically validates against OpenAPI schema
   from pydantic import BaseModel, EmailStr
   from uuid import UUID
   
   class CreateUserRequest(BaseModel):
       email: EmailStr
       display_name: str | None = None
   
       class Config:
           extra = 'forbid'  # Reject unknown fields
   
   @app.post("/users")
   async def create_user(request: CreateUserRequest) -> User:
       # Request already validated against schema
       ...
   ```

# Output defaults
```yaml
# openapi.yaml
openapi: 3.1.0
info:
  title: [Service Name] API
  version: 1.0.0
  description: |
    [Description]
    
    ## Versioning
    URL path versioning: /v1/, /v2/
    
    ## Authentication
    Bearer token in Authorization header

servers:
  - url: https://api.example.com/v1

paths:
  /[resource]:
    get:
      # ...
    post:
      # ...

components:
  schemas:
    # Reusable types
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
```

# References
- OpenAPI 3.1 Specification: https://swagger.io/specification/
- https://spec.openapis.org/oas/v3.1.0
- https://json-schema.org/learn/getting-started-step-by-step

# Failure handling
- **Schema and implementation drift**: Generate code from schema or validate implementation against schema in CI
- **Breaking change deployed accidentally**: Add breaking-change detection to CI; tools like `oasdiff` can compare specs
- **Clients fail on new fields**: Design clients to ignore unknown fields; use `additionalProperties: true` in response schemas
- **Validation too strict**: Start permissive, tighten later; it's easier to reject more than to accept more
- **Large spec becomes unmaintainable**: Split into multiple files using `$ref` to external files
