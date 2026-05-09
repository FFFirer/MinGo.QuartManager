## 1. Shared — TriggerSummaryDto + DTO 更新

- [x] 1.1 新建 `MinGo.Qap.Shared/Models/TriggerDtos.cs` — 包含 `TriggerSummaryDto`
- [x] 1.2 更新 `JobDetailDto`：添加 `List<TriggerSummaryDto> Triggers { get; set; }`
- [x] 1.3 更新 `JobDefinitionDto`：添加 `List<TriggerSummaryDto>? Triggers { get; set; }`

## 2. Agent — QuartzService 返回全部 triggers

- [x] 2.1 `QuartzService.GetJobAsync()`：迭代所有 triggers，每个附带 state，填充到 `Triggers` 列表

## 3. Platform — JobService 映射 triggers

- [x] 3.1 `JobService.GetAsync()`：从 Agent `JobDetailDto` 映射 triggers 到 `JobDefinitionDto`

## 4. 前端 — Types + UI

- [x] 4.1 `types/index.ts`：新增 `TriggerSummaryDto` 接口，更新 `JobDetailDto` / `JobDefinitionDto`
- [x] 4.2 `JobDetailPage.tsx`：添加 Triggers 展示区域，解析并渲染 trigger 列表

## 5. 验证

- [x] 5.1 `dotnet build` 后端编译通过
- [x] 5.2 LSP diagnostics 无错误
- [x] 5.3 前端 `pnpm build` 编译通过
