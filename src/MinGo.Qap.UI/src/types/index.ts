// Enums
export type ClusterStatus = 'Pending' | 'Online' | 'Warning' | 'Offline' | 'Deleted';
export type SyncStatus = 'Pending' | 'Synced' | 'Failed' | 'Timeout';
export type ScheduleType = 'Once' | 'Cron' | 'Interval';
export type MisfirePolicy = 'FireAndProceed' | 'IgnoreMisfire' | 'DoNothing';

// Common
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  errorMessage?: string;
  errorCode?: string;
  timestamp: string;
}

export interface PagedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PagedQuery {
  page: number;
  pageSize: number;
}

// Cluster
export interface ClusterDto {
  id: string;
  name: string;
  env: string;
  agentUrl?: string; // 已弃用，使用实例列表
  instanceCount: number;
  status: ClusterStatus;
  lastHeartbeat?: string;
  createdAt: string;
}

export interface ClusterSummaryDto {
  id: string;
  name: string;
  env: string;
  status: ClusterStatus;
  lastHeartbeat?: string;
  jobCount: number;
  instanceCount: number;
  healthyInstanceCount: number; // 状态为 Online 的实例数
}

export interface CreateClusterRequest {
  name: string;
  env: string;
  agentUrl: string;
  description?: string;
}

export interface CreateClusterResponse {
  id: string;
  name: string;
  token: string;
  status: ClusterStatus;
  createdAt: string;
}

// Job
export interface ScheduleDto {
  type: ScheduleType;
  cronExpression?: string;
  intervalSeconds?: number;
  runAt?: string;
}

export interface QuartzOptionsDto {
  disallowConcurrentExecution: boolean;
  misfirePolicy: MisfirePolicy;
}

export interface CreateJobRequest {
  jobKey: string;
  jobType: string;
  params: Record<string, any>;
  schedule: ScheduleDto;
  options: QuartzOptionsDto;
}

export interface UpdateJobRequest {
  params?: Record<string, any>;
  schedule?: ScheduleDto;
  options?: QuartzOptionsDto;
}

export interface JobDefinitionDto {
  id: string;
  clusterId: string;
  jobKey: string;
  jobType: string;
  params: string;
  schedule: string;
  options: string;
  status: SyncStatus;
  errorMessage?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface JobSummaryDto {
  jobKey: string;
  jobType: string;
  group: string;
  status: string;
  scheduleType: string;
  cronExpression?: string;
  nextFireTime?: string;
  previousFireTime?: string;
}

export interface JobDetailDto {
  jobKey: string;
  jobType: string;
  group: string;
  status: string;
  description: string;
  schedule: ScheduleDto;
  options: QuartzOptionsDto;
  params: Record<string, any>;
  nextFireTime?: string;
  previousFireTime?: string;
}

export interface JobQuery extends PagedQuery {
  status?: string;
  group?: string;
  keyword?: string;
}

// Job Manifest
export interface ParameterInfoDto {
  name: string;
  type: string;
  required: boolean;
  default?: any;
  label?: string;
}

export interface JobTypeInfoDto {
  key: string;
  description: string;
  parameters: ParameterInfoDto[];
}

export interface JobManifestDto {
  clusterId: string;
  jobs: JobTypeInfoDto[];
}

// Heartbeat
export interface HeartbeatDto {
  timestamp: string;
  agentVersion: string;
  uptimeSeconds: number;
  schedulerStatus: string;
  jobs: JobCountsDto;
  system: SystemMetricsDto;
}

export interface JobCountsDto {
  total: number;
  normal: number;
  paused: number;
  blocked: number;
  executing: number;
}

export interface SystemMetricsDto {
  memoryUsedMb: number;
  memoryTotalMb: number;
  cpuPercent: number;
}

// Agent Instance
export type AgentStatus = 'Pending' | 'Online' | 'Warning' | 'Offline' | 'Deleted';

export interface AgentInstanceDto {
  id: string;
  clusterId: string;
  name?: string;
  url: string;
  status: AgentStatus;
  lastHeartbeat?: string;
  quartzInstanceId?: string;
  agentVersion?: string;
  startedAt?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface AgentSummaryDto {
  id: string;
  name?: string;
  url: string;
  status: string;
  lastHeartbeat?: string;
  agentVersion?: string;
  startedAt?: string;
  createdAt: string;
}

export interface CreateAgentRequest {
  name?: string;
  url: string;
  agentVersion?: string;
  quartzInstanceId?: string;
}

export interface AgentRegistrationResponse {
  agentId: string;
  quartzInstanceId: string;
  clusterId: string;
  platformApiBaseUrl: string;
  heartbeatIntervalSeconds: number;
  warningThresholdSeconds: number;
  offlineThresholdSeconds: number;
}

export interface AgentHeartbeatRequest {
  agentId: string;
  quartzInstanceId?: string;
  agentVersion: string;
  status: string;
  metrics: string; // JSON string
}

export interface AgentHeartbeatResponse {
  success: boolean;
  message?: string;
  nextHeartbeatIntervalSeconds?: number;
}
