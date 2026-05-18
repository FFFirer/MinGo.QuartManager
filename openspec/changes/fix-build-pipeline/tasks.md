## 1. Fix Dockerfile path in build.yml

- [ ] 1.1 Change `file` parameter in `.gitea/workflows/build.yml` from `src/MinGo.Qap.Platform/Dockerfile` to `./Dockerfile`

## 2. Replace Node.js base image with domestic mirror

- [ ] 2.1 Change `FROM node:22-alpine` to `FROM swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/library/node:20-alpine` in `Dockerfile`

## 3. Verify changes

- [ ] 3.1 Run `lsp_diagnostics` on changed files
- [ ] 3.2 Confirm no syntax errors in YAML and Dockerfile
