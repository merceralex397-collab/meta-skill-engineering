\# skill-evaluation is not good.



1\. If there is no usefulness data, it looks like this just gets skipped entirely. no backup plan. no nothing.. This is bad.

2\. If there are no evals at all it just skips? "It just skips with a warning and moves on — No evals/ directory: <skill> — no failure, no crash."



Requirement A: Usefulness Criteria shouldn't be optional. If skill-evaluation is called, this should proceed to call skill-testing-harness if there is no evals or usefulness data.



Requirement B: Skill-testing-harness should also have information on how to create the usefulness data as it doesn't appear to have this. To update skill-testing-harness and give it this ability, perhaps we could examine the existing data in the project, think carefully and decide whether its really good or needs improvement. Then we can use these as examples/references for it? After this it can be handed back to skill-evaluation process or improvement if its moving forwards.



Requirement C: skill-variant-splitting should also be an automatic part of the pipeline but I feel like maybe it isn't? If during evaluation/improvement its determined the skill is too long/broad in scope then this should be used.



Requirement D: When we first test a new skill with evaluator, we should also be testing a baseline version with no skill when using to the judge. Otherwise we don't know if the \*initial\* skill is worse than no skill.



Requirement E: LLM judge should also be the standard for testing. I'm unsure but I think 3 tests to 1 judge would be good? So run 3x eval, then 1x judge?



Requirement F: With regards to skills not automatically going to skill-improve we could have three modes for evaluator. The original two settings, full suite and ad-hoc seems dumb. We would ALWAYS want a full-suite. ad-hoc just seems random and prone to giving us nonsensical scorings depending on the prompt? That needs to go.







Mode 1: Iterate -> goes to improver immediately after, feedback is passed to improver as well from all that skills evals. Then from there it should be passed back to evaluator. This would repeat until the skill scores enough on our system. Unless user defines, Iterate should have a limit of 5 loops.

Mode 2: Investigate -> just runs the eval tests and generates the markdowns

Mode 3: Meta-Meta -> With regards to regression (I presume a regression is a skill that we improve, but then when we are automatically auditing as part of the standard skill chain we find that its worse), then this should halt any Iterate Mode process but trigger Meta-Meta mode (detailed below) to examine all skills involved that touched the skill. So I presume there's a skill in our chain that details / explains regression and what to do? This is skill vs old version, or skill vs no skill (if never eval'd before). Do we have criteria already for a regression? If not, I would say that since our default is 3 runs of each after an improvement (skill + old skill), then if 2 out of 3 evals favour the old version or no skill version, then an iterate loop should be broken immediately and Meta-Meta mode should start.



With regards to corpus-eval.sh - we should essentially consider this as akin to running the regular evals. Is there any actual difference?

We need a meta-meta-skill-examiner skill (possibly a meta-meta-skill-refiner skill too). This skill would work similarly to the evaluate -> improve system except its purely for targeting the existing skills in meta-skill-engineering.



If the user doesn't specify which mode of the three they want then if there's a facility to force the evaluator to ask, this would be preferential. If not, it should default to investigate rather than starting an autonomous loop. 



If the repo is Meta-Skill-Engineering it should \*ALWAYS\* be Meta-Meta mode though even if they don't ask, and just say "evaluate skill-evaluator" for example, as it would need the corpus tests. We may just be able to put this in AGENTS.md rather than a skill? 



Requirement G: Final complete sweep of the repository and the last 10 commits for any issues.



Requirement F: New folder - Workbench. This is where any skills currently being worked on will go. The process will work as follows: 

Every skill should have its own folder, so if a skill is changed/updated then a new folder will be made for that skill called SKILLNAMEMSE. Within that folder will be SKILLNAMEv2 (and if running on iterate, each iterate loop will save a new version, so v3, v4, v5, v6 etc).





Update documentation and all skills necessary (most likely improver) to reflect this new process.



Requirement H:

Test out the Meta-Skill-Engineering Library in full.



There are 6 folders in the workbench. 



5 of them are random skills. These should just be tested all on until iterate hits its 5 loop limit. The other is called Scafforge. This is a special case. Do not do the iterate loop on this until all the others are done.

When you have finished this:



1 of them is a skill suite called Scafforge that I use. This is far more complicated. It involves an entire system to generate a repository, complete with tickets, custom opencode agents, their own custom skills (albeit simple project convention skills - I am working to add better skills but I need to complete work on MSE first).

Crucially, Scafforge works in a similar way to MSE, where the skills work in a chain. Due to this, I am not certain 100% of the best way to benchmark / test / evaluate this. 



Run the Iterate loop 3 times on Scafforge.



Requirement I:

Create detailed reports in Workbench for the 5 random skills, and Scafforge as a whole. 

You should explain the differences between the original state, and the next iteration, and the next etc until final state.



Requirement J:

Critically review the performance of Meta-Skill-Engineering in all previous requirement/tasks

