## 1. 准备工作

- [x] 1.1 安装 pnpm (如未安装): `npm install -g pnpm`
- [x] 1.2 删除现有的 node_modules 目录和 package-lock.json 文件

## 2. 迁移配置

- [x] 2.1 在项目根目录创建 .npmrc 配置文件 (可选，用于 pnpm 配置)
- [x] 2.2 验证 package.json 中的 scripts 配置无需修改

## 3. 安装依赖

- [x] 3.1 执行 `pnpm install` 安装依赖
- [x] 3.2 验证 pnpm-lock.yaml 文件已生成

## 4. 验证功能

- [x] 4.1 验证 `pnpm dev` 可以启动开发服务器
- [x] 4.2 验证 `pnpm build` 可以成功构建
- [x] 4.3 验证 `pnpm lint` 可以运行代码检查
- [x] 4.4 验证 `pnpm preview` 可以预览构建产物

## 5. 更新文档

- [x] 5.1 更新 README.md 中的包管理命令说明
- [x] 5.2 确保开发团队知道需要使用 pnpm