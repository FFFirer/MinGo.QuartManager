import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Layers, Server, Clock, AlertCircle, CheckCircle, XCircle, Activity, RefreshCw, Calendar } from 'lucide-react';
import StatsCard from '../components/StatsCard';
import UpcomingJobsList from '../components/UpcomingJobsList';
import { StatsCardSkeleton, CardSkeleton } from '../components/LoadingSkeleton';
import StatusBadge from '../components/StatusBadge';
import toast from 'react-hot-toast';

interface DashboardData {
  totalClusters: number;
  totalJobs: number;
  totalAgents: number;
  onlineAgents: number;
  warningAgents: number;
  offlineAgents: number;
  lastUpdated?: string;
  clusters: Array<{
    id: string;
    name: string;
    env: string;
    status: string;
    jobCount: number;
    agentCount: number;
    onlineAgentCount: number;
    lastHeartbeat?: string;
  }>;
  upcomingJobs: Array<{
    jobKey: string;
    jobType: string;
    clusterId: string;
    clusterName: string;
    scheduleDescription: string;
    nextFireTime: string;
  }>;
  jobStatus: {
    active: number;
    paused: number;
    blocked: number;
    executing: number;
  };
}

async function fetchDashboard(): Promise<DashboardData> {
  const response = await fetch('/api/dashboard');
  if (!response.ok) {
    throw new Error('Failed to fetch dashboard');
  }
  const result = await response.json();
  return result.data;
}

export function PlatformDashboardPage() {
  const { data, isLoading, error, refetch, isRefetching } = useQuery({
    queryKey: ['platform-dashboard'],
    queryFn: fetchDashboard,
    refetchInterval: 30000,
  });

  const handleRetry = () => {
    refetch();
    toast.success('Refreshing dashboard...');
  };

  if (error) {
    return (
      <div className="p-6">
        <div className="bg-red-500/10 border border-red-500/30 rounded-lg p-6 text-center">
          <AlertCircle size={48} className="mx-auto text-red-400 mb-4" />
          <h2 className="text-xl font-semibold text-slate-50 mb-2">Failed to load dashboard</h2>
          <p className="text-slate-400 mb-4">{error.message}</p>
          <button
            onClick={handleRetry}
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
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-50">Platform Dashboard</h1>
          <p className="text-slate-400 text-sm mt-1">
            Last updated: {data?.lastUpdated ? new Date(data.lastUpdated).toLocaleString() : '...'}
          </p>
        </div>
        <button
          onClick={() => refetch()}
          disabled={isRefetching}
          className="flex items-center gap-2 px-4 py-2 bg-slate-800 text-slate-300 rounded-lg hover:bg-slate-700 transition-colors disabled:opacity-50"
        >
          <RefreshCw size={16} className={isRefetching ? 'animate-spin' : ''} />
          Refresh
        </button>
      </div>

      {/* Overview Stats */}
      <div className="mb-6">
        <h2 className="text-lg font-semibold text-slate-50 mb-4">Platform Overview</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {isLoading ? (
            <>
              <StatsCardSkeleton />
              <StatsCardSkeleton />
              <StatsCardSkeleton />
              <StatsCardSkeleton />
            </>
          ) : (
            <>
              <StatsCard
                title="Total Clusters"
                value={data?.totalClusters ?? 0}
                icon={<Layers size={20} />}
                variant="default"
              />
              <StatsCard
                title="Total Jobs"
                value={data?.totalJobs ?? 0}
                icon={<Clock size={20} />}
                variant="default"
              />
              <StatsCard
                title="Online Agents"
                value={data?.onlineAgents ?? 0}
                subtitle={`of ${data?.totalAgents ?? 0} total`}
                icon={<CheckCircle size={20} />}
                variant="success"
              />
              <StatsCard
                title="Warning Agents"
                value={data?.warningAgents ?? 0}
                subtitle={data?.offlineAgents ? `${data.offlineAgents} offline` : undefined}
                icon={<AlertCircle size={20} />}
                variant={data?.warningAgents ? 'warning' : 'default'}
              />
            </>
          )}
        </div>
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
        {/* Job Status Distribution */}
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <h3 className="text-lg font-semibold text-slate-50 mb-4">Job Status</h3>
          {isLoading ? (
            <div className="space-y-3">
              <CardSkeleton />
              <CardSkeleton />
            </div>
          ) : (
            <div className="space-y-3">
              {[
                { label: 'Active', value: data?.jobStatus.active ?? 0, color: 'bg-green-500' },
                { label: 'Paused', value: data?.jobStatus.paused ?? 0, color: 'bg-amber-500' },
                { label: 'Blocked', value: data?.jobStatus.blocked ?? 0, color: 'bg-red-500' },
                { label: 'Executing', value: data?.jobStatus.executing ?? 0, color: 'bg-blue-500' },
              ].map(item => {
                const total = (data?.jobStatus.active ?? 0) + (data?.jobStatus.paused ?? 0) + 
                              (data?.jobStatus.blocked ?? 0) + (data?.jobStatus.executing ?? 0);
                const percentage = total > 0 ? (item.value / total) * 100 : 0;
                return (
                  <div key={item.label}>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-slate-400">{item.label}</span>
                      <span className="text-slate-50">{item.value}</span>
                    </div>
                    <div className="h-2 bg-slate-700 rounded-full overflow-hidden">
                      <div 
                        className={`h-full ${item.color} transition-all duration-500`}
                        style={{ width: `${percentage}%` }}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {/* Agent Health */}
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <h3 className="text-lg font-semibold text-slate-50 mb-4">Agent Health</h3>
          {isLoading ? (
            <div className="space-y-3">
              <CardSkeleton />
              <CardSkeleton />
            </div>
          ) : (
            <div className="space-y-3">
              {[
                { label: 'Online', value: data?.onlineAgents ?? 0, color: 'bg-green-500' },
                { label: 'Warning', value: data?.warningAgents ?? 0, color: 'bg-amber-500' },
                { label: 'Offline', value: data?.offlineAgents ?? 0, color: 'bg-red-500' },
              ].map(item => {
                const total = (data?.totalAgents ?? 0);
                const percentage = total > 0 ? (item.value / total) * 100 : 0;
                return (
                  <div key={item.label}>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-slate-400">{item.label}</span>
                      <span className="text-slate-50">{item.value}</span>
                    </div>
                    <div className="h-2 bg-slate-700 rounded-full overflow-hidden">
                      <div 
                        className={`h-full ${item.color} transition-all duration-500`}
                        style={{ width: `${percentage}%` }}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>

      {/* Clusters and Upcoming Jobs */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Clusters Overview */}
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-slate-50">Clusters Overview</h3>
            <Link to="/clusters" className="text-sm text-blue-400 hover:text-blue-300">
              View All →
            </Link>
          </div>
          {isLoading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              <CardSkeleton />
              <CardSkeleton />
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {data?.clusters.slice(0, 4).map(cluster => (
                <Link
                  key={cluster.id}
                  to={`/clusters/${cluster.id}`}
                  className="p-3 bg-slate-700/50 rounded-lg hover:bg-slate-700 transition-colors"
                >
                  <div className="flex items-center gap-2 mb-2">
                    <StatusBadge status={cluster.status} size="sm" showLabel={false} />
                    <span className="font-medium text-slate-50">{cluster.name}</span>
                  </div>
                  <div className="text-xs text-slate-400 space-y-1">
                    <p>{cluster.jobCount} jobs</p>
                    <p>{cluster.agentCount} agents ({cluster.onlineAgentCount} healthy)</p>
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>

        {/* Upcoming Jobs */}
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-slate-50">Upcoming Jobs (24h)</h3>
          </div>
          <UpcomingJobsList
            jobs={data?.upcomingJobs ?? []}
            showCluster
            loading={isLoading}
          />
        </div>
      </div>
    </div>
  );
}

export default PlatformDashboardPage;