import React, { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useCluster } from "../hooks/useClusters";
import { useAgentInstances } from "../hooks/useAgentInstances";
import StatusBadge from "../components/StatusBadge";
import DataTable from "../components/DataTable";
import ConfirmDialog from "../components/ConfirmDialog";

const AgentInstancesPage: React.FC = () => {
  const { clusterId } = useParams<{ clusterId: string }>();
  const {
    data: cluster,
    isLoading: clusterLoading,
    error: clusterError,
  } = useCluster(clusterId || "");
  const {
    data: instances,
    isLoading: instancesLoading,
    error: instancesError,
  } = useAgentInstances(clusterId || "");

  const formatTime = (time?: string) => {
    if (!time) return "Never";
    return new Date(time).toLocaleString();
  };

  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
  const [deletingInstanceId, setDeletingInstanceId] = useState<string | null>(
    null,
  );

  const handleDeleteInstance = async (id: string) => {
    setDeletingInstanceId(id);
    setIsDeleteConfirmOpen(true);
  };

  const confirmDeleteInstance = async () => {
    // TODO: Implement actual delete agent instance functionality
    // For now, just close the dialog and show an alert
    setIsDeleteConfirmOpen(false);
    alert("Agent instance deletion would be implemented here");
    setDeletingInstanceId(null);
  };

  if (clusterLoading || instancesLoading) {
    return <div className="p-8">Loading...</div>;
  }

  if (clusterError || instancesError) {
    const error = clusterError || instancesError;
    return <div className="p-8">Error: {error?.message}</div>;
  }

  return (
    <>
      <div className="p-6">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <Link to="/clusters" className="text-blue-400 hover:text-blue-300">
              Clusters
            </Link>
            <span className="text-slate-400">/</span>
            <span className="text-slate-50">{cluster?.name}</span>
            <span className="text-slate-400">/</span>
            <h1 className="text-2xl font-bold text-slate-50">
              Agent Instances
            </h1>
          </div>
          <button
            className="btn-primary"
            onClick={() =>
              alert(
                "Agent instances are automatically registered when they start. To add an agent, start a new agent instance with the same cluster ID.",
              )
            }
          >
            + Register Agent
          </button>
        </div>

        <div className="mb-6 p-4 bg-slate-800 rounded-lg">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="text-center p-3 bg-slate-900 rounded">
              <div className="text-2xl font-bold text-slate-50">
                {cluster?.instanceCount || 0}
              </div>
              <div className="text-sm text-slate-400">Total Instances</div>
            </div>
            <div className="text-center p-3 bg-slate-900 rounded">
              <div className="text-2xl font-bold text-green-500">
                {instances?.filter((i) => i.status === "Online").length || 0}
              </div>
              <div className="text-sm text-slate-400">Online</div>
            </div>
            <div className="text-center p-3 bg-slate-900 rounded">
              <div className="text-2xl font-bold text-amber-500">
                {instances?.filter((i) => i.status === "Warning").length || 0}
              </div>
              <div className="text-sm text-slate-400">Warning</div>
            </div>
            <div className="text-center p-3 bg-slate-900 rounded">
              <div className="text-2xl font-bold text-slate-500">
                {instances?.filter((i) => i.status === "Offline").length || 0}
              </div>
              <div className="text-sm text-slate-400">Offline</div>
            </div>
          </div>
        </div>

        <DataTable
          columns={[
            {
              header: "ID",
              accessor: (row: any) => row.id.slice(0, 8) + "...",
              width: 80,
              align: "left",
            },
            {
              header: "Name",
              accessor: "name",
              width: 120,
              align: "left",
            },
            {
              header: "Status",
              accessor: (row: any) => (
                <StatusBadge
                  status={row.status}
                  size="sm"
                  showLabel={true}
                  variant="inline"
                />
              ),
              width: 100,
              align: "center",
            },
            {
              header: "URL",
              accessor: "url",
              width: 150,
              align: "left",
            },
            {
              header: "Last Heartbeat",
              accessor: (row: any) => formatTime(row.lastHeartbeat),
              width: 140,
              align: "left",
            },
            {
              header: "Version",
              accessor: "agentVersion",
              width: 80,
              align: "left",
            },
            {
              header: "Actions",
              accessor: (row: any) => (
                <button
                  onClick={() => handleDeleteInstance(row.id)}
                  className="text-red-400 hover:text-red-300 text-sm"
                >
                  Delete
                </button>
              ),
              width: 80,
              align: "center",
            },
          ]}
          data={instances || []}
          loading={instancesLoading}
          emptyMessage="No agent instances found for this cluster."
          showBorder
          showHeader
          className="w-full"
        />
      </div>

      <ConfirmDialog
        isOpen={isDeleteConfirmOpen}
        onClose={() => {
          setIsDeleteConfirmOpen(false);
          setDeletingInstanceId(null);
        }}
        title="Delete Agent Instance"
        message="Are you sure you want to delete this agent instance? This action cannot be undone."
        confirmLabel="Delete"
        cancelLabel="Cancel"
        onConfirm={confirmDeleteInstance}
      />
    </>
  );
};

export default AgentInstancesPage;
