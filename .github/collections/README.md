# GitHub Copilot Collections

This directory contains curated collections of related agents, prompts, and instructions for specific workflows and domains.

## What Are Collections?

Collections are groupings of GitHub Copilot assets (agents, prompts, instructions) organized around a specific domain, workflow, or project aspect. They help teams quickly access all relevant tools for a particular type of work.

## Using Collections

Collections can be used to:

- Load multiple related agents at once
- Apply relevant instructions for a workflow
- Use context-specific prompts and guidelines
- Speed up onboarding for new team members
- Organize assets by domain (Frontend, Backend, DevOps, etc.)

## Creating Collections

To create a new collection:

1. Create a `.collection.yml` or `.collection.md` file with a descriptive name
2. Include metadata about the collection:

   ```yaml
   name: Frontend Development
   description: Tools and guidelines for React/Next.js development
   tags: [frontend, react, typescript]
   ```

3. List the included assets:

   ```yaml
   agents:
     - expert-react-frontend-engineer
     - expert-nextjs-developer

   instructions:
     - reactjs.instructions.md
     - typescript-5-es2022.instructions.md

   prompts:
     - create-implementation-plan.prompt.md
   ```

## Collection Naming Convention

Use descriptive names in kebab-case:

- `frontend-development.collection.yml`
- `backend-api-design.collection.yml`
- `devops-deployment.collection.yml`
- `security-review.collection.yml`

## Example Collection Structure

```yaml
name: "Frontend Development"
description: "Complete toolkit for React/Next.js frontend development"
version: "1.0"
tags: [frontend, react, typescript, nextjs]

agents:
  - expert-react-frontend-engineer
  - expert-nextjs-developer

instructions:
  - reactjs.instructions.md
  - typescript-5-es2022.instructions.md

prompts:
  - create-implementation-plan.prompt.md

related_collections:
  - frontend-testing
  - accessibility-compliance
```

## Planned Collections

The following collections should be created to fully organize the project:

### Frontend Development

- Expert React Frontend Engineer agent
- Expert Next.js Developer agent
- ReactJS instructions
- TypeScript instructions
- Create Implementation Plan prompt

### Backend Development

- (Planned: Expert .NET Developer agent)
- TypeScript instructions
- Security review code-reviewer agent
- Docker best practices instructions

### DevOps & Deployment

- Docker & Containerization instructions
- (Planned: DevOps specialist agent)
- Create Implementation Plan prompt

### Security & Code Review

- Code Reviewer (SE: Security) agent
- Docker best practices (security section)
- TypeScript security guidelines
- React security patterns

## Benefits of Collections

- **Quick Setup**: Load all relevant tools for a task with one reference
- **Consistency**: Ensure teams use the same standards and patterns
- **Discovery**: Help team members find relevant resources
- **Onboarding**: New developers can load a collection relevant to their role
- **Organization**: Group related assets logically

## Managing Collections

### Viewing Available Collections

```bash
# List all collections in the repository
ls -la .github/collections/
```

### Using a Collection in Copilot

Reference a collection when asking for help:

```
@assistant Load the frontend-development collection and help me build this component
```

### Updating Collections

1. Modify the `.collection.yml` file
2. Add/remove agents, instructions, or prompts as needed
3. Update the description and version
4. Commit changes with clear messages

## Related Resources

- **Agents**: See `.github/agents/README.md` for individual agent details
- **Instructions**: See `.github/instructions/README.md` for development guidelines
- **Prompts**: See `.github/prompts/README.md` for reusable prompt templates

## Community Collections

awesome-copilot provides community-created collections. View them at:

- https://github.com/github/awesome-copilot/tree/main/collections

## Contributing

To suggest new collections or improvements:

1. Open an issue describing the collection need
2. Propose which existing assets should be included
3. Submit a PR with the new collection file
4. Update this README with the new collection
