import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useClusters, useDeleteCluster } from '../hooks/useClusters';

const ClustersPage: React.FC = () => {
  const { data: clusters, isLoading, error } = useClusters();
  const deleteCluster = useDeleteCluster();

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Online': return 'bg-green-500';
      case 'Warning': return 'bg-amber-500';
      case 'Offline': return 'bg-slate-500';
      case 'Pending': return 'bg-blue-500';
      default: return 'bg-slate-500';
    }
  };

  if (isLoading) {
    return <div className="p-8 text-slate-400">Loading...</div>;
  }

  if (error) {
    return <div className="p-8 text-red-400">Error: {error.message}</div>;
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-slate-50">Clusters</h1>
        <button className="btn-primary">+ Add Cluster</button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {clusters?.map((cluster: any) => (
          <div
            key={cluster.id}
            className="card transition-all duration-200 hover:border-slate-600"
          >
            <div className="flex items-start justify-between mb-3">
              <div className="flex items-center">
                <span className={`status-dot ${getStatusColor(cluster.status)}`} />
                <span className="font-semibold text-slate-50">{cluster.name}</span>
              </div>
              <span className="text-xs text-slate-400 bg-slate-700 px-2 py-1 rounded">
                {cluster.env}
              </span>
            </div>

            <div className="text-sm text-slate-400 mb-2">
              {cluster.jobCount} jobs
            </div>

            {cluster.lastHeartbeat && (
              <div className="text-xs text-slate-500">
                Last seen: {new Date(cluster.lastHeartbeat).toLocaleString()}
              </div>
            )}

            <div className="mt-3 pt-3 border-t border-slate-700 flex justify-end gap-2">
              <Link
                to={`/clusters/${cluster.id}/jobs`}
                className="text-sm text-blue-400 hover:text-blue-300"
              >
                View Jobs
              </Link>
              <button 
                className="text-sm text-red-400 hover:text-red-300"
                onClick={(e) => {
                  e.stopPropagation();
                  if (confirm('Delete this cluster?')) {
                    deleteCluster.mutate(cluster.id);
                  }
                }}
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>

      {clusters?.length === 0 && (
        <div className="text-center py-12 text-slate-400">
          No clusters yet. Click "Add Cluster" to get started.
        </div>
      )}
    </div>
  );
};

export default ClustersPage;
