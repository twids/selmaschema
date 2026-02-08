FROM debian:bookworm-slim
WORKDIR /app

# Copy EF Core migrations bundle (self-contained linux-x64 binary)
COPY efbundle ./efbundle
RUN chmod +x ./efbundle

# Default entrypoint runs EF bundle; pass --connection via command
ENTRYPOINT ["/app/efbundle"]