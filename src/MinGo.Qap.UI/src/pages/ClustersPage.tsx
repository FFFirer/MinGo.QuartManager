import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useClusters, useDeleteCluster } from '../hooks/useClusters';
import CreateClusterModal from '../components/CreateClusterModal';
import StatusBadge from '../components/StatusBadge';
import PageHeader from '../components/PageHeader';
import ConfirmDialog from '../components/ConfirmDialog';

const ClustersPage: React.FC = () => {
   const { data: clusters, isLoading, error } = useClusters();
   const deleteCluster = useDeleteCluster();
   const [isModalOpen, setIsModalOpen] = useState(false);
   const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
   const [deletingClusterId, setDeletingClusterId] = useState<string | null>(null);

   const handleDeleteConfirmOpen = (id: string) => {
     setDeletingClusterId(id);
     setIsDeleteConfirmOpen(true);
   };

   const handleDeleteConfirmClose = () => {
     setIsDeleteConfirmOpen(false);
     setDeletingClusterId(null);
   };

   const handleDeleteConfirm = async () => {
     if (deletingClusterId) {
       try {
         await deleteCluster.mutateAsync(deletingClusterId);
       } catch (err: any) {
         alert('Failed to delete cluster: ' + err.message);
       }
     }
     handleDeleteConfirmClose();
   };



   if (isLoading) {
     return <div className="p-8">Loading...</div>;
   }

   if (error) {
     return <div className="p-8">Error: {error.message}</div>;
   }

  return (
    <div className="p-6">
       <PageHeader 
         title="Clusters"
         actions={(
           <button 
             className="btn-primary"
             onClick={() => setIsModalOpen(true)}
           >
             + Add Cluster
           </button>
         )}
       />

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {clusters?.map((cluster: any) => (
          <div
            key={cluster.id}
            className="card transition-all duration-200 hover:border-slate-600"
          >
             <div className="flex items-start justify-between mb-3">
               <div className="flex items-center">
                 <StatusBadge status={cluster.status} size="sm" showLabel={false} />
                 <Link 
                   to={`/clusters/${cluster.id}`}
                   className="font-semibold text-slate-50 hover:text-blue-400"
                 >
                   {cluster.name}
                 </Link>
               </div>
               <span className="text-xs text-slate-400 bg-slate-700 px-2 py-1 rounded">
                 {cluster.env}
               </span>
             </div>

            <div className="text-sm text-slate-400 mb-2">
               <div className="flex items-center gap-4">
                 <span>{cluster.jobCount} jobs</span>
                 <span className="flex items-center gap-1">
                   <StatusBadge status={cluster.healthyInstanceCount > 0 ? 'Online' : 'Offline'} size="sm" showLabel={false} />
                   {cluster.instanceCount} agents ({cluster.healthyInstanceCount} healthy)
                 </span>
               </div>
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
               <Link
                 to={`/clusters/${cluster.id}/agents`}
                 className="text-sm text-green-400 hover:text-green-300"
               >
                 View Agents
               </Link>
                <button 
                  className="text-sm text-red-400 hover:text-red-300"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleDeleteConfirmOpen(cluster.id);
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

       <CreateClusterModal 
         isOpen={isModalOpen} 
         onClose={() => setIsModalOpen(false)} 
       />

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
   );
 };

export default ClustersPage;
