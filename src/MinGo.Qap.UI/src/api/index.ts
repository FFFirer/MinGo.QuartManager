import axios from 'axios';
import type {
  ApiResponse,
  JobSummaryDto,
  JobDefinitionDto,
  CreateJobRequest,
  UpdateJobRequest,
  JobManifestDto,
  AgentSummaryDto,
  AgentDetailDto,
  AgentSchedulerDto,
  SchedulerSummaryDto,
  SchedulerDetailDto,
  SchedulerAgentDto,
  SchedulerReportRequest,
  RegisterAgentRequest,
  RegisterAgentResponse,
  PagedResponse
} from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || '/';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000,
});

// Error handling interceptor
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const message = error.response?.data?.errorMessage || error.message || 'Unknown error';
    return Promise.reject(new Error(message));
  }
);

// Agent APIs
export const agentApi = {
  getAll: (page?: number, pageSize?: number) =>
    api.get<ApiResponse<PagedResponse<AgentSummaryDto>>>('/api/agents', {
      params: { page, pageSize }
    }).then(r => r.data),

  get: (agentId: string) =>
    api.get<ApiResponse<AgentDetailDto>>(`/api/agents/${agentId}`).then(r => r.data),

  register: (data: RegisterAgentRequest) =>
    api.post<ApiResponse<RegisterAgentResponse>>('/api/agents', data).then(r => r.data),

  delete: (agentId: string) =>
    api.delete<ApiResponse<{}>>(`/api/agents/${agentId}`).then(r => r.data),

  heartbeat: (agentId: string, data: any) =>
    api.post<ApiResponse<{}>>(`/api/agents/${agentId}/heartbeat`, data).then(r => r.data),

  reportSchedulers: (agentId: string, data: SchedulerReportRequest) =>
    api.post<ApiResponse<{}>>(`/api/agents/${agentId}/schedulers`, data).then(r => r.data),

  getSchedulers: (agentId: string) =>
    api.get<ApiResponse<AgentSchedulerDto[]>>(`/api/agents/${agentId}/schedulers`).then(r => r.data),
};

// Scheduler APIs
export const schedulerApi = {
  getAll: () =>
    api.get<ApiResponse<SchedulerSummaryDto[]>>('/api/schedulers').then(r => r.data),

  get: (schedulerName: string) =>
    api.get<ApiResponse<SchedulerDetailDto>>(`/api/schedulers/${encodeURIComponent(schedulerName)}`).then(r => r.data),

  getAgents: (schedulerName: string) =>
    api.get<ApiResponse<SchedulerAgentDto[]>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/agents`).then(r => r.data),
};

// Job APIs (now using schedulerName instead of clusterId)
export const jobApi = {
  getAll: (schedulerName: string, page = 1, pageSize = 20, status?: string, group?: string, keyword?: string) =>
    api.get<ApiResponse<PagedResponse<JobSummaryDto>>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/jobs`, {
      params: { page, pageSize, status, group, keyword }
    }).then(r => r.data),

  get: (schedulerName: string, jobKey: string) =>
    api.get<ApiResponse<JobDefinitionDto>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/jobs/${encodeURIComponent(jobKey)}`).then(r => r.data),

  create: (schedulerName: string, data: CreateJobRequest) =>
    api.post<ApiResponse<JobDefinitionDto>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/jobs`, data).then(r => r.data),

  update: (schedulerName: string, jobKey: string, data: UpdateJobRequest) =>
    api.put<ApiResponse<{}>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/jobs/${encodeURIComponent(jobKey)}`, data).then(r => r.data),

  delete: (schedulerName: string, jobKey: string) =>
    api.delete<ApiResponse<{}>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/jobs/${encodeURIComponent(jobKey)}`).then(r => r.data),

  trigger: (schedulerName: string, jobKey: string) =>
    api.post<ApiResponse<{}>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/jobs/${encodeURIComponent(jobKey)}/trigger`).then(r => r.data),

  pause: (schedulerName: string, jobKey: string) =>
    api.post<ApiResponse<{}>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/jobs/${encodeURIComponent(jobKey)}/pause`).then(r => r.data),

  resume: (schedulerName: string, jobKey: string) =>
    api.post<ApiResponse<{}>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/jobs/${encodeURIComponent(jobKey)}/resume`).then(r => r.data),
};

// Manifest APIs
export const manifestApi = {
  get: (schedulerName: string) =>
    api.get<ApiResponse<JobManifestDto>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/manifest`).then(r => r.data),

  update: (schedulerName: string, data: JobManifestDto) =>
    api.post<ApiResponse<{}>>(`/api/schedulers/${encodeURIComponent(schedulerName)}/manifest`, data).then(r => r.data),
};

export default api;
