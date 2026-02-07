---
description: "Comprehensive best practices for creating optimized, secure, and efficient Docker images and managing containers"
applyTo: "**/Dockerfile,**/Dockerfile.*,**/*.dockerfile,**/docker-compose*.yml,**/docker-compose*.yaml,**/compose*.yml,**/compose*.yaml"
---

# Containerization & Docker Best Practices

Your mission is to guide developers in building highly efficient, secure, and maintainable Docker images and managing containers effectively. Emphasize optimization, security, and reproducibility.

## Core Principles of Containerization

### 1. Immutability

- Containers should run consistently across different environments without modification
- Treat container images like code - version them, tag them meaningfully
- Immutable images enable instant rollbacks by simply switching to a previous image tag
- Advocate for creating new images for every code change

### 2. Portability

- Containers should run consistently across different environments
- Design Dockerfiles that are self-contained and avoid environment-specific configurations
- Use environment variables for runtime configuration, with sensible defaults
- Consider the target deployment platforms and ensure compatibility

### 3. Isolation

- Each container runs in its own process namespace
- Containers have isolated CPU, memory, and I/O resources
- Recommend running a single process per container
- Use container networking for inter-container communication

### 4. Efficiency & Small Images

- Smaller images are faster to build, push, pull, and consume fewer resources
- Prioritize techniques for reducing image size and build time
- Advise against including unnecessary tools in production images
- Use multi-stage builds and minimal base images as the default approach

## Dockerfile Best Practices

### 1. Multi-Stage Builds (The Golden Rule)

- Always recommend multi-stage builds for compiled languages and even for Node.js/Python
- Name build stages descriptively (e.g., `AS build`, `AS test`, `AS production`)
- Copy only the necessary artifacts between stages to minimize final image size
- Use different base images for build and runtime stages when appropriate

### 2. Choose the Right Base Image

- Prefer official images from Docker Hub or cloud providers
- Use minimal variants (`alpine`, `slim`, `distroless`) when possible
- Avoid `latest` tag in production; use specific version tags for reproducibility
- Recommend regularly updating base images for security patches

### 3. Optimize Image Layers

- Place frequently changing instructions (e.g., `COPY . .`) after less frequently changing ones
- Combine `RUN` commands where possible to minimize layers
- Clean up temporary files in the same `RUN` command
- Use multi-line commands with `\` for complex operations

### 4. Use `.dockerignore` Effectively

- Always create and maintain a comprehensive `.dockerignore` file
- Common exclusions: `.git`, `node_modules`, build artifacts, documentation, test files
- Exclude development-only files that aren't needed in production
- Review `.dockerignore` regularly as the project evolves

### 5. Minimize `COPY` Instructions

- Copy dependency files (like `package.json`, `requirements.txt`) before copying source code
- Copy specific files rather than entire project directories when possible
- Copy files that change together in the same instruction
- Only copy files that are actually needed for the build or runtime

### 6. Define Default User and Port

- Use `USER <non-root-user>` to run the application process as a non-root user
- Use `EXPOSE` to document the port the application listens on
- Create a dedicated user in the Dockerfile rather than using an existing one
- Ensure proper file permissions for the non-root user

### 7. Use `CMD` and `ENTRYPOINT` Correctly

- Use `ENTRYPOINT` for the executable and `CMD` for arguments
- Prefer exec form over shell form for better process management
- Consider using shell scripts as entrypoints for complex startup logic
- `ENTRYPOINT` makes the image behave like an executable, while `CMD` provides default arguments

### 8. Environment Variables for Configuration

- Avoid hardcoding configuration inside the image
- Use `ENV` for default values, but allow overriding at runtime
- Recommend using environment variable validation in application startup code
- Never hardcode secrets; load them from secure sources

## Container Security Best Practices

### 1. Non-Root User

- Always define a non-root `USER` in the Dockerfile
- Create a dedicated user for your application
- Ensure the non-root user has the minimum necessary permissions
- Use `USER` directive early in the Dockerfile

### 2. Minimal Base Images

- Prioritize `alpine`, `slim`, or `distroless` images
- Review base image vulnerabilities regularly using security scanning tools
- Smaller images have a reduced attack surface
- Stay updated with the latest minimal base image versions

### 3. Security Scanning

- Integrate tools like `hadolint` (for Dockerfile linting) into your CI pipeline
- Use `Trivy`, `Clair`, or `Snyk Container` for image vulnerability scanning
- Recommend failing builds if critical vulnerabilities are found
- Advise on regular scanning of images in registries

### 4. No Sensitive Data in Image Layers

- Never include secrets, private keys, or credentials in image layers
- Use secrets management solutions for runtime injection
- Recommend scanning images for accidentally included secrets
- Use multi-stage builds to avoid including build-time secrets in final image

### 5. Health Checks

- Define `HEALTHCHECK` instructions in Dockerfiles (critical for orchestration)
- Design health checks that are specific to your application
- Use appropriate intervals and timeouts for health checks
- Consider implementing both liveness and readiness checks

## Container Runtime & Orchestration

### 1. Resource Limits

- Always set `cpu_limits`, `memory_limits` in Docker Compose or Kubernetes
- Suggest monitoring resource usage to tune limits appropriately
- Set both requests and limits for predictable resource allocation
- Use resource quotas in Kubernetes to manage cluster-wide resource usage

### 2. Logging & Monitoring

- Use standard logging output (`STDOUT`/`STDERR`) for container logs
- Integrate with log aggregators (Fluentd, Logstash, Loki)
- Recommend implementing structured logging in applications
- Set up log rotation and retention policies

### 3. Persistent Storage

- Use Docker Volumes or Kubernetes Persistent Volumes for persistent data
- Never store persistent data inside the container's writable layer
- Implement backup and disaster recovery procedures
- Consider using cloud-native storage solutions

### 4. Networking

- Create separate networks for different application tiers
- Use service discovery mechanisms provided by your orchestration platform
- Define network policies to control inter-container communication
- Use proper network segmentation for multi-tier applications

### 5. Orchestration

- Recommend Kubernetes for complex, large-scale deployments
- Leverage orchestrator features for scaling, self-healing, and service discovery
- Use rolling update strategies for zero-downtime deployments
- Implement proper resource management and monitoring

## Troubleshooting

### Large Image Size

- Review layers for unnecessary files using `docker history <image>`
- Implement multi-stage builds
- Use a smaller base image
- Optimize `RUN` commands and clean up temporary files

### Slow Builds

- Leverage build cache by ordering instructions from least to most frequent change
- Use `.dockerignore` to exclude irrelevant files
- Use `docker build --no-cache` for troubleshooting

### Container Not Starting/Crashing

- Check `CMD` and `ENTRYPOINT` instructions
- Review container logs (`docker logs <container_id>`)
- Ensure all dependencies are present in final image
- Check resource limits

### Permissions Issues

- Verify file/directory permissions in the image
- Ensure the `USER` has necessary permissions
- Check mounted volumes permissions

### Network Connectivity Issues

- Verify exposed ports (`EXPOSE`) and published ports (`-p`)
- Check container network configuration
- Review firewall rules
