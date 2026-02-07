# DevContainer Dockerfile for Co-Parenting Calendar
# Includes .NET 8 SDK, Node.js 20, Docker CLI, and pnpm

FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy

# Install prerequisites and Node.js 20.x
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    ca-certificates \
    gnupg \
    git \
    vim \
    nano \
    less \
    jq \
    wget \
    unzip \
    build-essential \
    python3 \
    sudo \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && npm install -g pnpm \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Set up non-root user (vscode user)
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Create vscode user if it doesn't exist
RUN if ! id -u ${USERNAME} > /dev/null 2>&1; then \
    groupadd --gid $USER_GID $USERNAME 2>/dev/null || true \
    && useradd --uid $USER_UID --gid $USER_GID -m -s /bin/bash $USERNAME \
    && echo "$USERNAME ALL=(root) NOPASSWD:ALL" > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME; \
    fi

# Add .NET environment settings and tools path
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    PATH="${PATH}:/home/vscode/.dotnet/tools"

# Set working directory
WORKDIR /workspace

# Default command
CMD ["/bin/bash"]
