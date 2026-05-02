# Parallelism Rules

Parallelize only when:

- the questions are independent
- the write scopes are disjoint
- the outputs do not need each other to be meaningful

Do not parallelize when:

- one result determines the other task shape
- both delegates would edit the same files
- the lead agent would still need to redo the synthesis anyway
