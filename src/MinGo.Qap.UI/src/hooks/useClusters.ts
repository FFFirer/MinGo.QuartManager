import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clusterApi, jobApi, manifestApi } from '../api';
import type { 
  CreateClusterRequest, 
  CreateJobRequest, 
  UpdateJobRequest,
  HeartbeatDto,
  JobManifestDto
} from '../types';

// Query keys
export const queryKeys = {
  clusters: ['clusters'] as const,
  cluster: (id: string) => ['clusters', id] as const,
  jobs: (clusterId: string) => ['jobs', clusterId] as const,
  job: (clusterId: string, jobKey: string) => ['jobs', clusterId, jobKey] as const,
  manifest: (clusterId: string) => ['manifest', clusterId] as const,
};

// Cluster hooks
export const useClusters = () => {
  return useQuery({
    queryKey: queryKeys.clusters,
    queryFn: async () => {
      const response = await clusterApi.getAll();
      if (!response.success) throw new Error(response.errorMessage);
      return response.data || [];
    },
  });
};

export const useCluster = (clusterId: string) => {
  return useQuery({
    queryKey: queryKeys.cluster(clusterId),
    queryFn: async () => {
      const response = await clusterApi.get(clusterId);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data;
    },
    enabled: !!clusterId,
  });
};

export const useCreateCluster = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateClusterRequest) => clusterApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.clusters });
    },
  });
};

export const useDeleteCluster = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (clusterId: string) => clusterApi.delete(clusterId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.clusters });
    },
  });
};

// Job hooks
export const useJobs = (clusterId: string, page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: [...queryKeys.jobs(clusterId), page, pageSize],
    queryFn: async () => {
      const response = await jobApi.getAll(clusterId, page, pageSize);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data || [];
    },
    enabled: !!clusterId,
  });
};

export const useJob = (clusterId: string, jobKey: string) => {
  return useQuery({
    queryKey: queryKeys.job(clusterId, jobKey),
    queryFn: async () => {
      const response = await jobApi.get(clusterId, jobKey);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data;
    },
    enabled: !!clusterId && !!jobKey,
  });
};

export const useCreateJob = (clusterId: string) => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateJobRequest) => jobApi.create(clusterId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.jobs(clusterId) });
    },
  });
};

export const useUpdateJob = (clusterId: string, jobKey: string) => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: UpdateJobRequest) => jobApi.update(clusterId, jobKey, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.jobs(clusterId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.job(clusterId, jobKey) });
    },
  });
};

export const useDeleteJob = (clusterId: string) => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (jobKey: string) => jobApi.delete(clusterId, jobKey),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.jobs(clusterId) });
    },
  });
};

export const useTriggerJob = (clusterId: string) => {
  return useMutation({
    mutationFn: (jobKey: string) => jobApi.trigger(clusterId, jobKey),
  });
};

export const usePauseJob = (clusterId: string) => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (jobKey: string) => jobApi.pause(clusterId, jobKey),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.jobs(clusterId) });
    },
  });
};

export const useResumeJob = (clusterId: string) => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (jobKey: string) => jobApi.resume(clusterId, jobKey),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.jobs(clusterId) });
    },
  });
};

// Manifest hooks
export const useManifest = (clusterId: string) => {
  return useQuery({
    queryKey: queryKeys.manifest(clusterId),
    queryFn: async () => {
      const response = await manifestApi.get(clusterId);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data;
    },
    enabled: !!clusterId,
  });
};
