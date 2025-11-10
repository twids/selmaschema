# DevContainer Dockerfile for Co-Parenting Calendar
# Includes .NET 8 SDK, Node.js 20, Docker CLI, and pnpm

FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install Node.js 20.x
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && npm install -g pnpm

# Install Docker CLI
RUN apt-get update \
    && apt-get install -y \
        apt-transport-https \
        ca-certificates \
        curl \
        gnupg \
        lsb-release \
    && curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null \
    && apt-get update \
    && apt-get install -y docker-ce-cli docker-compose-plugin

# Install additional development tools
RUN apt-get install -y \
    git \
    vim \
    nano \
    less \
    jq \
    wget \
    unzip \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Set up non-root user (vscode user is created by the base image)
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Create vscode user if it doesn't exist
RUN if ! id -u ${USERNAME} > /dev/null 2>&1; then \
        groupadd --gid $USER_GID $USERNAME \
        && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
        && apt-get update \
        && apt-get install -y sudo \
        && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
        && chmod 0440 /etc/sudoers.d/$USERNAME; \
    fi

# Install common .NET tools
RUN dotnet tool install --global dotnet-ef \
    && dotnet tool install --global dotnet-format

# Add .NET tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Set working directory
WORKDIR /workspace

# Default command
CMD ["/bin/bash"]
