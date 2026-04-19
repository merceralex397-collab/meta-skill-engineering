# Failure Modes — Flask

Common Flask runtime errors, their root causes, and fixes.

## Working Outside Application Context

**Symptom:** `RuntimeError: Working outside of application context.`

**Cause:** Code tries to access `current_app`, `g`, or an extension that depends on the app context, but no app context is active. Common in CLI scripts, background tasks, or tests.

**Fix:** Wrap the code in an explicit context:
```python
with app.app_context():
    db.create_all()
```
In tests, use a fixture that pushes the app context. In Celery tasks, use `app.app_context()` as a context manager.

## Working Outside Request Context

**Symptom:** `RuntimeError: Working outside of request context.`

**Cause:** Code accesses `request`, `session`, or `g` (request-scoped attributes) outside a request. Happens when calling route logic from a CLI command or background job.

**Fix:** For testing, use `app.test_request_context()`. For background jobs, pass the needed data as arguments rather than relying on `request`.
```python
with app.test_request_context("/path", method="POST"):
    result = my_route_helper()
```

## Circular Imports

**Symptom:** `ImportError` or `AttributeError` when importing models or routes.

**Cause:** `app/__init__.py` imports from `models.py`, which imports `db` from `extensions.py`, which is fine—but if `models.py` also imports `app` from `__init__.py`, a circular dependency forms.

**Fix:**
- Extensions go in `extensions.py`, never in `__init__.py`.
- Models import from `extensions.py`, never from `__init__.py`.
- Blueprint imports happen inside `create_app()`, not at the top of `__init__.py`.
- Use `current_app` inside blueprints instead of importing the app.

## Missing CSRF Token

**Symptom:** `400 Bad Request` or `The CSRF token is missing` on form POST.

**Cause:** The HTML form does not include the CSRF hidden field, or the AJAX request does not include the `X-CSRFToken` header.

**Fix:** In templates: `{{ form.hidden_tag() }}` or `<input type="hidden" name="csrf_token" value="{{ csrf_token() }}">`. For AJAX:
```javascript
fetch(url, {
    method: "POST",
    headers: { "X-CSRFToken": getCookie("csrf_token") },
    body: data,
});
```

## Session Cookie Misconfiguration

**Symptom:** Sessions reset on every request, or cookies are rejected by the browser.

**Cause:** `SECRET_KEY` changes between restarts (e.g., randomly generated at startup), `SESSION_COOKIE_SECURE=True` but running over HTTP, or `SESSION_COOKIE_DOMAIN` is wrong.

**Fix:**
- Set `SECRET_KEY` from a stable environment variable.
- Only enable `SESSION_COOKIE_SECURE` when serving over HTTPS.
- Leave `SESSION_COOKIE_DOMAIN` unset unless you need cross-subdomain cookies.

## Thread-Safety Issues with Global State

**Symptom:** Intermittent wrong data, data leaking between users, or segfaults under gunicorn.

**Cause:** Storing request-scoped data in module-level variables instead of `flask.g`. With multiple threads or workers, the global is shared.

**Fix:** Use `flask.g` for request-scoped data. Use `flask.session` for user-scoped data. Never store request data on module globals.

## SQLAlchemy Session Not Cleaned Up

**Symptom:** `DetachedInstanceError`, stale data, or connection pool exhaustion.

**Cause:** SQLAlchemy session is not properly scoped or cleaned up after requests. Flask-SQLAlchemy handles this automatically, but raw SQLAlchemy usage requires manual teardown.

**Fix:** If using Flask-SQLAlchemy, ensure `db.init_app(app)` is called. If using raw SQLAlchemy, register a teardown:
```python
@app.teardown_appcontext
def shutdown_session(exception=None):
    db_session.remove()
```

## Blueprint Not Registered

**Symptom:** Routes return 404 even though the view function exists.

**Cause:** The blueprint was defined but never registered with `app.register_blueprint()` in the factory.

**Fix:** Check `create_app()` for the registration call. Run `flask routes` to list all registered endpoints and verify the blueprint's routes appear.

## Jinja2 Template Not Found

**Symptom:** `jinja2.exceptions.TemplateNotFound: mytemplate.html`

**Cause:** Template file is not in the expected `templates/` directory, or the blueprint's `template_folder` is misconfigured.

**Fix:** Verify the template path. For blueprints with custom template folders:
```python
bp = Blueprint("auth", __name__, template_folder="templates")
```
Templates are resolved in order: app `templates/` → blueprint `template_folder/`.

## Debug Mode in Production

**Symptom:** Werkzeug debugger is accessible publicly, allowing arbitrary code execution.

**Cause:** `FLASK_DEBUG=1` or `app.run(debug=True)` left on in production.

**Fix:** Never use `app.run()` in production. Use a WSGI server. Set `FLASK_DEBUG=0` and `DEBUG=False` in production config. Audit with: `grep -r "debug=True\|FLASK_DEBUG=1" .`

Related skills: `python`, `fastapi`, `orm-patterns`.
