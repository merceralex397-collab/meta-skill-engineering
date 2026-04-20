# Validation Checklist — Flask

A Flask change is not ready for merge until every applicable item is verified.

## Application Structure

- [ ] `create_app()` factory exists and returns the `app` object
- [ ] All blueprints are registered inside the factory with explicit `url_prefix`
- [ ] Extensions are instantiated at module level and initialized with `init_app(app)` inside the factory
- [ ] No circular imports between `app/__init__.py`, models, and routes
- [ ] `current_app` is used inside blueprints—never a direct import of the app instance

## Configuration

- [ ] `SECRET_KEY` is loaded from environment variable in production config
- [ ] `SECRET_KEY` is NOT a hardcoded string like `"dev"` or `"change-me"` in production
- [ ] `DEBUG = False` in production configuration
- [ ] `TESTING = True` only in test configuration
- [ ] `SQLALCHEMY_DATABASE_URI` is set per environment (not shared between dev/prod)
- [ ] `SQLALCHEMY_TRACK_MODIFICATIONS = False` (suppresses deprecation warning)
- [ ] Sensitive config values are not committed to version control

## Security

- [ ] CSRF protection is enabled (Flask-WTF `CSRFProtect` initialized)
- [ ] Forms include `{{ form.hidden_tag() }}` or explicit CSRF token
- [ ] API endpoints that bypass CSRF use `@csrf.exempt` deliberately (not globally disabled)
- [ ] `SESSION_COOKIE_SECURE = True` in production (HTTPS only)
- [ ] `SESSION_COOKIE_HTTPONLY = True` (default, verify not overridden)
- [ ] `SESSION_COOKIE_SAMESITE = "Lax"` or `"Strict"` in production

## Error Handling

- [ ] Error handlers registered for at least 400, 404, and 500
- [ ] 500 error handler calls `db.session.rollback()` if using SQLAlchemy
- [ ] Error responses match the expected format (JSON for API, HTML for web)
- [ ] No bare `except:` blocks that swallow exceptions silently
- [ ] `abort()` is used instead of manually constructing error responses in routes

## Database and Migrations

- [ ] Flask-Migrate is initialized: `migrate.init_app(app, db)`
- [ ] Migration directory exists (`flask db init` has been run)
- [ ] New model changes have a corresponding migration (`flask db migrate`)
- [ ] Migrations apply cleanly on a fresh database (`flask db upgrade`)
- [ ] Downgrade path works (`flask db downgrade` tested)

## Testing

- [ ] Test fixtures create app with test configuration
- [ ] Test database is isolated (`:memory:` SQLite or separate test DB)
- [ ] `app.test_client()` is used for route integration tests
- [ ] Tests run with `pytest` and pass: `pytest tests/ -v`
- [ ] No tests depend on server state from previous tests (each test is isolated)

## Deployment

- [ ] App runs under WSGI server (gunicorn/waitress), not `flask run`
- [ ] Gunicorn worker count is configured: `gunicorn -w 4 -b 0.0.0.0:8000 "app:create_app()"`
- [ ] Static files are served by a reverse proxy (nginx), not Flask in production
- [ ] Logging is configured (not relying on Flask's default `print` to stderr)
- [ ] Health check endpoint exists and returns 200

## Pre-merge Smoke Test

```bash
# Run the full check sequence
export FLASK_APP=app
export FLASK_ENV=testing
pytest tests/ -v --tb=short
flask db upgrade  # verify migrations apply
flask routes      # verify all routes registered correctly
```
