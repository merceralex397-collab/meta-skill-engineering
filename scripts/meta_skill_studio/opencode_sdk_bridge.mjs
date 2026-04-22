import {spawn, spawnSync} from "node:child_process";
import fs from "node:fs";
import net from "node:net";
import path from "node:path";
import process from "node:process";
import {fileURLToPath} from "node:url";
import {createOpencodeClient} from "../../.opencode/node_modules/@opencode-ai/sdk/dist/client.js";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

function parseArgs(argv) {
  const args = {action: "assistant", prompt: "", promptFile: "", model: ""};
  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === "assistant") {
      args.action = "assistant";
      continue;
    }
    if (arg === "--prompt") {
      args.prompt = argv[i + 1] ?? "";
      i += 1;
      continue;
    }
    if (arg === "--prompt-file") {
      args.promptFile = argv[i + 1] ?? "";
      i += 1;
      continue;
    }
    if (arg === "--model") {
      args.model = argv[i + 1] ?? "";
      i += 1;
    }
  }
  return args;
}

function resolvePrompt(args) {
  if (args.promptFile) {
    return fs.readFileSync(args.promptFile, "utf8");
  }

  return args.prompt;
}

async function getFreePort() {
  return new Promise((resolve, reject) => {
    const server = net.createServer();
    server.listen(0, "127.0.0.1", () => {
      const address = server.address();
      server.close(() => resolve(address.port));
    });
    server.on("error", reject);
  });
}

function readRepoModel() {
  const configPath = path.join(repoRoot, ".opencode", "opencode.json");
  if (!fs.existsSync(configPath)) {
    return null;
  }

  try {
    const parsed = JSON.parse(fs.readFileSync(configPath, "utf8"));
    return typeof parsed.model === "string" ? parsed.model.trim() : null;
  } catch {
    return null;
  }
}

function getServerCommand() {
  const candidates = [
    path.join(repoRoot, ".opencode", "node_modules", "opencode-windows-x64", "bin", "opencode.exe"),
    path.join(repoRoot, ".opencode", "node_modules", "opencode-windows-x64-baseline", "bin", "opencode.exe"),
    path.join(repoRoot, ".opencode", "node_modules", "opencode-linux-x64", "bin", "opencode"),
    path.join(repoRoot, ".opencode", "node_modules", "opencode-linux-arm64", "bin", "opencode"),
    path.join(repoRoot, ".opencode", "node_modules", ".bin", "opencode.cmd"),
    path.join(repoRoot, ".opencode", "node_modules", ".bin", "opencode"),
  ];

  return candidates.find((candidate) => fs.existsSync(candidate)) ?? "opencode";
}

async function startServer() {
  const port = await getFreePort();
  const command = getServerCommand();
  const logs = [];
  const child =
    process.platform === "win32" && command.endsWith(".cmd")
      ? spawn(`"${command}" serve --hostname=127.0.0.1 --port=${port}`, {
          cwd: repoRoot,
          shell: true,
          windowsHide: true,
          stdio: ["ignore", "pipe", "pipe"],
        })
      : spawn(command, ["serve", "--hostname=127.0.0.1", `--port=${port}`], {
          cwd: repoRoot,
          windowsHide: true,
          stdio: ["ignore", "pipe", "pipe"],
        });

  child.stdout.on("data", (data) => logs.push(String(data)));
  child.stderr.on("data", (data) => logs.push(String(data)));

  const baseUrl = `http://127.0.0.1:${port}`;
  for (let i = 0; i < 40; i += 1) {
    try {
      const response = await fetch(`${baseUrl}/global/health`);
      if (response.ok) {
        return {baseUrl, child, port};
      }
    } catch {
      // Wait until the server is ready.
    }
    await sleep(500);
  }

  child.kill();
  throw new Error(`OpenCode server failed to start. ${logs.join("").trim()}`.trim());
}

async function stopServer(server) {
  const {child, port} = server;
  if (child.exitCode !== null || child.killed) {
    await stopServerProcessesByPort(port);
    return;
  }

  child.kill();
  child.stdout?.destroy();
  child.stderr?.destroy();
  await Promise.race([
    new Promise((resolve) => child.once("exit", resolve)),
    sleep(3000),
  ]);
  await stopServerProcessesByPort(port);
}

async function stopServerProcessesByPort(port) {
  if (process.platform !== "win32") {
    return;
  }

  const stopScript = `
    $targets = Get-CimInstance Win32_Process |
      Where-Object {
        $_.Name -in @('node.exe', 'opencode.exe') -and
        $_.CommandLine -like '*--port=${port}*'
      } |
      Select-Object -ExpandProperty ProcessId
    if ($targets) {
      Stop-Process -Id $targets -Force
    }
  `;

  spawnSync("powershell.exe", ["-NoProfile", "-Command", stopScript], {
    windowsHide: true,
    stdio: "ignore",
  });
}

function normalizeModelSelection(repoModel, providersPayload) {
  const providerMap = new Map(
    providersPayload.providers.map((provider) => [provider.id, provider])
  );

  const resolvePair = (providerID, modelID) => {
    if (!providerID || !modelID) {
      return null;
    }

    const provider = providerMap.get(providerID);
    if (!provider?.models) {
      return null;
    }

    if (provider.models[modelID]) {
      return {providerID, modelID};
    }

    const match = Object.values(provider.models).find(
      (model) => model.id.toLowerCase() === modelID.toLowerCase()
    );
    return match ? {providerID, modelID: match.id} : null;
  };

  if (repoModel?.includes("/")) {
    const [providerID, modelID] = repoModel.split("/", 2);
    const explicit = resolvePair(providerID, modelID);
    if (explicit) {
      return explicit;
    }
  }

  if (repoModel) {
    for (const provider of providersPayload.providers) {
      const pair = resolvePair(provider.id, repoModel);
      if (pair) {
        return pair;
      }
    }
  }

  const preferredProviders = [
    "minimax-coding-plan",
    "moonshotai",
    "kimi",
    "bigpickle",
    "big-pickle",
    "openrouter",
    "fireworks-ai",
    "llama",
    "nvidia",
    "opencode",
    "LocalModel",
  ];

  for (const providerID of preferredProviders) {
    const defaultModel = providersPayload.default?.[providerID];
    const pair = resolvePair(providerID, defaultModel);
    if (pair) {
      return pair;
    }
  }

  for (const provider of providersPayload.providers) {
    const firstModel = Object.values(provider.models ?? {})[0];
    if (firstModel?.id) {
      return {providerID: provider.id, modelID: firstModel.id};
    }
  }

  return null;
}

function extractResponseText(result) {
  return (result?.data?.parts ?? [])
    .filter((part) => part.type === "text" && typeof part.text === "string")
    .map((part) => part.text.trim())
    .filter(Boolean)
    .join("\n\n");
}

function buildModelCandidates(repoModel, providersPayload, preferredModel) {
  const candidates = [];
  const seen = new Set();
  const pushCandidate = (candidate) => {
    if (!candidate?.providerID || !candidate?.modelID) {
      return;
    }

    const key = `${candidate.providerID}/${candidate.modelID}`;
    if (!seen.has(key)) {
      seen.add(key);
      candidates.push(candidate);
    }
  };

  pushCandidate(normalizeModelSelection(preferredModel, providersPayload));

  const normalizedRepoModel = normalizeModelSelection(repoModel, providersPayload);
  pushCandidate(normalizedRepoModel);

  const preferredProviders = [
    "minimax-coding-plan",
    "moonshotai",
    "kimi",
    "bigpickle",
    "big-pickle",
    "openrouter",
    "opencode",
    "LocalModel",
    "llama",
    "fireworks-ai",
    "nvidia",
  ];

  for (const providerID of preferredProviders) {
    const modelID = providersPayload.default?.[providerID];
    pushCandidate(normalizeModelSelection(`${providerID}/${modelID}`, providersPayload));
  }

  return candidates;
}

async function readSessionStatus(baseUrl, sessionId) {
  const response = await fetch(`${baseUrl}/session/status`);
  if (!response.ok) {
    return null;
  }

  const payload = await response.json();
  return payload?.[sessionId] ?? null;
}

async function abortSession(client, sessionId) {
  try {
    await client.session.abort({path: {id: sessionId}});
  } catch {
    // Best effort cleanup only.
  }
}

async function promptWithCandidate(client, baseUrl, prompt, candidate) {
  const session = await client.session.create({
    body: {title: `Meta Skill Studio Assistant (${candidate.providerID})`},
  });

  const promptPromise = client.session
    .prompt({
      path: {id: session.data.id},
      body: {
        agent: "general",
        model: candidate,
        system:
          "You are the embedded Meta Skill Studio assistant. Stay grounded in this repository, prefer concrete next actions, and help the user operate the repo-owned skills, library, and studio workflows safely.",
        tools: {},
        parts: [{type: "text", text: prompt}],
      },
    })
    .then((result) => ({kind: "result", result}))
    .catch((error) => ({kind: "error", error: error instanceof Error ? error.message : String(error)}));

  for (let i = 0; i < 30; i += 1) {
    const outcome = await Promise.race([
      promptPromise,
      sleep(1000).then(() => null),
    ]);

    if (outcome?.kind === "result") {
      const response = extractResponseText(outcome.result);
      if (!response) {
        await abortSession(client, session.data.id);
        return {ok: false, error: "The assistant returned no text response."};
      }

      return {ok: true, response, sessionId: session.data.id};
    }

    if (outcome?.kind === "error") {
      await abortSession(client, session.data.id);
      return {ok: false, error: outcome.error};
    }

    const status = await readSessionStatus(baseUrl, session.data.id);
    if (status?.type === "retry" && typeof status.message === "string") {
      await abortSession(client, session.data.id);
      return {ok: false, error: status.message};
    }

    if (status?.type === "error" && typeof status.message === "string") {
      await abortSession(client, session.data.id);
      return {ok: false, error: status.message};
    }
  }

  await abortSession(client, session.data.id);
  return {ok: false, error: "Timed out waiting for the assistant response."};
}

async function runAssistant(prompt, preferredModel) {
  const server = await startServer();
  const {baseUrl, child} = server;
  const client = createOpencodeClient({baseUrl, throwOnError: true});

  try {
    const providersPayload = (await client.config.providers()).data;
    const candidates = buildModelCandidates(readRepoModel(), providersPayload, preferredModel);
    const failures = [];

    for (const candidate of candidates) {
      const attempt = await promptWithCandidate(client, baseUrl, prompt, candidate);
      if (attempt.ok) {
        console.log(
          JSON.stringify({
            ok: true,
            response: attempt.response,
            sessionId: attempt.sessionId,
            model: `${candidate.providerID}/${candidate.modelID}`,
          })
        );
        return;
      }

      failures.push(`${candidate.providerID}/${candidate.modelID}: ${attempt.error}`);
    }
    throw new Error(`Assistant prompt failed for all candidate models. ${failures.join(" | ")}`);
  } finally {
    await stopServer(server);
  }
}

const args = parseArgs(process.argv.slice(2));
const prompt = resolvePrompt(args).trim();

if (!prompt) {
  console.error(JSON.stringify({ok: false, error: "Assistant prompt is required."}));
  process.exit(1);
}

try {
  if (args.action !== "assistant") {
    throw new Error(`Unsupported SDK bridge action: ${args.action}`);
  }

  await runAssistant(prompt, args.model?.trim() || null);
} catch (error) {
  console.error(
    JSON.stringify({
      ok: false,
      error: error instanceof Error ? error.message : String(error),
    })
  );
  process.exit(1);
}
