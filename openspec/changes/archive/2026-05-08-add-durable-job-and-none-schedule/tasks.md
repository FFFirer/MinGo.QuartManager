## 1. Shared DTO — StoreDurable 属性

- [x] 1.1 QuartzOptionsDto 新增 `StoreDurable` bool 属性

## 2. Agent — JobConverter

- [x] 2.1 ConvertToDetail: 根据 `request.Options.StoreDurable` 调用 builder.StoreDurably(true)
- [x] 2.2 ConvertToTrigger: "none" 类型返回 null（不抛异常）

## 3. Agent — QuartzService

- [x] 3.1 CreateJobAsync: Schedule=None 时不创建 trigger，使用 `storeNonDurableWhileAwaitScheduling`
- [x] 3.2 GetScheduleType: trigger==null 时返回 "none"

## 4. 前端类型定义

- [x] 4.1 types/index.ts: ScheduleType 增加 'None'
- [x] 4.2 types/index.ts: QuartzOptionsDto 增加 storeDurable

## 5. 前端 CreateJobPage

- [x] 5.1 SCHEDULE_TYPES 增加 None 选项
- [x] 5.2 Options 区域增加"持久化 Job" checkbox
- [x] 5.3 handleSubmit 支持 Schedule=None 的请求构建
- [x] 5.4 Schedule=None 时隐藏 trigger 配置字段
