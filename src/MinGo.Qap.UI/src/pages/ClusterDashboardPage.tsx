import { useQuery } from '@tanstack/react-query';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { ArrowLeft, Server, Clock, Calendar, RefreshCw, AlertCircle, Play, Pause, Plus, Users } from 'lucide-react';
import toast from 'react-hot-toast';
import StatsCard from '../components/StatsCard';
import UpcomingJobsList from '../components/UpcomingJobsList';
import { StatsCardSkeleton, CardSkeleton } from '../components/LoadingSkeleton';
import StatusBadge from '../components/StatusBadge';
import ClusterTabs from '../components/ClusterTabs';
import CreateJobModal from '../components/CreateJobModal';

interface ClusterDashboardData {
  clusterId: string;
  clusterName: string;
  status: string;
  env: string;
  createdAt: string;
  jobSummary: {
    total: number;
    active: number;
    paused: number;
    blocked: number;
    executing: number;
  };
  agentSummary: {
    total: number;
    online: number;
    warning: number;
    offline: number;
  };
  recentAgents: Array<{
    id: string;
    name: string;
    url: string;
    status: string;
    lastHeartbeat?: string;
  }>;
  upcomingJobs: Array<{
    jobKey: string;
    jobType: string;
    scheduleDescription: string;
    nextFireTime: string;
  }>;
  lastUpdated: string;
}

async function fetchClusterDashboard(clusterId: string): Promise<ClusterDashboardData> {
  const response = await fetch(`/api/clusters/${clusterId}/dashboard`);
  if (!response.ok) {
    throw new Error('Failed to fetch cluster dashboard');
  }
  const result = await response.json();
  return result.data;
}

export function ClusterDashboardPage() {
  const { clusterId } = useParams<{ clusterId: string }>();
  const navigate = useNavigate();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  const { data, isLoading, error, refetch, isRefetching } = useQuery({
    queryKey: ['cluster-dashboard', clusterId],
    queryFn: () => fetchClusterDashboard(clusterId!),
    refetchInterval: 30000,
    enabled: !!clusterId,
  });

  useEffect(() => {
    if (data && data.clusterName) {
      const savedCluster = {
        id: data.clusterId,
        name: data.clusterName,
        status: data.status,
        env: data.env
      };
      localStorage.setItem('sidebar-selected-cluster', JSON.stringify(savedCluster));
    }
  }, [data]);

  if (error) {
    return (
      <div className="p-6">
        <button
          onClick={() => navigate('/clusters')}
          className="flex items-center gap-2 text-slate-400 hover:text-slate-50 mb-4"
        >
          <ArrowLeft size={16} />
          Back to Clusters
        </button>
        <div className="bg-red-500/10 border border-red-500/30 rounded-lg p-6 text-center">
          <AlertCircle size={48} className="mx-auto text-red-400 mb-4" />
          <h2 className="text-xl font-semibold text-slate-50 mb-2">Failed to load cluster</h2>
          <p className="text-slate-400 mb-4">{error.message}</p>
          <button
            onClick={() => refetch()}
            className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <ClusterTabs
        clusterName={data?.clusterName || 'Cluster'}
        clusterStatus={data?.status || 'Unknown'}
        clusterEnv={data?.env}
      />

      {/* Overview Stats */}
      <div className="mb-6">
        <h2 className="text-lg font-semibold text-slate-50 mb-4">Cluster Overview</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
          {isLoading ? (
            <>
              <StatsCardSkeleton />
              <StatsCardSkeleton />
              <StatsCardSkeleton />
              <StatsCardSkeleton />
              <StatsCardSkeleton />
            </>
          ) : (
            <>
              <StatsCard
                title="Total Jobs"
                value={data?.jobSummary.total ?? 0}
                icon={<Clock size={20} />}
                variant="default"
              />
              <StatsCard
                title="Active"
                value={data?.jobSummary.active ?? 0}
                icon={<Play size={20} />}
                variant="success"
              />
              <StatsCard
                title="Paused"
                value={data?.jobSummary.paused ?? 0}
                icon={<Pause size={20} />}
                variant="warning"
              />
              <StatsCard
                title="Blocked"
                value={data?.jobSummary.blocked ?? 0}
                icon={<AlertCircle size={20} />}
                variant={data?.jobSummary.blocked ? 'danger' : 'default'}
              />
              <StatsCard
                title="Agents"
                value={`${data?.agentSummary.online ?? 0}/${data?.agentSummary.total ?? 0}`}
                subtitle="Online"
                icon={<Server size={20} />}
                variant={data?.agentSummary.offline ? 'warning' : 'success'}
              />
            </>
          )}
        </div>
      </div>

      {/* Two Column Layout */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Agents */}
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-slate-50">Agent Instances</h3>
            <Link
              to={`/clusters/${clusterId}/agents`}
              className="text-sm text-blue-400 hover:text-blue-300"
            >
              View All →
            </Link>
          </div>
          {isLoading ? (
            <div className="space-y-3">
              <CardSkeleton />
              <CardSkeleton />
            </div>
          ) : (
            <div className="space-y-2">
              {data?.recentAgents?.slice(0, 5).map(agent => (
                <div
                  key={agent.id}
                  className="flex items-center justify-between p-3 bg-slate-700/50 rounded-lg"
                >
                  <div className="flex items-center gap-3">
                    <StatusBadge status={agent.status} size="sm" showLabel={false} />
                    <div>
                      <p className="text-sm font-medium text-slate-50">
                        {agent.name || agent.url.split('://')[1]?.split(':')[0]}
                      </p>
                      <p className="text-xs text-slate-400">{agent.url}</p>
                    </div>
                  </div>
                  <div className="text-xs text-slate-500">
                    {agent.lastHeartbeat && (
                      <span>Last seen: {new Date(agent.lastHeartbeat).toLocaleTimeString()}</span>
                    )}
                  </div>
                </div>
              ))}
              {(!data?.recentAgents || data.recentAgents.length === 0) && (
                <p className="text-center text-slate-400 py-4">No agent instances</p>
              )}
            </div>
          )}
        </div>

        {/* Upcoming Jobs */}
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-slate-50">Upcoming Jobs (24h)</h3>
            <Link
              to={`/clusters/${clusterId}/calendar`}
              className="text-sm text-blue-400 hover:text-blue-300 flex items-center gap-1"
            >
              <Calendar size={14} />
              View Calendar
            </Link>
          </div>
          <UpcomingJobsList
            jobs={data?.upcomingJobs ?? []}
            loading={isLoading}
          />
        </div>
      </div>

      {/* Execution History Placeholder */}
      <div className="mt-6 bg-slate-800 rounded-lg p-4 border border-slate-700">
        <h3 className="text-lg font-semibold text-slate-50 mb-4">Recent Executions</h3>
        <div className="text-center py-8 text-slate-400">
          <AlertCircle size={32} className="mx-auto mb-2 opacity-50" />
          <p>Execution history is not yet available</p>
          <p className="text-sm text-slate-500 mt-1">This feature will be added in a future update.</p>
        </div>
      </div>

      {clusterId && (
        <CreateJobModal
          clusterId={clusterId}
          isOpen={isCreateModalOpen}
          onClose={() => setIsCreateModalOpen(false)}
        />
      )}
    </div>
  );
}

export default ClusterDashboardPage;