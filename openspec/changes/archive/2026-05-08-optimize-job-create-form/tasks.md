## 1. Job Identity: Name/Group 顺序交换 + Name 正则校验

- [x] 1.1 交换 `CreateJobPage.tsx` Job Identity section 中 Group 和 Name 的 grid 列顺序（Name 左列、Group 右列）
- [x] 1.2 在 `validate()` 中添加 Name 正则校验 `/^[a-zA-Z0-9\-_]+$/`，非法字符时设置错误消息 "Job name只能包含字母、数字、-和_"
- [x] 1.3 保留原有的空 Name 校验和 "." 字符校验，整合到新正则逻辑中

## 2. Parameters 默认值预填

- [x] 2.1 修改 `handleJobTypeChange`：选择 JobType 时遍历 `selectedJob.parameters`，将参数项的 `default` 值（非 undefined）写入 `params[name] = param.default`
- [x] 2.2 确认 copyFrom 场景优先级：copySource 的 params 在 handleJobTypeChange 之后执行，覆盖默认值

## 3. storeNonDurableWhileAwaitingScheduling 条件化

- [x] 3.1 修改 `QuartzService.CreateJobAsync` 中 Schedule=None 分支，根据 `request.Options?.StoreDurable` 值条件传递 `storeNonDurableWhileAwaitingScheduling` 参数

## 4. Specs 同步

- [x] 4.1 更新 `openspec/specs/job-create-form/spec.md`：应用 delta spec 中的 MODIFIED Requirements 到主 spec
- [x] 4.2 更新 `openspec/specs/none-schedule-type/spec.md`：应用 delta spec 中的 MODIFIED Requirements 到主 spec
