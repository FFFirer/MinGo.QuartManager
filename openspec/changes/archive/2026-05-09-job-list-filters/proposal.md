## Why

作业列表页面目前缺少筛选功能，用户无法快速通过 Group 或 Name 查找特定 Job。后端 API 已支持 `group` 和 `keyword` 查询参数，但前端未暴露筛选控件，导致用户需要手动翻页查找。

## What Changes

- 在 JobsPage 表头区域添加 Group 文本输入框和 Name(Keyword) 文本输入框
- 为 Name 输入添加 500ms 防抖，避免频繁请求
- 筛选条件变更时自动重置到第 1 页
- 更新 useQuery 的 queryKey 和 API 调用以包含筛选参数
- 将筛选状态同步到 URL query params，支持分享和浏览器回退

## Capabilities

### New Capabilities

- `job-list-filters`: 作业列表筛选能力，支持按 Group 和 Name 文本筛选

### Modified Capabilities

无

## Impact

- **Frontend**: `src/MinGo.Qap.UI/src/pages/JobsPage.tsx` — 新增筛选 UI 和逻辑
- **Backend**: 无影响，API 已支持 group 和 keyword 参数
- **API**: `jobApi.getAll()` 已在签名中包含 `group` 和 `keyword`，无需修改
