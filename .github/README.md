# GitHub Copilot Custom Workflows & Awesome-Copilot Integration

This directory contains all GitHub Copilot assets for the selmaschema project, following the community-standard awesome-copilot directory structure.

## Directory Structure

```
.github/
├── agents/              # Specialized AI agents for specific domains
├── instructions/        # Development guidelines and best practices
├── prompts/            # Reusable prompt templates for common tasks
├── collections/        # Curated groupings of related assets
└── README.md          # This file
```

## Quick Start

### Using Agents in VS Code

1. Type `@` in Copilot Chat to open the agent picker
2. Select your desired agent:

   - `@agent-code-reviewer` - Security-focused code review
   - `@agent-expert-react-frontend-engineer` - React 19.2 expertise
   - `@agent-expert-nextjs-developer` - Next.js 16 expertise

3. Ask your question:
   ```
   @agent-expert-react-frontend-engineer
   How should I structure a form component with validation?
   ```

### Applying Development Instructions

Reference instructions in your development:

```
According to our typescript-5-es2022.instructions.md, how should I handle this error?
```

Use with Copilot:

```
@assistant
Please follow the ReactJS development instructions
while helping me refactor this component.
```

### Using Prompts for Planning

Load a prompt template when planning:

```
Generate an implementation plan using the
create-implementation-plan.prompt.md structure for this feature.
```

## Available Assets

### Agents (`.github/agents/`)

| Agent                                                                             | Purpose                      | Tech Stack                             |
| --------------------------------------------------------------------------------- | ---------------------------- | -------------------------------------- |
| [Code Reviewer](agents/README.md#code-reviewer)                                   | Security-focused code review | OWASP, Zero Trust, Reliability         |
| [Expert React Frontend Engineer](agents/README.md#expert-react-frontend-engineer) | React 19.2+ development      | React, TypeScript, Hooks               |
| [Expert Next.js Developer](agents/README.md#expert-nextjs-developer)              | Next.js 16+ development      | Next.js, App Router, Server Components |

**See [agents/README.md](agents/README.md) for full details and usage examples.**

### Instructions (`.github/instructions/`)

| Instruction                                                                                      | Coverage                   | When to Use                        |
| ------------------------------------------------------------------------------------------------ | -------------------------- | ---------------------------------- |
| [TypeScript Development](instructions/typescript-5-es2022.instructions.md)                       | TS 5.x/ES2022 standards    | All TypeScript code                |
| [ReactJS Development](instructions/reactjs.instructions.md)                                      | React 19+ best practices   | Building React components          |
| [Docker & Containerization](instructions/containerization-docker-best-practices.instructions.md) | Docker/container standards | Dockerfile and docker-compose work |

**See [instructions/README.md](instructions/README.md) for full details.**

### Prompts (`.github/prompts/`)

| Prompt                                                                     | Use Case                      | Output                           |
| -------------------------------------------------------------------------- | ----------------------------- | -------------------------------- |
| [Create Implementation Plan](prompts/create-implementation-plan.prompt.md) | Planning features/refactoring | Structured implementation phases |

**See [prompts/README.md](prompts/README.md) for full details.**

### Collections (`.github/collections/`)

Collections group related agents, instructions, and prompts for specific workflows:

- Frontend Development (planned)
- Backend Development (planned)
- DevOps & Deployment (planned)
- Security & Code Review (planned)

**See [collections/README.md](collections/README.md) for full details.**

## Project Stack Context

These assets are curated for the selmaschema project:

**Frontend:**

- React 19.2+ with TypeScript
- Next.js 16+ (App Router, Turbopack)
- Vite build tool
- Playwright E2E testing

**Backend:**

- .NET 8 C# (Minimal API)
- PostgreSQL 16 database
- Docker containerization

**DevOps:**

- Docker & Docker Compose
- GitHub Actions CI/CD
- Multi-stage builds

## Workflow Examples

### Planning a New Feature

1. Use the **Create Implementation Plan** prompt to structure the work
2. Reference relevant **instructions** while planning
3. Tag the appropriate **agent** when asking for implementation details

```
@assistant
Use .github/prompts/create-implementation-plan.prompt.md
to create a plan for adding user authentication.

Follow .github/instructions/typescript-5-es2022.instructions.md
for backend code and
.github/instructions/reactjs.instructions.md
for frontend components.
```

### Building a React Component

```
@agent-expert-react-frontend-engineer

I need to build a form component with validation.
Please follow the patterns in our ReactJS development instructions.
```

### Code Security Review

```
@agent-code-reviewer

Please review this code for security vulnerabilities.
Check against OWASP Top 10 and our security standards.
```

### Next.js Implementation

```
@agent-expert-nextjs-developer

I'm building a new API route with data fetching.
How should I handle caching and Partial Pre-Rendering?
```

## Setting Up Custom Workflows

### In GitHub Actions

Reference agents in your CI/CD workflows:

```yaml
- name: Request Copilot Review
  uses: github/copilot-actions/review@v1
  with:
    agent: code-reviewer
```

### In VS Code

1. Install the GitHub Copilot extension
2. Sign in with your GitHub account
3. Access agents and instructions through:
   - Copilot Chat (`Ctrl+Shift+I` / `Cmd+Shift+I`)
   - Inline Chat (`Ctrl+I` / `Cmd+I`)
   - Ask inline with `@` mentions

### Team Integration

1. **Share with Team**: Reference this README in onboarding docs
2. **Customize Assets**: Fork awesome-copilot assets and customize for your team
3. **Version Control**: Keep `.github/` in git for consistency
4. **Documentation**: Update README when adding new agents/instructions

## Adding New Assets

### Adding a New Agent

1. Create `.github/agents/new-agent.agent.md`
2. Follow the structure of existing agents (agents/README.md)
3. Include expertise areas, approach, and examples
4. Update `agents/README.md` with the new agent

### Adding New Instructions

1. Create `.github/instructions/topic.instructions.md`
2. Include core intent, guardrails, and practical examples
3. Structure with clear sections for easy reference
4. Update `instructions/README.md`

### Adding New Prompts

1. Create `.github/prompts/task-name.prompt.md`
2. Include frontmatter with name and description
3. Provide structure and examples
4. Update `prompts/README.md`

## Learning More

- **GitHub Copilot Docs**: https://docs.github.com/en/copilot
- **awesome-copilot Repository**: https://github.com/github/awesome-copilot
- **Agent Writing Guide**: https://github.com/github/awesome-copilot/blob/main/docs/AGENT_WRITING_GUIDE.md
- **Prompt Engineering**: https://github.com/github/awesome-copilot/blob/main/docs/PROMPT_ENGINEERING.md

## Contributing

To improve these assets:

1. **Suggest Improvements**: Open an issue with suggestions
2. **Submit Updates**: Create a PR to improve instructions or agents
3. **Add New Assets**: Contribute new agents, prompts, or instructions
4. **Share Feedback**: Let us know what works and what doesn't

## Version History

- **v1.0** (Initial Setup)
  - ✅ Code Reviewer agent (SE: Security)
  - ✅ Expert React Frontend Engineer agent
  - ✅ Expert Next.js Developer agent
  - ✅ TypeScript 5.x/ES2022 instructions
  - ✅ ReactJS development instructions
  - ✅ Docker & Containerization instructions
  - ✅ Create Implementation Plan prompt
  - ✅ Complete README documentation

## Next Steps

- [ ] Add Expert .NET Developer agent (pending awesome-copilot availability)
- [ ] Create Frontend Development collection
- [ ] Create Backend Development collection
- [ ] Create DevOps & Deployment collection
- [ ] Create Security & Code Review collection
- [ ] Add API design prompt
- [ ] Add component architecture prompt
- [ ] Add testing strategy prompt
- [ ] Add deployment checklist prompt

## Questions or Issues?

If you encounter any issues with agents, instructions, or prompts:

1. Check the relevant README (agents/, instructions/, prompts/, collections/)
2. Review the awesome-copilot documentation
3. Open an issue with details and examples
4. Reference the relevant asset files in your issue

---

**Last Updated**: Initial Setup
**Maintained By**: Development Team
**Based On**: [awesome-copilot](https://github.com/github/awesome-copilot) v1.0 standards
