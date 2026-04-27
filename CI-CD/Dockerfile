# Combined Dockerfile for Backend API and Frontend UI
# For Render deployment - runs both services in one container

# ============================================
# STAGE 1: Build Backend API
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy backend project files
COPY CateringEcommerce.API/CateringEcommerce.API.csproj ./CateringEcommerce.API/
COPY CateringEcommerce.BAL/CateringEcommerce.BAL.csproj ./CateringEcommerce.BAL/
COPY CateringEcommerce.Domain/CateringEcommerce.Domain.csproj ./CateringEcommerce.Domain/

# Restore backend dependencies
RUN dotnet restore CateringEcommerce.API/CateringEcommerce.API.csproj

# Copy all source files
COPY . .

# Publish backend
RUN dotnet publish CateringEcommerce.API/CateringEcommerce.API.csproj -c Release -o /app/backend

# ============================================
# STAGE 2: Build Frontend UI
# ============================================
FROM node:18 AS frontend-build
WORKDIR /app

# Copy frontend package files
COPY CateringEcommerce.Web/package.json ./CateringEcommerce.Web/
COPY CateringEcommerce.Web/package-lock.json ./CateringEcommerce.Web/

WORKDIR /app/CateringEcommerce.Web
RUN npm install

# Copy frontend source
COPY CateringEcommerce.Web/ ./

# Build frontend
RUN npm run build

# ============================================
# STAGE 3: Final Runtime Image
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install Nginx and supervisor for managing multiple processes
RUN apt-get update && apt-get install -y \
    nginx \
    supervisor \
    && rm -rf /var/lib/apt/lists/*

# Create directories
RUN mkdir -p /app/backend /app/frontend /var/log/supervisor

# Copy backend files
COPY --from=backend-build /app/backend /app/backend


# Copy frontend build files directly to Nginx's default directory
RUN rm -rf /var/www/html/*
COPY --from=frontend-build /app/CateringEcommerce.Web/dist /var/www/html

# Copy supervisor configuration
RUN echo '[supervisord]' > /etc/supervisor/conf.d/supervisord.conf && \
    echo 'nodaemon=true' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo '' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo '[program:dotnet]' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'command=dotnet CateringEcommerce.API.dll' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'directory=/app/backend' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'autostart=true' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'autorestart=true' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'stderr_logfile=/var/log/supervisor/dotnet.err.log' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'stdout_logfile=/var/log/supervisor/dotnet.out.log' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo '' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo '[program:nginx]' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'command=/usr/sbin/nginx -g "daemon off;"' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'autostart=true' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'autorestart=true' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'stderr_logfile=/var/log/supervisor/nginx.err.log' >> /etc/supervisor/conf.d/supervisord.conf && \
    echo 'stdout_logfile=/var/log/supervisor/nginx.out.log' >> /etc/supervisor/conf.d/supervisord.conf

# Expose ports (Backend: 8080, Nginx: 80)
EXPOSE 80 8080

# Create secure file storage directory
RUN mkdir -p /app/ProjectFiles

# Start supervisor (which manages both .NET and Nginx)
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]