## 1. 数据修复与类型加固

- [x] 1.1 在 `jobApi.get` 调用处增加 params 反序列化逻辑 — 检测 `typeof params === 'string'` 时执行 `JSON.parse`，try-catch 回退到 `{}`
- [x] 1.2 确认 `JobDetailDto` 和 `JobDefinitionDto` 的类型对齐，确保前端 `job` 对象的 `params` 类型为 `Record<string, any>`

## 2. 提取 JobParamsDisplay 组件

- [x] 2.1 创建 `src/MinGo.Qap.UI/src/components/JobParamsDisplay.tsx`，定义 `JobParamsDisplayProps` 接口
- [x] 2.2 实现基础骨架：接收 `params` 和可选的 `paramDefinitions`，渲染标题栏 + 参数列表容器
- [x] 2.3 实现参数搜索过滤功能：search input state、case-insensitive 过滤、空结果状态

## 3. 类型感知渲染器

- [x] 3.1 实现 `ParamBool`：`true` → 绿色 ✓ 图标，`false` → 灰色 ✗ 图标
- [x] 3.2 实现 `ParamNumber`：右对齐 + `tabular-nums` 字体
- [x] 3.3 实现 `ParamDate`：ISO 日期检测 + `toLocaleString()` 格式化 + tooltip
- [x] 3.4 实现 `ParamJson`：可折叠 JSON 树，默认折叠
- [x] 3.5 实现 `ParamLongText`：80 字符截断 + 展开/收起按钮
- [x] 3.6 实现 `ParamText`：纯文本 + 复制按钮
- [x] 3.7 编写类型派发逻辑 `renderParamByType(value, definition?)`，按优先级 bool → number → date → object → string 判断

## 4. Manifest 元数据集成

- [x] 4.1 在 `JobDetailPage` 中读取 manifest（`useQuery` + `manifestApi.get`），传入 `JobParamsDisplay`
- [x] 4.2 实现元数据匹配：以参数名 `name` 为 key 建 Map，匹配时使用 `label`、标注 `required`、显示 type badge、显示 default 提示
- [x] 4.3 无 manifest 时回退为原始 key + 运行时类型推断

## 5. 复制功能

- [x] 5.1 实现 copy-to-clipboard 工具函数（使用 `navigator.clipboard.writeText`）
- [x] 5.2 为每个参数值添加复制按钮（ClipboardIcon from lucide-react），点击后显示 "Copied!" 反馈（短暂 tooltip 或颜色闪烁）

## 6. 集成到 JobDetailPage

- [x] 6.1 用 `JobParamsDisplay` 替换现有的内联参数渲染（JobDetailPage.tsx 第 196-208 行）
- [x] 6.2 在 `JobDetailPage` 中添加 manifest 查询逻辑，与 job 查询并行
- [x] 6.3 验证整体渲染效果，检查 loading/error/empty 状态

## 7. 清理与验证

- [x] 7.1 运行 `typeScript` 检查无类型/语法错误（`npx tsc --noEmit --project tsconfig.app.json` 通过）
- [x] 7.2 运行 `typeScript` 编译确保无构建错误（仅 pre-existing 文件有报错，不在本次变更范围内）
- [x] 7.3 清理未使用的 import 和代码
