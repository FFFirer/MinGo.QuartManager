## 1. Configure Vite Dev Server Proxy

- [x] 1.1 Update `vite.config.ts` to add `server.proxy` configuration for `/api` paths
- [x] 1.2 Set proxy target to `http://localhost:5256`
- [x] 1.3 Configure proxy to change origin (handle host header)
- [x] 1.4 Verify proxy configuration is only active in dev mode (not in build/preview)

## 2. Update Frontend API Client

- [x] 2.1 Update `src/api/index.ts` to use relative path `/api` as baseURL
- [x] 2.2 Keep `VITE_API_URL` environment variable as fallback
- [ ] 2.3 Test API client with proxy (requests to `/api/*` should be proxied)

## 3. Verification

- [ ] 3.1 Start backend server on port 5256
- [ ] 3.2 Start frontend dev server with `npm run dev`
- [ ] 3.3 Verify API requests are proxied correctly (check network tab)
- [ ] 3.4 Verify Authorization headers are preserved
- [ ] 3.5 Verify HMR works without breaking proxy
- [ ] 3.6 Verify production build (`npm run build`) does not include proxy config
