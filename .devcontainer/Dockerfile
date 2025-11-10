# DevContainer Dockerfile for Co-Parenting Calendar
# Includes .NET 8 SDK, Node.js 20, Docker CLI, and pnpm

FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install Node.js 20.x
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && npm install -g pnpm

# Install additional development tools
RUN apt-get update && apt-get install -y \
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

# Install common .NET tools as vscode user
USER ${USERNAME}
RUN dotnet tool install --global dotnet-ef \
    && dotnet tool install --global dotnet-format
# Add .NET tools to PATH for vscode user
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"
USER root
# Set working directory
WORKDIR /workspace

# Default command
CMD ["/bin/bash"]
