# Implementation Patterns — Express / Node.js

## Middleware Ordering Rules

Express processes middleware in registration order. The correct sequence is critical:

```javascript
const express = require("express");
const helmet = require("helmet");
const cors = require("cors");
const rateLimit = require("express-rate-limit");

const app = express();

// 1. Security headers (first, before any response is sent)
app.use(helmet());

// 2. CORS (must handle OPTIONS preflight before routes)
app.use(cors({ origin: process.env.ALLOWED_ORIGINS?.split(","), credentials: true }));

// 3. Body parsing (before any route that reads req.body)
app.use(express.json({ limit: "10kb" }));
app.use(express.urlencoded({ extended: true }));

// 4. Rate limiting (before routes, after parsing)
app.use("/api/", rateLimit({ windowMs: 15 * 60 * 1000, max: 100 }));

// 5. Application routes
app.use("/api/v1/users", userRouter);
app.use("/api/v1/posts", postRouter);

// 6. 404 handler (after all routes)
app.use((req, res) => {
  res.status(404).json({ error: "Not Found", message: `${req.method} ${req.path} not found` });
});

// 7. Error handler (MUST be last, MUST have 4 args)
app.use((err, req, res, next) => { /* ... */ });
```

## Router Composition

Each resource gets its own file returning a `Router` instance:

```javascript
// routes/users.js
const { Router } = require("express");
const { validateBody } = require("../middleware/validate");
const { createUserSchema } = require("../schemas/user");
const asyncHandler = require("../util/asyncHandler");
const userController = require("../controllers/userController");

const router = Router();

router.get("/", asyncHandler(userController.list));
router.get("/:id", asyncHandler(userController.getById));
router.post("/", validateBody(createUserSchema), asyncHandler(userController.create));
router.put("/:id", validateBody(createUserSchema), asyncHandler(userController.update));
router.delete("/:id", asyncHandler(userController.remove));

module.exports = router;
```

## Error Handler Structure

The centralized error handler normalizes all errors into a consistent shape:

```javascript
// middleware/errorHandler.js
const logger = require("../util/logger");

function errorHandler(err, req, res, _next) {
  // Operational errors (expected, e.g., validation failure)
  if (err.isOperational || err.status) {
    return res.status(err.status || 400).json({
      error: err.name || "Error",
      message: err.message,
      ...(err.details && { details: err.details }),
    });
  }

  // Programming errors (unexpected)
  logger.error("Unhandled error", { err, method: req.method, path: req.path });

  res.status(500).json({
    error: "Internal Server Error",
    message: process.env.NODE_ENV === "production"
      ? "An unexpected error occurred"
      : err.message,
  });
}

module.exports = errorHandler;
```

## Async/Await Error Propagation

Option A — wrapper function (explicit):

```javascript
// util/asyncHandler.js
const asyncHandler = (fn) => (req, res, next) =>
  Promise.resolve(fn(req, res, next)).catch(next);

module.exports = asyncHandler;
```

Option B — `express-async-errors` (implicit, require once at entry):

```javascript
require("express-async-errors"); // patches Express to catch async rejections
```

## TypeScript Request Type Augmentation

Extend the Express `Request` to include custom properties:

```typescript
// types/express.d.ts
import { User } from "../models/User";

declare global {
  namespace Express {
    interface Request {
      user?: User;
      requestId?: string;
    }
  }
}
```

## Validation Middleware Pattern

Using zod for schema validation:

```typescript
// middleware/validate.ts
import { z, ZodSchema } from "zod";
import { Request, Response, NextFunction } from "express";

export function validateBody(schema: ZodSchema) {
  return (req: Request, res: Response, next: NextFunction) => {
    const result = schema.safeParse(req.body);
    if (!result.success) {
      return res.status(400).json({
        error: "Validation Error",
        details: result.error.flatten().fieldErrors,
      });
    }
    req.body = result.data; // replace with parsed/typed data
    next();
  };
}
```

## Dependency Injection Pattern

Pass dependencies through factory functions rather than module-level imports:

```javascript
// controllers/userController.js
function createUserController({ userService, logger }) {
  return {
    async list(req, res) {
      const users = await userService.findAll(req.query);
      res.json({ data: users });
    },
    async create(req, res) {
      const user = await userService.create(req.body);
      logger.info("User created", { userId: user.id });
      res.status(201).json({ data: user });
    },
  };
}
module.exports = createUserController;
```

## Graceful Shutdown

```javascript
// server.js
const app = require("./app");
const logger = require("./util/logger");

const server = app.listen(process.env.PORT || 3000, () => {
  logger.info(`Server listening on port ${process.env.PORT || 3000}`);
});

function shutdown(signal) {
  logger.info(`${signal} received. Shutting down gracefully...`);
  server.close(() => {
    logger.info("HTTP server closed");
    // Close DB connections, flush logs, etc.
    process.exit(0);
  });
  // Force shutdown after 30s
  setTimeout(() => {
    logger.error("Forced shutdown after timeout");
    process.exit(1);
  }, 30000);
}

process.on("SIGTERM", () => shutdown("SIGTERM"));
process.on("SIGINT", () => shutdown("SIGINT"));
```

## Health Check Endpoint

```javascript
router.get("/health", (req, res) => {
  res.json({ status: "ok", uptime: process.uptime(), timestamp: new Date().toISOString() });
});

router.get("/ready", async (req, res) => {
  try {
    await db.raw("SELECT 1"); // check DB connectivity
    res.json({ status: "ready" });
  } catch {
    res.status(503).json({ status: "not ready" });
  }
});
```

Related skills: `api-contracts`, `rate-limits-retries`, `postgresql`, `observability-logging`.
