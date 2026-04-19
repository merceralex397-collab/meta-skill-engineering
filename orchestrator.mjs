#!/usr/bin/env node
/**
 * orchestrator.mjs - SDK-based mass parallel evaluation orchestrator
 * 
 * Uses @opencode-ai/sdk to manage multiple OpenCode sessions for parallel evaluation.
 * Based on the patterns from scripts/run-evals.sh but using OpenCode SDK instead of copilot CLI.
 * 
 * Usage: node orchestrator.mjs [--skills skill1,skill2,...] [--workers N]
 */

import { createOpencodeClient } from "@opencode-ai/sdk";

const DEFAULT_WORKERS = 5;
const DEFAULT_SERVER = "http://127.0.0.1:4096";

const args = process.argv.slice(2);
const skillsArg = args.find(a => a.startsWith("--skills="));
const workersArg = args.find(a => a.startsWith("--workers="));

const SKILLS = skillsArg 
  ? skillsArg.split("=")[1].split(",")
  : ["skill-creator", "skill-evaluation", "skill-improver", "skill-trigger-optimization", "skill-anti-patterns"];
const WORKERS = workersArg ? parseInt(workersArg.split("=")[1]) : DEFAULT_WORKERS;

async function main() {
  console.log(`Starting orchestrator with ${WORKERS} workers`);
  console.log(`Skills: ${SKILLS.join(", ")}`);
  console.log(`Server: ${DEFAULT_SERVER}`);

  const client = createOpencodeClient({ baseUrl: DEFAULT_SERVER });

  const health = await client.global.health();
  console.log(`Server healthy: ${health.data.healthy}, version: ${health.data.version}`);

  const sessions = await Promise.all(
    SKILLS.slice(0, WORKERS).map(async (_, i) => {
      const session = await client.session.create({
        body: { title: `eval-worker-${i + 1}` }
      });
      console.log(`Created session ${session.id} for worker ${i + 1}`);
      return session;
    })
  );

  const assignments = SKILLS.map((skill, i) => ({
    session: sessions[i % sessions.length],
    skill
  }));

  console.log(`\nDistributing ${SKILLS.length} skills across ${sessions.length} workers...`);

  const results = await Promise.allSettled(
    assignments.map(async ({ session, skill }) => {
      console.log(`Worker ${skill}: Running eval...`);
      
      const result = await client.session.prompt({
        path: { id: session.id },
        body: {
          parts: [{ type: "text", text: `./scripts/run-evals.sh ${skill} --observe` }]
        }
      });

      return { skill, success: true, result };
    })
  );

  const succeeded = results.filter(r => r.status === "fulfilled").length;
  const failed = results.filter(r => r.status === "rejected").length;

  console.log(`\n=== Results ===`);
  console.log(`Succeeded: ${succeeded}/${SKILLS.length}`);
  console.log(`Failed: ${failed}/${SKILLS.length}`);

  for (const result of results) {
    if (result.status === "rejected") {
      console.error(`Error: ${result.reason}`);
    }
  }

  console.log("\nOrchestrator complete.");
}

main().catch(console.error);
