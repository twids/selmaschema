# GitHub Copilot Agents

This directory contains specialized AI agents for GitHub Copilot that provide expert guidance in specific domains.

## Available Agents

### Code Reviewer (SE: Security)

**File**: `code-reviewer.agent.md`

A security-focused code review specialist trained in OWASP Top 10, Zero Trust architecture, and reliability patterns. Use this agent for:

- Security vulnerability detection and fixes
- OWASP compliance verification
- Safe coding patterns and practices
- Sensitive code review and approval

**Usage**: `@agent-code-reviewer` or use in pull request reviews

---

### Expert React Frontend Engineer

**File**: `expert-react-frontend-engineer.agent.md`

Specialized in React 19.2+ development with modern hooks, Server Components, and advanced patterns. Use for:

- React component architecture and design patterns
- Modern hooks: `use()`, `useFormStatus`, `useOptimistic`, `useActionState`
- Server Components and RSC patterns
- Performance optimization and accessibility
- React Compiler awareness and concurrent rendering

**Tech Stack**: React 19.2, TypeScript, functional components
**Expertise Areas**: Hooks, Server Components, Actions, Performance, Testing, Accessibility, Form Handling
**Testing Frameworks**: Jest, React Testing Library, Vitest, Playwright

**Usage**: `@agent-expert-react-frontend-engineer`

---

### Expert Next.js Developer

**File**: `expert-nextjs-developer.agent.md`

Specialized in Next.js 16+ with App Router, Cache Components, Turbopack, and Server Components. Use for:

- Next.js App Router architecture and file-based routing
- Cache Components with `use cache` and Partial Pre-Rendering (PPR)
- Turbopack bundler optimization and configuration
- React Compiler awareness and concurrent rendering
- Advanced data fetching and caching strategies
- Server Components and Client Components patterns
- Metadata API and SEO optimization
- Deployment and production patterns

**Tech Stack**: Next.js 16+, React 19.2, TypeScript, App Router
**Breaking Changes v16**: `params` and `searchParams` are now async
**Expertise Areas**: App Router, Cache Components, Turbopack, Server/Client Components, Data Fetching, RSC, Performance, Deployment
**Testing Frameworks**: Jest, Playwright, React Testing Library

**Usage**: `@agent-expert-nextjs-developer`

---

## How to Use Agents in Copilot

### In VS Code

```
Type @ to open the agent picker and select your desired agent:
@agent-code-reviewer
@agent-expert-react-frontend-engineer
@agent-expert-nextjs-developer
```

### In Pull Request Reviews

Tag agents with their description in code review comments:

```
@agent-code-reviewer please review this for security vulnerabilities
```

### With Chat

```
@agent-expert-react-frontend-engineer How should I structure this component?
@agent-expert-nextjs-developer What's the best way to fetch data in this route?
```

## Adding New Agents

To add a new agent:

1. Create a new `.agent.md` file in this directory
2. Include agent name, description, and expertise areas in frontmatter
3. Document the agent's specialization and usage examples
4. Add an entry to this README with usage instructions

## Agent Specifications

Each agent file should contain:

- **Name**: Agent identifier and display name
- **Description**: What the agent specializes in
- **Expertise Areas**: Detailed list of skills and knowledge
- **Approach**: How the agent tackles problems
- **Guidelines**: Best practices and coding standards
- **Common Scenarios**: Examples of typical use cases
- **Response Style**: How responses should be formatted
- **Advanced Capabilities**: Complex features the agent can handle
- **Code Examples**: Representative examples for the domain

For details, see the awesome-copilot documentation:
https://github.com/github/awesome-copilot
