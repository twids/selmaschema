---
name: "Create Implementation Plan"
description: "Create a comprehensive, machine-readable implementation plan for complex features or refactoring"
---

# Create Implementation Plan

You are tasked with creating a comprehensive, deterministic implementation plan for a feature, bug fix, or refactoring task in a software project.

## Your Mission

Create a detailed, machine-readable implementation plan that can be executed by an AI or a development team. The plan should break down the work into discrete, actionable phases with clear completion criteria, dependencies, and file-level changes.

## Plan Structure

Your implementation plan must follow this structure:

1. **Status**: Current state (planning, in-progress, completed, blocked)
2. **Introduction**: Brief overview of what's being implemented
3. **Requirements**: What needs to be accomplished (user-facing and technical)
4. **Implementation Steps**: Ordered phases with clear deliverables
5. **Files to Create/Modify**: Exact file paths and nature of changes
6. **Testing Strategy**: How to validate the implementation
7. **Alternatives Considered**: Other approaches and why they were rejected
8. **Dependencies**: External libraries, services, or team dependencies
9. **Assumptions & Risks**: What could go wrong and how to mitigate

## Phase Architecture

Each implementation phase should include:

- **Phase Name**: Clear, actionable title
- **Description**: What this phase accomplishes
- **Deliverables**: Concrete outputs (code, configs, tests)
- **Completion Criteria**: Measurable ways to know it's done
- **Parallel Tasks**: Work that can happen simultaneously
- **Sequential Tasks**: Work that must happen in order
- **Estimated Effort**: Time/complexity assessment

## AI-Optimized Standards

- Use **explicit, unambiguous language** — avoid vague descriptions
- Structure steps as **discrete, atomic operations** — each should be completable without ambiguity
- Include **code examples or pseudo-code** where clarification helps
- Specify **exact file paths** and operations (create, modify, delete)
- Use **machine-parseable formats** where applicable (YAML, JSON for metadata)
- Clearly state **success criteria** for each phase so progress is measurable
- Identify **blocking dependencies** so phases can run in parallel safely

## Output Specifications

Your implementation plan should be formatted as a markdown document with the following structure:

```markdown
---
status: "planning|in-progress|completed|blocked"
effort: "small|medium|large"
priority: "critical|high|medium|low"
---

# [Feature/Fix Name] - Implementation Plan

## Overview

Brief summary of what's being implemented

## Requirements

- **User-Facing**: What end-users will experience
- **Technical**: Architecture, performance, security requirements

## Implementation Phases

### Phase 1: [Name]

- **Goal**: Clear objective
- **Deliverables**: What's created/changed
- **Completion Criteria**: How to know it's done
- **Files**: Exact paths
- **Effort**: Time/complexity

### Phase 2: [Name]

...

## Testing Strategy

- Unit tests: Specific test cases
- Integration tests: System-level validation
- Manual testing: User flows to verify

## Deployment Strategy

- Pre-deployment checks
- Rollout plan
- Rollback procedure
- Monitoring/alerting

## Known Issues & Mitigations

- Potential problems
- How to detect them
- Mitigation strategies
```

## Example Application

For a feature like "Add user authentication":

```markdown
---
status: "planning"
effort: "large"
priority: "high"
---

# User Authentication - Implementation Plan

## Overview

Implement JWT-based authentication for API and frontend session management.

## Implementation Phases

### Phase 1: Backend Auth Service

- Implement JWT token generation and validation
- Create password hashing and verification
- Add auth middleware to API endpoints

### Phase 2: API Endpoints

- POST /auth/login - user login
- POST /auth/logout - user logout
- GET /auth/me - current user info

### Phase 3: Frontend Auth

- Create auth context and hooks
- Add login/logout UI components
- Implement protected route components
```

## Key Guidelines

- **Break work into phases** that can be reviewed and merged independently
- **Include specific file paths** for every file that will be created or modified
- **State success criteria** for each phase so progress is objectively measurable
- **Identify dependencies** between phases so work can be parallelized
- **Plan testing from the start** — include test requirements in each phase
- **Consider security implications** and include security checks in testing
- **Plan deployment carefully** — include migration strategies, rollback plans
- **Document assumptions** — what must be true for this plan to work
- **Identify risks** — what could derail the implementation and how to mitigate
