---
name: flask
description: "Guides Flask application development: application factory creation, Blueprint organization, extension wiring (SQLAlchemy, Migrate, Login, WTF), request/app context handling, error handler registration, configuration management, Jinja2 templating, and WSGI deployment with gunicorn or waitress."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: flask
  maturity: draft
  risk: low
  tags: [flask, python, wsgi, web]
---

# Purpose

Guides the design and implementation of Flask web applications—from project scaffolding with the application factory pattern through Blueprint decomposition, extension initialization, request lifecycle management, and production WSGI deployment. Ensures Flask-specific idioms are applied correctly so that context handling, configuration layering, and error propagation follow established Flask conventions.

# When to use this skill

Use this skill when:

- creating a new Flask application or adding routes/blueprints to an existing one
- wiring Flask extensions (Flask-SQLAlchemy, Flask-Migrate, Flask-Login, Flask-WTF, Flask-CORS)
- working with Flask's request context, application context, or the `g` object
- registering error handlers, before/after request hooks, or teardown callbacks
- configuring Flask for multiple environments (dev, test, staging, production)
- writing tests using `test_client()` or `test_request_context()`
- setting up WSGI deployment with gunicorn, waitress, or similar servers
- rendering templates with Jinja2 or building Flask CLI commands

# Do not use this skill when

- the project uses FastAPI or another ASGI framework—prefer `fastapi` skill
- the task is general Python project structure with no Flask dependency—prefer `python` skill
- the task is purely frontend HTML/CSS/JS with no server logic
- the project is a Django application

# Operating procedure

1. **Identify the app entry point.** Confirm the project uses an application factory (`create_app()`) or determine whether to introduce one. Check for a `wsgi.py` or `app.py` that calls the factory.
2. **Map the Blueprint structure.** Identify existing blueprints in the project. For new features, determine the correct blueprint or whether a new one is needed. Each blueprint should own a URL prefix and group related routes.
3. **Wire extensions correctly.** Extensions must be instantiated at module level (e.g., `db = SQLAlchemy()`) and initialized inside the factory with `ext.init_app(app)`. Never pass `app` to the constructor in a factory-based project.
4. **Handle configuration layering.** Use `app.config.from_object()` for defaults, `app.config.from_envvar()` or `app.config.from_pyfile()` for environment overrides. Verify `SECRET_KEY` is set and not hardcoded in production.
5. **Register error handlers.** Add `@app.errorhandler(400)`, `@app.errorhandler(404)`, `@app.errorhandler(500)` in the factory or a dedicated errors blueprint. Return JSON or HTML depending on the request's `Accept` header.
6. **Set up request hooks.** Use `before_request` for auth checks or request-scoped setup, `after_request` for response headers, `teardown_appcontext` for cleanup (closing DB connections, etc.).
7. **Write tests.** Use `app.test_client()` for integration tests and `app.test_request_context()` for unit tests that need request/app context. Use pytest fixtures to create and configure the test app.
8. **Verify deployment readiness.** Confirm `debug=False`, `SECRET_KEY` is from environment, CSRF protection is active, and the app runs under a WSGI server (not `flask run` in production).

# Decision rules

- Always use the application factory pattern for any project beyond a single-file prototype.
- Initialize extensions with `init_app(app)` inside the factory—never at import time with a concrete app instance.
- Store request-scoped data on `flask.g`, not on module-level variables or the session (unless persistence across requests is needed).
- Use `current_app` to access the app inside blueprints—never import the app instance directly.
- Prefer `abort(status_code)` over manually constructing error responses in route handlers.
- Keep Jinja2 templates in a `templates/` directory co-located with the blueprint or at the app root.
- Use `flask db migrate` / `flask db upgrade` for schema changes—never raw DDL in application code.
- Choose gunicorn (Linux) or waitress (cross-platform) for production; never use the built-in development server.

# Output requirements

1. `App Structure` — factory function, blueprint registration, extension wiring
2. `Route Implementation` — endpoint, methods, validation, response format
3. `Error Handling` — error handler registrations, error response shape
4. `Configuration` — environment-specific config classes, secret management
5. `Verification` — pytest commands, test client assertions, deployment check

# References

Read these only when relevant:

- `references/implementation-patterns.md` — factory, blueprint, extension, and hook patterns
- `references/validation-checklist.md` — pre-deploy and pre-merge verification items
- `references/failure-modes.md` — common Flask runtime errors and their fixes

# Related skills

- `python` — general Python project structure, typing, and tooling
- `fastapi` — alternative Python web framework (ASGI-based)
- `orm-patterns` — SQLAlchemy model design and query patterns
- `postgresql` — database configuration and connection management
- `observability-logging` — structured logging in Flask applications

# Anti-patterns

- **God factory.** Putting all route definitions directly in `create_app()` instead of using blueprints. This makes the factory unreadable and untestable.
- **Import-time app access.** Importing `app` from a module that calls `create_app()` at import time, causing circular imports and context errors.
- **Extension double-init.** Passing `app` to an extension constructor AND calling `init_app(app)`, causing duplicate initialization or configuration conflicts.
- **Bare `except` in routes.** Catching all exceptions in a route handler, swallowing errors that should propagate to Flask's error handling system.
- **Session as cache.** Storing large data structures in `flask.session`, bloating the signed cookie and hitting browser size limits.
- **`g` across requests.** Assuming `flask.g` persists between requests—it is reset on every request.
- **Debug mode in production.** Leaving `FLASK_DEBUG=1` or `debug=True` in production, exposing the Werkzeug debugger and allowing arbitrary code execution.
- **Hardcoded secret key.** Committing `SECRET_KEY = 'dev'` and deploying it, making session cookies forgeable.

# Failure handling

- If an operation fails because code is running outside an application context, wrap it in `with app.app_context():` or ensure the calling code is within a request or CLI command.
- If a circular import occurs, move the `from app import db` into the function body or restructure to use `current_app` and late imports.
- If CSRF validation fails on a form submission, verify Flask-WTF is initialized and `{{ form.hidden_tag() }}` or `{{ csrf_token() }}` is in the template.
- If tests fail with "Working outside of application context," ensure the test fixture creates an app and pushes the context with `app.app_context()`.
- If the development server shows "Address already in use," check for zombie processes on the port or use `flask run --port <alt>`.
- If database migrations fail, check that `Flask-Migrate` is initialized with both `app` and `db`, and that the migrations directory exists (`flask db init`).
