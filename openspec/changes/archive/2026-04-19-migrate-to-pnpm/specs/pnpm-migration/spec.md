## ADDED Requirements

### Requirement: 前端项目使用 pnpm 作为包管理器
前端项目必须使用 pnpm 作为包管理工具，替代现有的 npm。所有依赖安装命令必须通过 pnpm 执行。

#### Scenario: 使用 pnpm 安装依赖
- **WHEN** 开发者执行 `pnpm install` 命令
- **THEN** pnpm 解析 package.json 中的依赖并安装到 node_modules

#### Scenario: 添加新依赖
- **WHEN** 开发者执行 `pnpm add <package>` 命令
- **THEN** pnpm 将包添加到 package.json 并安装到 node_modules

#### Scenario: 安装开发依赖
- **WHEN** 开发者执行 `pnpm add -D <package>` 命令
- **THEN** pnpm 将包添加到 package.json 的 devDependencies

### Requirement: 现有 npm scripts 保持兼容
package.json 中定义的 scripts 必须继续正常工作，无需修改。

#### Scenario: 运行开发服务器
- **WHEN** 开发者执行 `pnpm dev` 命令
- **THEN** Vite 开发服务器正常启动

#### Scenario: 构建生产版本
- **WHEN** 开发者执行 `pnpm build` 命令
- **THEN** TypeScript 编译通过且 Vite 构建成功生成产物

#### Scenario: 运行代码检查
- **WHEN** 开发者执行 `pnpm lint` 命令
- **THEN** ESLint 检查通过

#### Scenario: 预览构建产物
- **WHEN** 开发者执行 `pnpm preview` 命令
- **THEN** Vite 预览服务器启动并展示构建产物

### Requirement: pnpm-lock.yaml 版本锁定
pnpm 必须生成 pnpm-lock.yaml 文件以锁定依赖版本，确保团队成员和 CI 环境使用相同的依赖版本。

#### Scenario: 首次安装依赖
- **WHEN** 项目中不存在 pnpm-lock.yaml 时执行 `pnpm install`
- **THEN** pnpm 生成 pnpm-lock.yaml 文件

#### Scenario: 团队成员同步依赖
- **WHEN** 团队成员拉取代码后执行 `pnpm install`
- **THEN** 根据 pnpm-lock.yaml 安装与其他人相同的依赖版本