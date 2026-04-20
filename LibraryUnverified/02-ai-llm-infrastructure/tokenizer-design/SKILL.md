---
name: tokenizer-design
description: Design, train, and evaluate tokenizers (BPE, SentencePiece unigram) for LLMs. Use when selecting vocabulary size, defining special tokens, training a tokenizer on a corpus, analyzing fertility/compression, or handling multilingual coverage. Covers the HuggingFace `tokenizers` library and `SentencePieceTrainer`.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: tokenizer-design
  maturity: draft
  risk: low
  tags: [tokenizer, bpe, sentencepiece, vocabulary, nlp]
---

# Purpose

Guide the design, training, and evaluation of tokenizers for language models—covering algorithm selection (BPE vs unigram), vocabulary sizing, special token definition, pre-tokenization strategy, multilingual script coverage, and compression ratio analysis.

# When to use this skill

- Training a new BPE or unigram tokenizer on a custom corpus
- Choosing vocabulary size (32k / 64k / 128k) and analyzing its impact on embedding parameters and sequence length
- Defining special tokens (`<bos>`, `<eos>`, `<pad>`, `<unk>`) or chat template tokens (`<|im_start|>`, `<|im_end|>`)
- Configuring pre-tokenization: whitespace splitting, digit isolation, byte-level fallback
- Measuring tokenizer fertility (tokens per word) and compression ratio (bytes per token)
- Ensuring multilingual coverage with character coverage thresholds (e.g., 0.9999)

# Do not use this skill when

- The task is model architecture design (weight layout, attention)—use `model-architecture`
- The task is end-to-end pretraining orchestration—use `pretraining-pipeline`
- You only need to *load* an existing tokenizer for inference with no design decisions

# Operating procedure

1. **Select algorithm.** Choose BPE (`tokenizers.models.BPE`) for merge-rule transparency or SentencePiece unigram for probabilistic subword selection. Use `sentencepiece.SentencePieceTrainer.train()` for language-agnostic byte-fallback support.
2. **Set vocabulary size.** Start with 32k for single-language models; use 64k–128k for multilingual. Each vocab entry adds `hidden_dim` parameters to the embedding matrix—quantify the tradeoff.
3. **Define special tokens.** Register `<bos>`, `<eos>`, `<pad>`, `<unk>` at fixed IDs. For chat models add role delimiters: `<|im_start|>system`, `<|im_end|>`. Reserve a contiguous block for future additions.
4. **Configure pre-tokenization.** Use `pre_tokenizers.ByteLevel(add_prefix_space=False)` for GPT-style or `pre_tokenizers.Sequence([Whitespace(), Digits(individual_digits=True)])` for Llama-style digit splitting.
5. **Train the tokenizer.**
   ```python
   from tokenizers import Tokenizer, models, trainers, pre_tokenizers
   tokenizer = Tokenizer(models.BPE())
   tokenizer.pre_tokenizer = pre_tokenizers.ByteLevel(add_prefix_space=False)
   trainer = trainers.BpeTrainer(vocab_size=64000, special_tokens=["<pad>","<eos>","<bos>","<unk>"])
   tokenizer.train(files=["corpus.txt"], trainer=trainer)
   ```
   For SentencePiece: `spm.SentencePieceTrainer.train(input="corpus.txt", model_prefix="tok", vocab_size=64000, character_coverage=0.9999, model_type="bpe")`.
6. **Analyze fertility.** Compute tokens-per-word on held-out samples per language. Target ≤1.5 for English, ≤2.5 for CJK. Compute bytes-per-token; higher is better compression.
7. **Test round-trip fidelity.** Assert `decode(encode(text)) == text` for edge cases: Unicode emoji, mixed-script, code blocks, LaTeX math, zero-width joiners.
8. **Export.** Save as `tokenizer.json` (HuggingFace fast tokenizer format) or `.model` (SentencePiece). Commit the trained artifact alongside its training config for reproducibility.

# Decision rules

- If the corpus is multilingual (>3 scripts), prefer SentencePiece with `byte_fallback=True` to avoid `<unk>` on rare characters.
- If vocabulary budget is constrained, prefer 32k with byte-level fallback over 64k without—smaller embedding matrix, zero OOV.
- If the model will process code, ensure digit splitting is enabled and whitespace sequences are preserved as individual tokens.
- Never silently drop characters—verify `<unk>` rate is 0% on a diverse validation set.

# Output requirements

1. `Tokenizer config` — algorithm, vocab size, pre-tokenizer chain, special token map with fixed IDs
2. `Training command` — reproducible script or snippet with corpus path, coverage, and vocab size
3. `Fertility report` — tokens-per-word and bytes-per-token per language/domain on held-out data
4. `Round-trip test results` — pass/fail on Unicode, code, math, and multilingual edge cases

# References

- HuggingFace `tokenizers` docs: https://huggingface.co/docs/tokenizers
- SentencePiece repo: https://github.com/google/sentencepiece
- Kudo & Richardson, "SentencePiece: A simple and language independent subword tokenizer" (EMNLP 2018)
- Sennrich et al., "Neural Machine Translation of Rare Words with Subword Units" (ACL 2016)

# Related skills

- `model-architecture` — embedding layer sizing depends on vocab size chosen here
- `pretraining-pipeline` — tokenizer must be finalized before data preprocessing begins
- `llm-creation` — end-to-end model creation references tokenizer design decisions

# Failure handling

- If fertility exceeds 3.0 tokens/word for a target language, increase `character_coverage` or add that language's data to the training corpus and retrain.
- If round-trip decode fails, check for normalization (NFKC) conflicts in SentencePiece—disable normalization or switch to byte-level BPE.
- If vocab size causes OOM in the embedding layer, reduce vocab and verify compression ratio remains acceptable before proceeding.
