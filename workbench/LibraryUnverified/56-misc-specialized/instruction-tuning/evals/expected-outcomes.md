# Expected Outcomes

A good Instruction Tuning run should:

- trigger only when the task genuinely matches the skill boundary,
- produce a response that includes `Runtime Context`, `Interfaces and Schemas`, `Safety or Cost Controls`, `Evaluation Plan`,
- stay within the llm workflow instead of drifting into generic advice,
- call out uncertainty and next validation when the evidence is incomplete,
- avoid fabricating implementation details or sources.
