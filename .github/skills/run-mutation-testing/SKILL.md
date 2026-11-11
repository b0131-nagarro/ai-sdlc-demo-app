---
name: run-mutation-testing
description: Runs Stryker.NET on the approval code and explains how to handle surviving mutants.
---
# Mutation testing
- Run from the test project folder: dotnet stryker (reads stryker-config.json).
- HTML report: StrykerOutput/<timestamp>/reports/mutation-report.html
- For each Survived mutant: write a test that fails on the mutant, or explain why it is equivalent
(no observable behaviour change).
- Never change production code to raise the score. Never delete a test to raise the score.
- Record the final score line ("The final mutation score is ...") in docs/mutation/.