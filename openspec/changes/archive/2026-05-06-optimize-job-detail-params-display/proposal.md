## Why

Job 详情页的参数区域目前以纯 key:value 平铺方式展示，存在三个核心问题：
1. **无类型感知** — bool 显示为 "true"/"false" 字符串，JSON 对象渲染异常，日期无格式化
2. **未利用元数据** — manifest 已包含参数 label/type/required 等定义，但详情页未使用
3. **数据流不匹配** — API 返回的 params 是序列化 JSON 字符串，前端却当作对象处理

这导致用户在查看参数时需要"脑补"类型信息，体验欠佳。

## What Changes

- 为 Job 详情页参数区域引入类型感知渲染组件
- 接入 manifest 元数据，用 label 替代原始 key，标注类型/必填/默认值
- 修复 params 反序列化流程，确保前端拿到正确的对象数据
- 增加参数搜索/过滤和复制功能
- 提取参数渲染为独立组件，便于复用

## Capabilities

### New Capabilities
- `job-params-display`: Job 参数的类型感知展示，包含类型感知渲染、元数据映射、搜索过滤、复制功能

### Modified Capabilities
<!-- 无 — 此为纯前端实现优化，不改变现有 capability 的 spec-level 行为 -->

## Impact

- **前端文件**: `src/MinGo.Qap.UI/src/pages/JobDetailPage.tsx` — 参数区域重写
- **新增组件**: `src/MinGo.Qap.UI/src/components/JobParamsDisplay.tsx` — 参数渲染独立组件
- **类型文件**: `src/MinGo.Qap.UI/src/types/index.ts` — 可能修正 JobDetailDto/JobDefinitionDto 类型
- **API 层**: 不修改后端 API
- **后端**: 无影响
