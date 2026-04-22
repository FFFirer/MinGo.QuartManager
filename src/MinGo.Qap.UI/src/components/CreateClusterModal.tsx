import React, { useState, useEffect } from 'react';
import { X, Check, ArrowLeft, ArrowRight } from 'lucide-react';
import { useCreateCluster } from '../hooks/useClusters';
import toast from 'react-hot-toast';

interface CreateClusterModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const ENV_OPTIONS = [
  { value: 'dev', label: 'Development', description: 'Development environment' },
  { value: 'staging', label: 'Staging', description: 'Staging environment' },
  { value: 'prod', label: 'Production', description: 'Production environment' },
];

const STEPS = [
  { id: 1, label: 'Basic Info' },
  { id: 2, label: 'Configuration' },
  { id: 3, label: 'Review' },
  { id: 4, label: 'Create' },
];

const CreateClusterModal: React.FC<CreateClusterModalProps> = ({ isOpen, onClose }) => {
  const createCluster = useCreateCluster();
  
  const [step, setStep] = useState(1);
  const [name, setName] = useState('');
  const [env, setEnv] = useState('');
  const [agentUrl, setAgentUrl] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState('');
  const [showCancelConfirm, setShowCancelConfirm] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setStep(1);
      setName('');
      setEnv('');
      setAgentUrl('');
      setDescription('');
      setError('');
      setShowCancelConfirm(false);
    }
  }, [isOpen]);

  const validateStep = () => {
    setError('');
    
    if (step === 1) {
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
    }
    
    return true;
  };

  const handleNext = () => {
    if (validateStep()) {
      setStep(prev => Math.min(prev + 1, 4));
    }
  };

  const handleBack = () => {
    setStep(prev => Math.max(prev - 1, 1));
    setError('');
  };

  const handleSubmit = async () => {
    if (!validateStep()) return;

    try {
      await createCluster.mutateAsync({
        name: name.trim(),
        env,
        agentUrl: agentUrl.trim(),
        description: description.trim() || undefined,
      });
      toast.success('Cluster created successfully!');
      onClose();
    } catch (err: any) {
      toast.error(err.message || 'Failed to create cluster');
      setError(err.message || 'Failed to create cluster');
    }
  };

  const handleClose = () => {
    if (name || env || agentUrl || description) {
      setShowCancelConfirm(true);
    } else {
      onClose();
    }
  };

  const confirmCancel = () => {
    setShowCancelConfirm(false);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-slate-800 rounded-lg w-full max-w-2xl max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b border-slate-700">
          <h2 className="text-xl font-semibold text-slate-50">Create Cluster</h2>
          <button onClick={handleClose} className="text-slate-400 hover:text-slate-200">
            <X size={20} />
          </button>
        </div>

        {/* Progress Indicator */}
        <div className="flex px-4 py-3 border-b border-slate-700">
          {STEPS.map((s, idx) => (
            <div key={s.id} className="flex items-center">
              <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${
                step > s.id ? 'bg-green-500 text-white' :
                step === s.id ? 'bg-blue-500 text-white' :
                'bg-slate-700 text-slate-400'
              }`}>
                {step > s.id ? <Check size={16} /> : s.id}
              </div>
              <span className={`ml-2 text-sm ${step === s.id ? 'text-slate-50' : 'text-slate-500'}`}>
                {s.label}
              </span>
              {idx < STEPS.length - 1 && <div className="mx-4 w-8 h-px bg-slate-700" />}
            </div>
          ))}
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {error && (
            <div className="mb-4 p-3 bg-red-500/20 border border-red-500/50 rounded-md text-red-400 text-sm">
              {error}
            </div>
          )}

          {/* Step 1: Basic Info */}
          {step === 1 && (
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
                <div className="grid grid-cols-3 gap-2">
                  {ENV_OPTIONS.map((option) => (
                    <div
                      key={option.value}
                      onClick={() => setEnv(option.value)}
                      className={`p-3 rounded-lg border cursor-pointer text-center transition-colors ${
                        env === option.value
                          ? 'border-blue-500 bg-blue-500/10'
                          : 'border-slate-700 hover:border-slate-600'
                      }`}
                    >
                      <div className="font-medium text-slate-50 text-sm">{option.label}</div>
                      <div className="text-xs text-slate-500">{option.description}</div>
                    </div>
                  ))}
                </div>
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
          )}

          {/* Step 2: Configuration (placeholder for future) */}
          {step === 2 && (
            <div className="space-y-4">
              <div className="p-4 bg-slate-700/50 rounded-lg">
                <h3 className="text-lg font-medium text-slate-50 mb-2">Advanced Configuration</h3>
                <p className="text-slate-400 text-sm">
                  Advanced configuration options will be available in a future update.
                  For now, you can proceed with the basic configuration.
                </p>
              </div>
              
              <div className="space-y-3">
                <div className="flex items-center justify-between p-3 bg-slate-700/30 rounded-lg opacity-50">
                  <div>
                    <div className="font-medium text-slate-50">Enable Metrics Collection</div>
                    <div className="text-sm text-slate-400">Collect job execution metrics</div>
                  </div>
                  <input type="checkbox" disabled className="w-5 h-5 rounded" />
                </div>
                
                <div className="flex items-center justify-between p-3 bg-slate-700/30 rounded-lg opacity-50">
                  <div>
                    <div className="font-medium text-slate-50">Auto-failover</div>
                    <div className="text-sm text-slate-400">Automatic agent failover</div>
                  </div>
                  <input type="checkbox" disabled className="w-5 h-5 rounded" />
                </div>
              </div>
            </div>
          )}

          {/* Step 3: Review */}
          {step === 3 && (
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-slate-50 mb-4">Review & Confirm</h3>
              
              <div className="bg-slate-700/30 rounded-lg p-4 space-y-3">
                <div className="flex justify-between">
                  <span className="text-slate-400">Name</span>
                  <span className="text-slate-50 font-medium">{name}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Environment</span>
                  <span className="text-slate-50 font-medium capitalize">{env}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Agent URL</span>
                  <span className="text-slate-50 font-mono text-sm">{agentUrl}</span>
                </div>
                {description && (
                  <div className="flex justify-between">
                    <span className="text-slate-400">Description</span>
                    <span className="text-slate-50">{description}</span>
                  </div>
                )}
              </div>

              <div className="p-4 bg-blue-500/10 border border-blue-500/30 rounded-lg">
                <p className="text-sm text-blue-400">
                  Please verify all information is correct before creating the cluster.
                </p>
              </div>
            </div>
          )}

          {/* Step 4: Creating */}
          {step === 4 && (
            <div className="text-center py-8">
              <div className="animate-spin w-12 h-12 border-4 border-blue-500 border-t-transparent rounded-full mx-auto mb-4" />
              <h3 className="text-lg font-medium text-slate-50 mb-2">Creating Cluster...</h3>
              <p className="text-slate-400">Please wait while we create your cluster.</p>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex justify-between p-4 border-t border-slate-700">
          <button
            onClick={step === 1 ? handleClose : handleBack}
            className="btn-secondary flex items-center gap-2"
          >
            <ArrowLeft size={16} />
            {step === 1 ? 'Cancel' : 'Back'}
          </button>
          
          {step < 4 ? (
            <button onClick={handleNext} className="btn-primary flex items-center gap-2">
              Next
              <ArrowRight size={16} />
            </button>
          ) : (
            <button 
              onClick={handleSubmit} 
              disabled={createCluster.isPending}
              className="btn-primary disabled:opacity-50 flex items-center gap-2"
            >
              {createCluster.isPending ? 'Creating...' : 'Create Cluster'}
            </button>
          )}
        </div>
      </div>

      {/* Cancel Confirmation */}
      {showCancelConfirm && (
        <div className="fixed inset-0 bg-black/70 flex items-center justify-center z-[60]">
          <div className="bg-slate-800 rounded-lg p-6 max-w-sm w-full mx-4">
            <h3 className="text-lg font-semibold text-slate-50 mb-2">Discard Changes?</h3>
            <p className="text-slate-400 mb-4">
              You have unsaved changes. Are you sure you want to discard them?
            </p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setShowCancelConfirm(false)}
                className="btn-secondary"
              >
                Keep Editing
              </button>
              <button
                onClick={confirmCancel}
                className="btn-danger"
              >
                Discard
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CreateClusterModal;