import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clusterApi, agentInstanceApi } from '../api';
import type { AgentInstanceDto } from '../types';

// Query keys
export const agentInstanceQueryKeys = {
  all: ['agentInstances'] as const,
  cluster: (clusterId: string) => ['agentInstances', clusterId] as const,
  agent: (agentId: string) => ['agentInstances', agentId] as const,
};

// Hooks for agent instances
export const useAgentInstances = (clusterId: string) => {
  return useQuery({
    queryKey: agentInstanceQueryKeys.cluster(clusterId),
    queryFn: async () => {
      const response = await clusterApi.getAgents(clusterId);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data || [];
    },
    enabled: !!clusterId,
  });
};

export const useAgentInstance = (agentId: string) => {
  return useQuery({
    queryKey: agentInstanceQueryKeys.agent(agentId),
    queryFn: async () => {
      const response = await agentInstanceApi.get(agentId);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data;
    },
    enabled: !!agentId,
  });
};

export const useDeleteAgentInstance = (clusterId: string) => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (agentId: string) => agentInstanceApi.delete(agentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: agentInstanceQueryKeys.cluster(clusterId) });
    },
  });
};

export const useAgentHeartbeat = (agentId: string) => {
  return useMutation({
    mutationFn: (data: any) => agentInstanceApi.heartbeat(agentId, data),
  });
};