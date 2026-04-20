import React, { useState, useEffect } from 'react';
import { X } from 'lucide-react';
import { useCreateCluster } from '../hooks/useClusters';

interface CreateClusterModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const ENV_OPTIONS = [
  { value: 'dev', label: 'Development' },
  { value: 'staging', label: 'Staging' },
  { value: 'prod', label: 'Production' },
];

const CreateClusterModal: React.FC<CreateClusterModalProps> = ({ isOpen, onClose }) => {
  const createCluster = useCreateCluster();
  
  const [name, setName] = useState('');
  const [env, setEnv] = useState('');
  const [agentUrl, setAgentUrl] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    if (isOpen) {
      setName('');
      setEnv('');
      setAgentUrl('');
      setDescription('');
      setError('');
    }
  }, [isOpen]);

  const validate = () => {
    setError('');
    
    if (!name.trim()) {
      setError('Name is required');
      return false;
    }
    
    if (!env) {
      setError('Environment is required');
      return false;
    }

    if (!agentUrl.trim()) {
      setError('Agent URL is required');
      return false;
    }

    return true;
  };

  const handleSubmit = async () => {
    if (!validate()) return;

    try {
      await createCluster.mutateAsync({
        name: name.trim(),
        env,
        agentUrl: agentUrl.trim(),
        description: description.trim() || undefined,
      });
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to create cluster');
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-slate-800 rounded-lg w-full max-w-md overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b border-slate-700">
          <h2 className="text-xl font-semibold text-slate-50">Create Cluster</h2>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-200">
            <X size={20} />
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
          {error && (
            <div className="mb-4 p-3 bg-red-500/20 border border-red-500/50 rounded-md text-red-400 text-sm">
              {error}
            </div>
          )}

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">
                Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g., Production Cluster"
                className="input"
                autoFocus
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">
                Environment <span className="text-red-500">*</span>
              </label>
              <select
                value={env}
                onChange={(e) => setEnv(e.target.value)}
                className="input"
              >
                <option value="">Select environment</option>
                {ENV_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">
                Agent URL <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={agentUrl}
                onChange={(e) => setAgentUrl(e.target.value)}
                placeholder="e.g., http://agent:5000"
                className="input"
              />
              <p className="mt-1 text-xs text-slate-500">
                The base URL where the Quartz agent is running
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">
                Description (optional)
              </label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Describe this cluster..."
                rows={3}
                className="input resize-none"
              />
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex justify-end gap-3 p-4 border-t border-slate-700">
          <button onClick={onClose} className="btn-secondary">
            Cancel
          </button>
          <button 
            onClick={handleSubmit} 
            disabled={createCluster.isPending}
            className="btn-primary disabled:opacity-50"
          >
            {createCluster.isPending ? 'Creating...' : 'Create Cluster'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default CreateClusterModal;