# Development Instructions

This directory contains development guidelines and best practices for working on this project.

## Available Instructions

### TypeScript Development (`typescript-5-es2022.instructions.md`)

Guidelines for writing TypeScript code targeting TypeScript 5.x and ES2022. Covers:

- Project organization and naming conventions
- Type system best practices (avoiding `any`, discriminated unions)
- Async/await, error handling, and event patterns
- Security practices and input validation
- Testing expectations with project frameworks
- Performance optimization and resource management
- Documentation and JSDoc standards

**When to Use**: Writing any TypeScript code in the project
**Key Principles**: Readable solutions, respect existing architecture, maintainability

---

### ReactJS Development (`reactjs.instructions.md`)

Best practices for building React applications with modern patterns. Covers:

- Functional components and hooks as default
- Component design and composition patterns
- State management (useState, useReducer, useContext, etc.)
- Hooks and effects with proper dependency arrays
- Styling approaches (CSS Modules, CSS-in-JS)
- Performance optimization (React.memo, code splitting, lazy loading)
- Data fetching with React Query/SWR patterns
- Error handling with Error Boundaries
- Form handling and validation
- Accessibility compliance (WCAG 2.1 AA)
- Testing with React Testing Library/Jest
- Security practices for web applications

**When to Use**: Building any React component or feature
**Framework Stack**: React 19+, TypeScript, Vite
**Key Principles**: Single responsibility, composition, reusability, accessibility

---

### Docker & Containerization Best Practices (`containerization-docker-best-practices.instructions.md`)

Comprehensive guide for creating efficient, secure Docker images. Covers:

- Core containerization principles (immutability, portability, isolation, efficiency)
- Multi-stage builds and image optimization
- Dockerfile best practices and layer optimization
- Using `.dockerignore` effectively
- Security practices (non-root user, minimal images, scanning)
- Health checks and monitoring
- Container runtime management
- Troubleshooting common issues
- Performance optimization and resource limits

**When to Use**: Creating or modifying Dockerfiles, docker-compose configurations
**Key Principles**: Security, efficiency, immutability, reproducibility

---

## How to Apply Instructions

### In Development

1. **Reference During Coding**: Keep relevant instructions open while implementing features
2. **Use with Copilot**: Include instruction files in context when asking for help:

   ```
   I'm building a React component. Reference the ReactJS instructions.
   ```

3. **Code Review**: Use instructions as a checklist when reviewing PRs:
   - Does the TypeScript follow the guidelines?
   - Are React components following best practices?
   - Do Dockerfiles follow security standards?

### With GitHub Copilot

Tag instructions in your requests:

```
@assistant Create a React component that follows our reactjs.instructions.md guidelines
```

Or reference them in chat:

```
According to our TypeScript instructions, what's the best way to handle this error?
```

## Contributing to Instructions

To improve or add new instructions:

1. Update the relevant `.instructions.md` file
2. Keep guidelines practical and implementation-focused
3. Include specific examples and anti-patterns
4. Link to related instructions and agents
5. Update this README with any new additions

## Related Resources

- **Agents**: See `.github/agents/README.md` for specialized AI agents
- **Prompts**: See `.github/prompts/README.md` for reusable prompt templates
- **Collections**: See `.github/collections/` for grouped sets of related assets

## Project-Specific Deviations

While these instructions provide general best practices, always follow:

1. Existing code patterns in the project
2. Team conventions and decisions
3. Framework-specific requirements
4. Project-specific performance constraints

When in doubt about how to apply instructions to a specific situation, discuss with the team or reference the project's existing codebase.
