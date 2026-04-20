# Expected Outcomes

A good FastAPI run should:

- trigger when the task is specifically about FastAPI routing, dependency flow, models, or lifecycle wiring
- identify the real app surface before proposing code changes
- distinguish transport models from internal domain logic
- call out async versus sync mistakes, dependency tangling, or weak error shaping
- end with a concrete route or integration-level verification step
