import axios from 'axios';
import type { 
  ApiResponse, 
  ClusterDto, 
  ClusterSummaryDto, 
  CreateClusterRequest,
  CreateClusterResponse,
  JobSummaryDto,
  JobDefinitionDto,
  CreateJobRequest,
  UpdateJobRequest,
  JobManifestDto,
  HeartbeatDto
} from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

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

// Cluster APIs
export const clusterApi = {
  getAll: () => 
    api.get<ApiResponse<ClusterSummaryDto[]>>('/api/clusters').then(r => r.data),
  
  get: (clusterId: string) => 
    api.get<ApiResponse<ClusterDto>>(`/api/clusters/${clusterId}`).then(r => r.data),
  
  create: (data: CreateClusterRequest) => 
    api.post<ApiResponse<CreateClusterResponse>>('/api/clusters', data).then(r => r.data),
  
  delete: (clusterId: string) => 
    api.delete<ApiResponse<{}>>(`/api/clusters/${clusterId}`).then(r => r.data),
  
  heartbeat: (clusterId: string, data: HeartbeatDto) => 
    api.post<ApiResponse<{}>>(`/api/clusters/${clusterId}/heartbeat`, data).then(r => r.data),
};

// Job APIs
export const jobApi = {
  getAll: (clusterId: string, page = 1, pageSize = 20, status?: string, group?: string, keyword?: string) => 
    api.get<ApiResponse<JobSummaryDto[]>>(`/api/clusters/${clusterId}/jobs`, {
      params: { page, pageSize, status, group, keyword }
    }).then(r => r.data),
  
  get: (clusterId: string, jobKey: string) => 
    api.get<ApiResponse<JobDefinitionDto>>(`/api/clusters/${clusterId}/jobs/${jobKey}`).then(r => r.data),
  
  create: (clusterId: string, data: CreateJobRequest) => 
    api.post<ApiResponse<JobDefinitionDto>>(`/api/clusters/${clusterId}/jobs`, data).then(r => r.data),
  
  update: (clusterId: string, jobKey: string, data: UpdateJobRequest) => 
    api.put<ApiResponse<{}>>(`/api/clusters/${clusterId}/jobs/${jobKey}`, data).then(r => r.data),
  
  delete: (clusterId: string, jobKey: string) => 
    api.delete<ApiResponse<{}>>(`/api/clusters/${clusterId}/jobs/${jobKey}`).then(r => r.data),
  
  trigger: (clusterId: string, jobKey: string) => 
    api.post<ApiResponse<{}>>(`/api/clusters/${clusterId}/jobs/${jobKey}/trigger`).then(r => r.data),
  
  pause: (clusterId: string, jobKey: string) => 
    api.post<ApiResponse<{}>>(`/api/clusters/${clusterId}/jobs/${jobKey}/pause`).then(r => r.data),
  
  resume: (clusterId: string, jobKey: string) => 
    api.post<ApiResponse<{}>>(`/api/clusters/${clusterId}/jobs/${jobKey}/resume`).then(r => r.data),
};

// Manifest APIs
export const manifestApi = {
  get: (clusterId: string) => 
    api.get<ApiResponse<JobManifestDto>>(`/api/clusters/${clusterId}/manifest`).then(r => r.data),
  
  update: (clusterId: string, data: JobManifestDto) => 
    api.post<ApiResponse<{}>>(`/api/clusters/${clusterId}/manifest`, data).then(r => r.data),
};

export default api;
