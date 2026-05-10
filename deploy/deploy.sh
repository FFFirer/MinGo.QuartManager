#!/bin/bash
# =============================================================================
# MinGo.QuartManager 部署脚本
# 通过 SSH 在远程 Debian 12 服务器上执行 docker compose 更新
#
# 用法:
#   ./deploy.sh                          # 使用当前 .env 中的 IMAGE_TAG
#   ./deploy.sh v1.2.3                   # 部署指定版本
#   ./deploy.sh latest                   # 部署 latest 标签
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="/opt/quartmanager"
SSH_HOST="${DEPLOY_HOST:-}"
SSH_USER="${DEPLOY_USER:-root}"
SSH_KEY="${DEPLOY_KEY:-}"

# 如果提供了版本参数，更新 IMAGE_TAG
if [ $# -ge 1 ]; then
    export IMAGE_TAG="$1"
    echo "Deploying version: ${IMAGE_TAG}"
fi

# 检查必要的环境变量
if [ -z "${SSH_HOST}" ]; then
    if [ -f "${SCRIPT_DIR}/.env" ]; then
        echo "Using .env file for local deployment..."
        cd "${SCRIPT_DIR}"
        docker compose pull
        docker compose up -d --remove-orphans
        docker image prune -f
        echo "Deployment completed successfully."
        exit 0
    else
        echo "ERROR: DEPLOY_HOST not set and no local .env found."
        echo "Usage: DEPLOY_HOST=server.example.com ./deploy.sh [version]"
        exit 1
    fi
fi

# SSH 部署
echo "Deploying to ${SSH_HOST}..."

# 将 docker-compose.yml 和 .env 同步到远程服务器
rsync -avz -e "ssh${SSH_KEY:+ -i ${SSH_KEY}}" \
    "${SCRIPT_DIR}/docker-compose.yml" \
    "${SCRIPT_DIR}/.env" \
    "${SSH_USER}@${SSH_HOST}:${DEPLOY_DIR}/"

# 同步 nginx 配置
rsync -avz -e "ssh${SSH_KEY:+ -i ${SSH_KEY}}" \
    "${SCRIPT_DIR}/nginx/" \
    "${SSH_USER}@${SSH_HOST}:${DEPLOY_DIR}/nginx/"

# 远程执行部署
ssh "${SSH_KEY:+-i ${SSH_KEY}}" "${SSH_USER}@${SSH_HOST}" "
    set -e
    cd ${DEPLOY_DIR}
    echo 'Pulling latest images...'
    docker compose pull
    echo 'Recreating services...'
    docker compose up -d --remove-orphans
    echo 'Cleaning up old images...'
    docker image prune -f
    echo 'Deployment completed successfully.'
"
