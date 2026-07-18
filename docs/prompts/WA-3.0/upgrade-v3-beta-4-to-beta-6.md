We are upgrading the Web Awesome Blazor wrapper library from Web Awesome 3.0.0-beta.4 to 3.0.0-beta.6.

Inputs:
- Up-to-date documenation for 3.0.0-beta.6: @inputs\WebAwesome
- Documentation diff from 3.0.0-beta.4: @inputs\diff_3.0.0-beta.6.patch

Task - planning phase:
- Analyze the differences, read the changed documentation and identify changes that have impact on the Blazor wrappers.
- Review the respective source code in @src\WebAwesome.Blazor and determine what needs to be changed and/or added.

Task - working phase:
- Write an implementation document with a task list.

Output:
- Implementation document that can be used as instructions for Claude Code to actually implement the features step-by-step, stored in: @docs\prompts\WA-3.0\upgrade-v3-beta-4-to-beta-6-plan.md
- The output is produced as a result of approving Claude Code's plan.

Limitations:
- Do not make assumptions.
- When in doubt, ask.

Out of scope:
- **Do not code**: Any source code changes and/or addition are out of scope.
- **Do not modify the documentation**: The Web Awesome documentation in @inputs\WebAwesome is read-only and considered only as input. Do not plan for any changes in it, not even fixing typos.