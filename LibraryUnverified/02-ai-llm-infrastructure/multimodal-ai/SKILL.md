---
name: multimodal-ai
description: >-
  Integrate vision, audio, and multimodal LLM capabilities into applications.
  Use when sending images to vision models, processing audio with Whisper,
  combining text+image inputs, or building multimodal pipelines. Do not use
  for text-only LLM tasks or embedding/search (prefer embeddings-indexing).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: multimodal-ai
  maturity: draft
  risk: low
  tags: [multimodal, vision, audio, llm]
---

# Purpose

Integrate vision, audio, and multimodal inputs into LLM applications using API and local model capabilities.

# When to use this skill

- sending images to GPT-4o, Claude, or Gemini for analysis
- transcribing audio with Whisper or similar models
- building pipelines that combine text, image, and audio inputs
- extracting structured data from documents, screenshots, or diagrams

# Do not use this skill when

- working with text-only LLM tasks — use standard prompting
- building vector search — prefer `embeddings-indexing`
- deploying inference servers — prefer `inference-serving`

# Procedure

1. **Identify modalities** — determine which inputs are needed: text, image (screenshot, photo, diagram), audio (speech, music), video (frame extraction).
2. **Choose model** — GPT-4o and Gemini for native multimodal; Claude for vision+text; Whisper for audio transcription.
3. **Prepare image inputs** — resize to model limits (GPT-4o: max 2048px short side). Encode as base64 or provide URL. Use `detail: "low"` for cost savings.
4. **Prepare audio inputs** — convert to supported format (Whisper: mp3, wav, m4a). Split files > 25MB into chunks.
5. **Structure the prompt** — place image/audio context before the question. Be specific: "Describe the error in this screenshot" not "What is this?".
6. **Handle responses** — parse structured output (JSON mode) for data extraction. Validate extracted fields.
7. **Implement fallbacks** — if vision model fails on complex diagrams, try OCR + text model as backup.
8. **Optimize cost** — use `detail: "low"` for images when high resolution is not needed (85 tokens vs 765+ tokens).

# Vision API patterns

```python
# OpenAI GPT-4o vision
import openai, base64

def analyze_image(image_path, question):
    with open(image_path, "rb") as f:
        b64 = base64.b64encode(f.read()).decode()

    response = openai.chat.completions.create(
        model="gpt-4o",
        messages=[{
            "role": "user",
            "content": [
                {"type": "text", "text": question},
                {"type": "image_url", "image_url": {
                    "url": f"data:image/png;base64,{b64}",
                    "detail": "high"
                }}
            ]
        }],
        max_tokens=1000
    )
    return response.choices[0].message.content
```

# Audio transcription

```python
# Whisper via OpenAI API
def transcribe(audio_path):
    with open(audio_path, "rb") as f:
        transcript = openai.audio.transcriptions.create(
            model="whisper-1",
            file=f,
            response_format="verbose_json",
            timestamp_granularities=["segment"]
        )
    return transcript
```

# Decision rules

- Use `detail: "low"` for images when you need general understanding, not pixel-level detail — saves 90% of image tokens.
- Gemini supports longest multimodal context (1M tokens) — best for video or many-image tasks.
- Always validate extracted data — vision models hallucinate on numbers, dates, and small text.
- For document extraction, try structured output (JSON mode) — more reliable than free-form text.
- Split long audio into segments with overlap — prevents information loss at chunk boundaries.

# References

- https://platform.openai.com/docs/guides/vision
- https://docs.anthropic.com/en/docs/build-with-claude/vision
- https://platform.openai.com/docs/guides/speech-to-text

# Related skills

- `model-selection` — choosing multimodal-capable models
- `context-management-memory` — managing multimodal token budgets
- `embeddings-indexing` — indexing extracted text from multimodal sources
