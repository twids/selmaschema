# GitHub Copilot Prompts

This directory contains reusable prompt templates for GitHub Copilot and AI assistants working on this project.

## Available Prompts

### Create Implementation Plan (`create-implementation-plan.prompt.md`)

A comprehensive framework for creating detailed, machine-readable implementation plans for features, bug fixes, or refactoring.

**Use When**:

- Planning a complex feature implementation
- Breaking down refactoring tasks
- Creating actionable work for the team
- Documenting technical approach for review

**Key Sections**:

- Status, effort, and priority tracking
- Requirements (user-facing and technical)
- Phased implementation steps with clear deliverables
- Testing strategy and validation approach
- Alternatives considered and trade-offs
- Dependencies, assumptions, and risks

**Example Output Format**:

```markdown
---
status: "planning"
effort: "large"
priority: "high"
---

# Feature Name - Implementation Plan

## Overview

Brief summary of implementation

## Requirements

- User-facing requirements
- Technical requirements

## Implementation Phases

### Phase 1: ...

### Phase 2: ...
```

**Tips**:

- Break work into phases that can be independently reviewed/merged
- Include specific file paths for each modification
- State measurable completion criteria for each phase
- Identify dependencies between phases to enable parallelization

---

## How to Use Prompts

### With GitHub Copilot in VS Code

Reference a prompt in your request:

```
Using the create-implementation-plan prompt structure,
plan the implementation for [feature name]
```

### For PR Descriptions and Planning

Copy the prompt template and fill in specific details:

```markdown
Use this as your PR description template:
<copy from create-implementation-plan.prompt.md>
```

### With Chat Assistants

Include the prompt context:

```
Reference: .github/prompts/create-implementation-plan.prompt.md

Please create an implementation plan for:
[your feature description]
```

## Creating New Prompts

To add new prompts to this collection:

1. Create a new `.prompt.md` file with a descriptive name
2. Include frontmatter with `name` and `description`
3. Structure the prompt with clear sections and examples
4. Add usage examples and tips
5. Update this README with the new prompt entry

## Prompt Naming Convention

Use descriptive names in kebab-case:

- `create-implementation-plan.prompt.md`
- `code-review-checklist.prompt.md`
- `api-endpoint-design.prompt.md`
- `component-architecture.prompt.md`

## Related Resources

- **Agents**: See `.github/agents/README.md` for specialized AI agents
- **Instructions**: See `.github/instructions/README.md` for development guidelines
- **Collections**: See `.github/collections/` for grouped asset sets

## Tips for Effective Prompts

1. **Be Specific**: Include context, examples, and expected output format
2. **Structure Clearly**: Use consistent sections and formatting
3. **Provide Examples**: Show what good output looks like
4. **Include Context**: Link to related instructions and agents
5. **Make Actionable**: Focus on outputs that can be directly used

## Community Prompts

This project uses awesome-copilot community prompts enhanced with project-specific context. For more community prompts:

- https://github.com/github/awesome-copilot/tree/main/prompts

## Usage Metrics

Track prompt effectiveness:

- Which prompts are used most frequently?
- Which produce the best outcomes?
- What new prompts does the team need?

Share feedback to improve the collection over time.
