# Implementation Patterns — Flask

## Application Factory

Every non-trivial Flask project should use an application factory. This avoids module-level app state and enables testing with different configurations.

```python
# app/__init__.py
from flask import Flask
from app.extensions import db, migrate, login_manager, csrf

def create_app(config_name="default"):
    app = Flask(__name__)
    app.config.from_object(config_by_name[config_name])

    # Initialize extensions
    db.init_app(app)
    migrate.init_app(app, db)
    login_manager.init_app(app)
    csrf.init_app(app)

    # Register blueprints
    from app.auth import auth_bp
    app.register_blueprint(auth_bp, url_prefix="/auth")

    from app.api import api_bp
    app.register_blueprint(api_bp, url_prefix="/api/v1")

    # Register error handlers
    register_error_handlers(app)

    return app
```

## Extension Initialization

Declare extensions at module level without an app instance. Initialize them inside the factory.

```python
# app/extensions.py
from flask_sqlalchemy import SQLAlchemy
from flask_migrate import Migrate
from flask_login import LoginManager
from flask_wtf.csrf import CSRFProtect

db = SQLAlchemy()
migrate = Migrate()
login_manager = LoginManager()
csrf = CSRFProtect()
```

## Blueprint Structure

Organize each blueprint as a package with its own routes, forms, templates, and models.

```
app/
├── __init__.py          # create_app factory
├── extensions.py        # extension instances
├── auth/
│   ├── __init__.py      # auth_bp = Blueprint("auth", __name__)
│   ├── routes.py        # @auth_bp.route("/login")
│   ├── forms.py         # LoginForm(FlaskForm)
│   └── templates/auth/  # login.html, register.html
├── api/
│   ├── __init__.py      # api_bp = Blueprint("api", __name__)
│   ├── routes.py
│   └── schemas.py
└── models/
    ├── __init__.py
    └── user.py
```

## Request Hooks

```python
@app.before_request
def load_current_user():
    g.user = get_user_from_session()

@app.after_request
def add_security_headers(response):
    response.headers["X-Content-Type-Options"] = "nosniff"
    response.headers["X-Frame-Options"] = "DENY"
    return response

@app.teardown_appcontext
def close_db_session(exception=None):
    db_session = g.pop("db_session", None)
    if db_session is not None:
        db_session.close()
```

## Configuration Layering

```python
# config.py
import os

class BaseConfig:
    SECRET_KEY = os.environ.get("SECRET_KEY", "change-me-in-production")
    SQLALCHEMY_TRACK_MODIFICATIONS = False

class DevelopmentConfig(BaseConfig):
    DEBUG = True
    SQLALCHEMY_DATABASE_URI = "sqlite:///dev.db"

class TestingConfig(BaseConfig):
    TESTING = True
    SQLALCHEMY_DATABASE_URI = "sqlite:///:memory:"
    WTF_CSRF_ENABLED = False

class ProductionConfig(BaseConfig):
    SECRET_KEY = os.environ["SECRET_KEY"]  # Fail loudly if missing
    SQLALCHEMY_DATABASE_URI = os.environ["DATABASE_URL"]

config_by_name = {
    "default": DevelopmentConfig,
    "development": DevelopmentConfig,
    "testing": TestingConfig,
    "production": ProductionConfig,
}
```

## Error Handler Registration

```python
def register_error_handlers(app):
    @app.errorhandler(400)
    def bad_request(error):
        if request_wants_json():
            return jsonify(error="Bad Request", message=str(error)), 400
        return render_template("errors/400.html"), 400

    @app.errorhandler(404)
    def not_found(error):
        if request_wants_json():
            return jsonify(error="Not Found"), 404
        return render_template("errors/404.html"), 404

    @app.errorhandler(500)
    def internal_error(error):
        db.session.rollback()
        if request_wants_json():
            return jsonify(error="Internal Server Error"), 500
        return render_template("errors/500.html"), 500

def request_wants_json():
    return request.accept_mimetypes.best_match(
        ["application/json", "text/html"]
    ) == "application/json"
```

## CLI Commands with flask.cli

```python
# app/commands.py
import click
from flask.cli import with_appcontext

@app.cli.command("seed-db")
@with_appcontext
def seed_db():
    """Seed the database with initial data."""
    from app.models.user import User
    admin = User(username="admin", email="admin@example.com")
    db.session.add(admin)
    db.session.commit()
    click.echo("Database seeded.")
```

## Test Fixtures

```python
# tests/conftest.py
import pytest
from app import create_app
from app.extensions import db as _db

@pytest.fixture
def app():
    app = create_app("testing")
    with app.app_context():
        _db.create_all()
        yield app
        _db.drop_all()

@pytest.fixture
def client(app):
    return app.test_client()

@pytest.fixture
def runner(app):
    return app.test_cli_runner()
```

Related skills: `python`, `fastapi`, `orm-patterns`, `postgresql`.
