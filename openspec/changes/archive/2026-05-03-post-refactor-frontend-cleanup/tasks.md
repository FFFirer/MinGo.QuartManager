## 1. CalendarPage — 移除 ClusterTabs 依赖并迁移到 schedulerName

- [x] 1.1 删除 CalendarPage.tsx 中 `import ClusterTabs from '../components/ClusterTabs'` 语句
- [x] 1.2 将 CalendarPage 的 clusterId 替换为 schedulerName：useParams 改为从路由 /schedulers/:schedulerName/calendar 获取 schedulerName
- [x] 1.3 替换 CalendarPage 中 ClusterTabs 组件的渲染为自己的 header 区域（显示 schedulerName + StatusBadge）
- [x] 1.4 将 API 调用从 `/api/clusters/{clusterId}/calendar` 改为 `/api/schedulers/{schedulerName}/calendar`
- [x] 1.5 移除不再需要的 `useCluster` hook 调用
- [x] 1.6 修复 context menu 中的导航路径（/clusters/ → /schedulers/）
- [x] 1.7 确认 App.tsx 中 CalendarPage 的路由已正确映射到 /schedulers/:schedulerName/calendar

## 2. CreateJobModal — props 从 clusterId 改为 schedulerName

- [x] 2.1 修改 CreateJobModalProps 接口，将 `clusterId: string` 替换为 `schedulerName: string`
- [x] 2.2 内部逻辑中所有使用 clusterId 的地方改为使用 schedulerName
- [x] 2.3 移除对已删除的 `useCluster`、`useCreateJob`、`useManifest` hooks 的依赖，改用直接 API 调用（jobApi, manifestApi）
- [x] 2.4 确认 JobsPage 的调用处传参正确（目前已传 schedulerName，只需确认接口匹配）

## 3. PlatformDashboardPage — 清理 cluster 引用

- [x] 3.1 修改 DashboardData 接口中的 clusters 数组为 schedulers，字段对应调整（id→name, name→schedulerName 等）
- [x] 3.2 将 cluster 卡片的导航路径从 `/clusters/{id}` 改为 `/schedulers/{name}`
- [x] 3.3 确保 "View All" 链接指向 /schedulers 而非 /clusters
- [x] 3.4 更新 job 条目中的 clusterName 字段为 schedulerName
- [x] 3.5 确认 `/api/dashboard` 端点返回数据模型与更新后的接口匹配

## 4. UpcomingJobsList — 数据模型更新

- [x] 4.1 将 UpcomingJob 接口中的 clusterId/clusterName 字段替换为 schedulerId/schedulerName
- [x] 4.2 更新组件内部对 clusterName 的引用为 schedulerName
- [x] 4.3 检查所有调用 UpcomingJobsList 的地方（PlatformDashboardPage），同步更新传入数据

## 5. StatusBadge — 修复颜色映射

- [x] 5.1 在 getStatusColor 中将 `case 'blocked': return 'bg-slate-500'` 改为 `return 'bg-red-500'`
- [x] 5.2 确保 Offline 映射保持 bg-slate-500 不变

## 6. 规格文件同步到主规格目录

- [x] 6.1 将 change 中 specs 的变更同步到 openspec/specs/ 对应目录（platform-dashboard、sidebar-navigation、cluster-dashboard、cluster-tabs、unified-create-flow、cluster-calendar）
