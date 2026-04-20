import React, { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { ArrowLeft, Server, Trash2 } from 'lucide-react';
import { useCluster, useDeleteCluster } from '../hooks/useClusters';
import { useAgentInstances } from '../hooks/useAgentInstances';
import StatusBadge from '../components/StatusBadge';
import PageHeader from '../components/PageHeader';
import ConfirmDialog from '../components/ConfirmDialog';

const ClusterDetailPage: React.FC = () => {
  const { clusterId } = useParams<{ clusterId: string }>();
  const { data: cluster, isLoading, error } = useCluster(clusterId!);
  const { data: agents } = useAgentInstances(clusterId!);
  const deleteCluster = useDeleteCluster();



   const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);

   const handleDeleteConfirmOpen = () => {
     setIsDeleteConfirmOpen(true);
   };

   const handleDeleteConfirmClose = () => {
     setIsDeleteConfirmOpen(false);
   };

   const handleDeleteConfirm = async () => {
     await deleteCluster.mutateAsync(clusterId!);
     // Navigation will happen automatically after successful delete (handled by the hook)
     window.location.href = '/clusters';
     handleDeleteConfirmClose();
   };

   if (isLoading) {
     return <div className="p-8">Loading...</div>;
   }

   if (error) {
     return <div className="p-8">Error: {error.message}</div>;
   }

   if (!cluster) {
     return <div className="p-8">Cluster not found</div>;
   }

  return (
    <div className="p-6">
       <PageHeader 
         title="Cluster Details"
         backPath="/"
         status={<StatusBadge status={cluster?.status ?? 'Pending'} size="sm" showLabel={true} />}
       />

      {/* Cluster Info */}
      <div className="card mb-6">
           <div className="flex items-start justify-between">
             <div className="flex items-center gap-3">
               <StatusBadge status={cluster.status} size="sm" showLabel={true} variant="badge" />
               <div>
                 <h2 className="text-xl font-semibold text-slate-50">{cluster.name}</h2>
                 <p className="text-sm text-slate-400">{cluster.env}</p>
               </div>
             </div>
           </div>

        <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mt-6">
          <div>
            <p className="text-sm text-slate-400">Instance Count</p>
            <p className="text-lg font-semibold text-slate-50">{cluster.instanceCount}</p>
          </div>
          <div>
            <p className="text-sm text-slate-400">Created</p>
            <p className="text-lg font-semibold text-slate-50">
              {new Date(cluster.createdAt).toLocaleDateString()}
            </p>
          </div>
          {cluster.lastHeartbeat && (
            <div>
              <p className="text-sm text-slate-400">Last Heartbeat</p>
              <p className="text-lg font-semibold text-slate-50">
                {new Date(cluster.lastHeartbeat).toLocaleString()}
              </p>
            </div>
          )}
        </div>

         <div className="mt-6 pt-4 border-t border-slate-700 flex justify-end gap-2">
           <button 
             onClick={handleDeleteConfirmOpen}
             disabled={deleteCluster.isPending}
             className="btn-danger flex items-center gap-2"
           >
             <Trash2 size={16} />
             {deleteCluster.isPending ? 'Deleting...' : 'Delete Cluster'}
           </button>
         </div>

         <ConfirmDialog
           isOpen={isDeleteConfirmOpen}
           onClose={handleDeleteConfirmClose}
           title="Delete Cluster"
           message="Are you sure you want to delete this cluster? This action cannot be undone."
           confirmLabel="Delete"
           cancelLabel="Cancel"
           isConfirmLoading={deleteCluster.isPending}
           onConfirm={handleDeleteConfirm}
         />
      </div>

      {/* Agent Instances */}
      <div className="card">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-slate-50 flex items-center gap-2">
            <Server size={18} />
            Agent Instances
          </h3>
          <Link 
            to={`/clusters/${clusterId}/agents`}
            className="text-sm text-blue-400 hover:text-blue-300"
          >
            View All
          </Link>
        </div>

        {agents && agents.length > 0 ? (
          <div className="space-y-2">
            {agents.slice(0, 5).map((agent: any) => (
              <div 
                key={agent.id}
                className="flex items-center justify-between p-3 bg-slate-700/50 rounded-lg"
              >
                 <div className="flex items-center gap-3">
                   <StatusBadge status={agent.status} size="sm" showLabel={false} />
                   <div>
                     <p className="text-sm font-medium text-slate-50">
                       {agent.name || agent.url}
                     </p>
                     <p className="text-xs text-slate-400">{agent.url}</p>
                   </div>
                 </div>
                <div className="text-xs text-slate-400">
                  {agent.lastHeartbeat && (
                    <span>Last seen: {new Date(agent.lastHeartbeat).toLocaleString()}</span>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-slate-400">No agent instances registered</p>
        )}
      </div>
    </div>
  );
};

export default ClusterDetailPage;