---
name: image-prompt-direction
description: >-
  Crafts structured prompts for visual AI image generators (Midjourney, DALL-E,
  Stable Diffusion) using systematic prompt architecture: subject, style, composition,
  lighting, mood, and technical parameters. Includes negative prompting and model-specific
  parameter tuning.
  Trigger: "write a Midjourney prompt", "image prompt for", "DALL-E prompt",
  "Stable Diffusion prompt", "art direction for AI", "negative prompt",
  "style reference", "prompt engineering for images", "consistent character".
  Do NOT use for text-based prompt engineering (use prompt-crafting), for editing
  existing images (use image-editor), or for generating images directly (use imagegen).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: image-prompt-direction
  maturity: draft
  risk: low
  tags: [image-generation, prompt-engineering, art-direction, midjourney, dall-e, stable-diffusion]
---

# Purpose

Provides a systematic methodology for crafting effective prompts for visual AI
image generators. Covers prompt architecture, style vocabulary, composition controls,
lighting direction, negative prompting, model-specific parameter tuning, and consistency
techniques so that generated images reliably match the creator's intent.

# When to use this skill

- User asks to write, improve, or debug a prompt for Midjourney, DALL-E, Stable Diffusion, or similar
- Task requires art direction: defining visual style, mood, composition, or lighting for AI generation
- User needs negative prompts to eliminate artifacts or unwanted elements
- Consistency work: maintaining a character, style, or scene across multiple generations
- User wants to understand model-specific parameters (--ar, --v, --s, CFG scale, sampling steps)
- Batch prompt creation for a series of related images (e.g., product shots, character sheets, storyboards)
- User says "this image isn't coming out right" and needs prompt diagnosis

# Do not use this skill when

- The task is text-based prompt engineering for LLMs — use `prompt-crafting`
- The user wants to edit or manipulate an existing image — use `image-editor`
- The user wants to directly generate an image (not craft the prompt) — use `imagegen`
- The task is brand identity or logo design process — use `brand-guidelines` for strategy, then this skill for generation prompts
- The user needs to build a UI that displays images — use `frontend-design` or `react-typescript`

# Operating procedure

## Step 1 — Creative brief intake

Gather these inputs before writing any prompt:

1. **Subject** — what is the image of? (person, object, scene, concept)
2. **Purpose** — where will this image be used? (social media, website hero, print, concept art, personal)
3. **Target model** — which generator? (Midjourney v6, DALL-E 3, SDXL, Flux, other)
4. **Aspect ratio** — landscape (16:9), portrait (9:16), square (1:1), or custom
5. **Style direction** — any reference images, artists, art movements, or aesthetic keywords
6. **Constraints** — brand guidelines, content policies, elements that must NOT appear
7. **Quantity** — single image or series; if series, what varies between images

## Step 2 — Prompt architecture

Build the prompt using this layered structure, from most to least important:

```
[Subject & Action] + [Environment/Setting] + [Style & Medium] + [Composition & Framing] + [Lighting] + [Mood & Atmosphere] + [Color Palette] + [Technical Parameters]
```

### Subject & action layer
- Lead with the primary subject: "a weathered lighthouse keeper", not "an image of a man"
- Include action or pose if relevant: "standing at the railing, looking out to sea"
- Specify key details: age, expression, clothing, distinguishing features
- Use concrete nouns over abstract descriptions

### Environment / setting layer
- Ground the subject in a specific place: "on a rocky coastal cliff during a storm"
- Include time of day and season if relevant to lighting/mood
- Add depth cues: foreground, midground, background elements

### Style & medium vocabulary

| Category | Examples |
|---|---|
| **Photorealistic** | editorial photography, 35mm film, DSLR, Hasselblad, Canon EOS R5 |
| **Illustration** | digital illustration, concept art, matte painting, book illustration |
| **Traditional media** | watercolor, oil painting, gouache, charcoal sketch, ink wash |
| **Graphic styles** | vector art, flat design, isometric, pixel art, low-poly, voxel art |
| **Art movements** | Art Nouveau, Art Deco, Impressionism, Surrealism, Ukiyo-e, Bauhaus |
| **Contemporary** | synthwave, vaporwave, cyberpunk, solarpunk, cottagecore, dark academia |

### Composition & framing controls

| Technique | Prompt language |
|---|---|
| **Rule of thirds** | "subject positioned at the left third intersection" |
| **Golden ratio** | "golden spiral composition" |
| **Leading lines** | "converging railway tracks drawing the eye to the vanishing point" |
| **Depth of field** | "shallow depth of field, bokeh background", "f/1.4 aperture" |
| **Camera angle** | "bird's-eye view", "worm's-eye view", "eye-level", "Dutch angle" |
| **Shot type** | "extreme close-up", "medium shot", "wide establishing shot", "over-the-shoulder" |
| **Symmetry** | "perfectly symmetrical composition", "bilateral symmetry" |

### Lighting vocabulary

| Lighting type | Visual effect | Prompt language |
|---|---|---|
| **Rembrandt** | Triangle of light on shadow-side cheek | "Rembrandt lighting, dramatic triangle shadow" |
| **Rim / back lighting** | Glowing edge outline | "rim lighting, backlit silhouette glow" |
| **Golden hour** | Warm, soft, directional | "golden hour sunlight, warm tones, long shadows" |
| **Studio** | Clean, controlled | "studio lighting, softbox, white background" |
| **Chiaroscuro** | High contrast, dramatic | "chiaroscuro lighting, deep shadows, Caravaggio-inspired" |
| **Overcast / diffused** | Soft, even, no harsh shadows | "overcast sky, diffused natural light, soft shadows" |
| **Neon / colored** | Stylized, cyberpunk | "neon-lit, pink and cyan light spill, wet reflections" |
| **Volumetric** | God rays, fog beams | "volumetric lighting, god rays through fog" |

### Mood & atmosphere
- Use emotional adjectives: "melancholic", "triumphant", "eerie", "serene", "chaotic"
- Pair mood with environmental cues: "desolate wasteland" vs. "lush paradise"
- Color temperature reinforces mood: warm = comfort/energy, cool = calm/isolation

## Step 3 — Negative prompting

Define exclusions to steer the model away from common failure modes:

**Universal negative prompt baseline:**
```
blurry, low quality, low resolution, watermark, text, signature, cropped, out of frame, worst quality, jpeg artifacts
```

**Anatomy-specific (for human subjects):**
```
deformed hands, extra fingers, extra limbs, fused fingers, mutated hands, bad anatomy, malformed limbs, extra arms, extra legs, cloned face
```

**Style-specific negatives:**
- Photorealistic: `illustration, cartoon, anime, painting, drawing, CGI`
- Illustration: `photo, photograph, realistic, photographic`
- Clean design: `cluttered, busy background, noisy, grain`

**Guidance:** Only include negatives relevant to your target style. Over-stuffing the negative prompt can constrain the model too aggressively and produce flat results.

## Step 4 — Model-specific parameters

### Midjourney (v5/v6)

| Parameter | Purpose | Range / values |
|---|---|---|
| `--ar` | Aspect ratio | `1:1`, `16:9`, `9:16`, `3:2`, `2:3`, `21:9` |
| `--v` | Model version | `5.2`, `6`, `6.1` |
| `--s` (stylize) | How strongly Midjourney applies its aesthetic | `0`–`1000`; low = literal, high = artistic |
| `--c` (chaos) | Variation between results | `0`–`100`; low = similar, high = diverse |
| `--q` (quality) | Rendering time/detail | `0.25`, `0.5`, `1` (default), `2` |
| `--no` | Negative prompt | `--no text, watermark, blurry` |
| `--seed` | Reproducibility | Any integer; same seed + same prompt ≈ same image |
| `--sref` | Style reference image URL | `--sref <url>` to match a visual style |
| `--cref` | Character reference image URL | `--cref <url>` to maintain character consistency |

### DALL-E 3 (via ChatGPT / API)

- No direct parameter flags; control via detailed natural language in the system prompt
- Specify aspect ratio in the request: "wide landscape format", "tall portrait format", "square"
- Use "I NEED the image to exactly match this description" to reduce DALL-E's prompt rewriting
- Negative concepts go in the prompt body: "without any text or watermarks"
- Seed control: not user-facing; use very specific descriptions for consistency

### Stable Diffusion (SDXL / SD 1.5)

| Parameter | Purpose | Recommended range |
|---|---|---|
| **CFG Scale** | Prompt adherence vs. creativity | `5`–`12`; `7` is a good default |
| **Sampling steps** | Detail vs. speed | `20`–`50`; `30` is a good default |
| **Sampler** | Denoising algorithm | Euler a (fast), DPM++ 2M Karras (quality), DDIM (consistent) |
| **Seed** | Reproducibility | Any integer |
| **Resolution** | Output dimensions | SDXL: `1024×1024`, `1216×832`, `832×1216`; SD 1.5: `512×512`, `768×512` |
| **LoRA / embeddings** | Style or character fine-tuning | Specify model name and weight (e.g., `<lora:stylename:0.7>`) |
| **Negative prompt** | Separate field for exclusions | Use the negative prompts from Step 3 |

## Step 5 — Consistency techniques

For maintaining visual coherence across a series:

1. **Seed pinning** — use the same seed number across variations; change only the element you want to vary.
2. **Reference images** — provide reference URLs (Midjourney `--sref`, `--cref`) or img2img in SD.
3. **Style transfer** — lock the style by referencing a specific artist, movement, or prior generation.
4. **Character sheets** — generate a multi-angle character reference sheet first, then use it as reference for scene images.
5. **Prompt templating** — create a base prompt template with `{variable}` slots; only swap the variable per image.
6. **Consistent terminology** — use the exact same descriptive phrases for recurring elements across all prompts in a series.

## Step 6 — Iteration and refinement

1. Generate initial results with the crafted prompt.
2. Diagnose issues using this checklist:
   - Wrong style → adjust style/medium keywords; add style-negatives
   - Wrong composition → make framing/angle instructions more explicit
   - Wrong lighting → specify lighting type by name; add "no flash photography" if too flat
   - Artifacts → strengthen negative prompt; adjust CFG or steps
   - Too literal / too abstract → tune stylize (MJ) or CFG (SD)
3. Document what changed between iterations and why (Iteration Notes).
4. After 3+ failed iterations, reconsider the fundamental approach rather than tweaking parameters.

# Decision rules

- **Prompt length > 75 tokens (SD) or overly complex** → simplify; focus on the most important 5–7 descriptors. Models lose coherence with prompt-stuffing.
- **Conflicting styles in one prompt** (e.g., "photorealistic watercolor") → pick one style; use blending only with explicit intent and model support.
- **No aspect ratio specified** → always specify; default square rarely matches the use case.
- **No negative prompt** → always include at minimum the universal baseline negatives.
- **Anatomy in the image** → always include anatomy-specific negatives for human/animal subjects.
- **Requesting text in images** → warn that most models render text poorly; suggest adding text in post-production. DALL-E 3 handles short text better than others.
- **Brand or commercial use** → flag content policy and licensing considerations for the target model.
- **Reproducibility required** → mandate seed pinning and document all parameters.
- **Vague style words** ("cool", "nice", "beautiful") → replace with specific aesthetic vocabulary from the style table.

# Output structure

Deliver these sections for each prompt request:

## 1. Prompt Brief

Summary of creative intent: subject, purpose, target model, aspect ratio, style direction.

## 2. Full Prompt

The complete, ready-to-paste prompt text with all layers applied.

```
[Full prompt text here]
```

## 3. Negative Prompt

Separate negative prompt text (for SD; for Midjourney, integrated via `--no`).

```
[Negative prompt text here]
```

## 4. Parameter Settings

Model-specific parameters in a copy-pasteable format:

```
--ar 16:9 --v 6 --s 250 --seed 42
```

or

```
CFG: 7 | Steps: 30 | Sampler: DPM++ 2M Karras | Seed: 42 | Resolution: 1024x1024
```

## 5. Iteration Notes

Table tracking changes across prompt versions:

| Version | Change made | Reason | Result |
|---|---|---|---|
| v1 | Initial prompt | — | — |
| v2 | Added "Rembrandt lighting" | First version too flat | Improved shadow depth |

## 6. Consistency Guide (for series)

Template, fixed elements, variable elements, seed, and reference image strategy.

# Anti-patterns

- **Prompt-dumping** — cramming 20+ disconnected style keywords into one prompt. Models lose coherence; pick 3–5 complementary descriptors maximum.
- **Ignoring aspect ratio** — generating square images for a 16:9 banner creates cropping headaches or distortion. Always match output to intended use.
- **No negative prompts** — omitting negatives in Stable Diffusion especially leads to watermarks, text artifacts, and anatomical errors that are trivially preventable.
- **Vague style words** — "beautiful", "stunning", "epic" carry almost zero signal for the model. Replace with concrete aesthetics: "golden hour rim lighting" beats "beautiful lighting".
- **Conflicting style instructions** — "photorealistic anime oil painting" fights itself. Be intentional about style blending or pick one.
- **Ignoring model strengths** — using Stable Diffusion for text-heavy graphics (DALL-E 3 is better) or DALL-E for precise parameter control (SD is better). Match task to model.
- **No seed documentation** — generating a great image and not recording the seed or parameters means it cannot be reproduced or iterated upon.
- **Over-relying on artist names** — ethical and legal grey area; also, model knowledge of specific artists varies. Describe the visual qualities you want instead.
- **Skipping iteration notes** — repeating the same failed adjustments because previous attempts weren't documented.

# Related skills

- `imagegen` — directly generating images (this skill crafts the prompt; imagegen executes it)
- `prompt-crafting` — text-based prompt engineering for LLMs (sister skill for non-visual prompting)
- `brand-guidelines` — ensuring generated images align with brand identity standards
- `image-editor` — post-production editing of generated images
- `worldbuilding-research` — generating consistent visual world-building assets
- `canvas-design` — composing generated images into layouts and designs

# Failure handling

- If the user provides no subject or purpose, ask: "What is the image of, and where will it be used?"
- If the target model is not specified, default to Midjourney v6 parameters but note the assumption and offer alternatives.
- If the user's description contains conflicting style directions, surface the conflict and ask them to prioritize.
- If the prompt exceeds the model's effective token window, summarize and offer both a full and condensed version.
- If the user expects photorealistic text rendering, warn about model limitations and suggest compositing text in post-production.
- If consistency across a series is failing, recommend switching to a seed-pinning + reference-image strategy before further iteration.
