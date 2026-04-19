# Leading Indicators

A useful premortem does not stop at causes. It names the first signals.

## Examples

| Failure mode | Early indicator |
| --- | --- |
| Hidden integration mismatch | staging errors or schema mismatches during dry-run |
| Adoption failure | low completion rate on the first-run flow |
| Operational fragility | canary alerts, elevated retries, unexplained manual intervention |
| Team/process failure | repeated unresolved blockers, slipping review turnarounds |
| Data migration failure | sample audit mismatch, unexpected nulls, duplicate keys |

Prefer leading indicators that can be checked before customer harm.
