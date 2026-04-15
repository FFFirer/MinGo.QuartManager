import React, { useState, useEffect } from 'react';
import { X } from 'lucide-react';
import { useCreateJob } from '../hooks/useClusters';
import { useManifest } from '../hooks/useClusters';
import type { CreateJobRequest, ScheduleDto, QuartzOptionsDto, JobTypeInfoDto, ParameterInfoDto, ScheduleType } from '../types';

interface CreateJobModalProps {
  clusterId: string;
  isOpen: boolean;
  onClose: () => void;
}

const SCHEDULE_TYPES = [
  { value: 'Once', label: 'Once', description: 'Run one time' },
  { value: 'Cron', label: 'Cron', description: 'Cron expression' },
  { value: 'Interval', label: 'Interval', description: 'Repeat interval' },
];

const MISFIRE_POLICIES = [
  { value: 'FireAndProceed', label: 'Fire and Proceed' },
  { value: 'IgnoreMisfire', label: 'Ignore Misfire' },
  { value: 'DoNothing', label: 'Do Nothing' },
];

const CreateJobModal: React.FC<CreateJobModalProps> = ({ clusterId, isOpen, onClose }) => {
  const { data: manifest } = useManifest(clusterId);
  const createJob = useCreateJob(clusterId);
  
  // Form state
  const [step, setStep] = useState(1);
  const [selectedJobType, setSelectedJobType] = useState('');
  const [jobKey, setJobKey] = useState('');
  const [params, setParams] = useState<Record<string, any>>({});
  const [scheduleType, setScheduleType] = useState<ScheduleType>('Cron');
  const [cronExpression, setCronExpression] = useState('0 0 * * *');
  const [intervalSeconds, setIntervalSeconds] = useState(60);
  const [runAt, setRunAt] = useState('');
  const [disallowConcurrent, setDisallowConcurrent] = useState(false);
  const [misfirePolicy, setMisfirePolicy] = useState('FireAndProceed');
  const [error, setError] = useState('');

  const selectedJob = manifest?.jobs?.find(j => j.key === selectedJobType);

  useEffect(() => {
    if (isOpen) {
      // Reset form
      setStep(1);
      setSelectedJobType('');
      setJobKey('');
      setParams({});
      setScheduleType('Cron');
      setCronExpression('0 0 * * *');
      setIntervalSeconds(60);
      setRunAt('');
      setDisallowConcurrent(false);
      setMisfirePolicy('FireAndProceed');
      setError('');
    }
  }, [isOpen]);

  const handleParamChange = (name: string, value: any) => {
    setParams(prev => ({ ...prev, [name]: value }));
  };

  const validateStep = () => {
    setError('');
    
    if (step === 1) {
      if (!selectedJobType) {
        setError('Please select a job type');
        return false;
      }
      if (!jobKey.trim()) {
        setError('Please enter a job key');
        return false;
      }
    }
    
    if (step === 3) {
      if (scheduleType === 'Cron' && !cronExpression.trim()) {
        setError('Please enter a cron expression');
        return false;
      }
      if (scheduleType === 'Interval' && intervalSeconds <= 0) {
        setError('Please enter a valid interval');
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

    const schedule: ScheduleDto = {
      type: scheduleType,
      cronExpression: scheduleType === 'Cron' ? cronExpression : undefined,
      intervalSeconds: scheduleType === 'Interval' ? intervalSeconds : undefined,
      runAt: scheduleType === 'Once' && runAt ? new Date(runAt).toISOString() : undefined,
    };

    const options: QuartzOptionsDto = {
      disallowConcurrentExecution: disallowConcurrent,
      misfirePolicy: misfirePolicy as any,
    };

    const request: CreateJobRequest = {
      jobKey,
      jobType: selectedJobType,
      params,
      schedule,
      options,
    };

    try {
      await createJob.mutateAsync(request);
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to create job');
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-slate-800 rounded-lg w-full max-w-2xl max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b border-slate-700">
          <h2 className="text-xl font-semibold text-slate-50">Create Job</h2>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-200">
            <X size={20} />
          </button>
        </div>

        {/* Progress */}
        <div className="flex px-4 py-3 border-b border-slate-700">
          {['Select Type', 'Configure', 'Schedule', 'Options'].map((label, idx) => (
            <div key={label} className="flex items-center">
              <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${
                step > idx + 1 ? 'bg-green-500 text-white' :
                step === idx + 1 ? 'bg-blue-500 text-white' :
                'bg-slate-700 text-slate-400'
              }`}>
                {step > idx + 1 ? '✓' : idx + 1}
              </div>
              <span className={`ml-2 text-sm ${step === idx + 1 ? 'text-slate-50' : 'text-slate-500'}`}>
                {label}
              </span>
              {idx < 3 && <div className="mx-4 w-8 h-px bg-slate-700" />}
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

          {/* Step 1: Select Job Type */}
          {step === 1 && (
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-2">Job Type</label>
                <div className="grid grid-cols-1 gap-2">
                  {manifest?.jobs?.map((job: JobTypeInfoDto) => (
                    <div
                      key={job.key}
                      onClick={() => setSelectedJobType(job.key)}
                      className={`p-4 rounded-lg border cursor-pointer transition-colors ${
                        selectedJobType === job.key
                          ? 'border-blue-500 bg-blue-500/10'
                          : 'border-slate-700 hover:border-slate-600'
                      }`}
                    >
                      <div className="font-medium text-slate-50">{job.key}</div>
                      <div className="text-sm text-slate-400">{job.description}</div>
                      <div className="text-xs text-slate-500 mt-1">
                        {job.parameters.length} parameters
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-300 mb-2">Job Key</label>
                <input
                  type="text"
                  value={jobKey}
                  onChange={(e) => setJobKey(e.target.value)}
                  placeholder="e.g., daily-sync"
                  className="input"
                />
                <p className="mt-1 text-xs text-slate-500">
                  Unique identifier for this job instance
                </p>
              </div>
            </div>
          )}

          {/* Step 2: Configure Parameters */}
          {step === 2 && selectedJob && (
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-slate-50 mb-4">
                Configure {selectedJob.key}
              </h3>
              
              {selectedJob.parameters.length === 0 ? (
                <p className="text-slate-400">No parameters required</p>
              ) : (
                selectedJob.parameters.map((param: ParameterInfoDto) => (
                  <div key={param.name}>
                    <label className="block text-sm font-medium text-slate-300 mb-2">
                      {param.label || param.name}
                      {param.required && <span className="text-red-500 ml-1">*</span>}
                    </label>
                    {param.type === 'bool' ? (
                      <select
                        value={params[param.name]?.toString() || param.default?.toString() || 'false'}
                        onChange={(e) => handleParamChange(param.name, e.target.value === 'true')}
                        className="input"
                      >
                        <option value="true">True</option>
                        <option value="false">False</option>
                      </select>
                    ) : param.type === 'int' ? (
                      <input
                        type="number"
                        value={params[param.name] || param.default || ''}
                        onChange={(e) => handleParamChange(param.name, parseInt(e.target.value))}
                        className="input"
                      />
                    ) : (
                      <input
                        type="text"
                        value={params[param.name] || param.default || ''}
                        onChange={(e) => handleParamChange(param.name, e.target.value)}
                        placeholder={param.required ? 'Required' : 'Optional'}
                        className="input"
                      />
                    )}
                    <p className="mt-1 text-xs text-slate-500">
                      Type: {param.type} {param.default !== undefined && `(default: ${param.default})`}
                    </p>
                  </div>
                ))
              )}
            </div>
          )}

          {/* Step 3: Schedule */}
          {step === 3 && (
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-2">Schedule Type</label>
                <div className="grid grid-cols-3 gap-2">
                  {SCHEDULE_TYPES.map((type) => (
                    <div
                      key={type.value}
                      onClick={() => setScheduleType(type.value as ScheduleType)}
                      className={`p-3 rounded-lg border cursor-pointer text-center transition-colors ${
                        scheduleType === type.value
                          ? 'border-blue-500 bg-blue-500/10'
                          : 'border-slate-700 hover:border-slate-600'
                      }`}
                    >
                      <div className="font-medium text-slate-50 text-sm">{type.label}</div>
                      <div className="text-xs text-slate-500">{type.description}</div>
                    </div>
                  ))}
                </div>
              </div>

              {scheduleType === 'Cron' && (
                <div>
                  <label className="block text-sm font-medium text-slate-300 mb-2">
                    Cron Expression
                  </label>
                  <input
                    type="text"
                    value={cronExpression}
                    onChange={(e) => setCronExpression(e.target.value)}
                    placeholder="0 0 * * *"
                    className="input font-mono"
                  />
                  <div className="mt-2 text-xs text-slate-500 space-y-1">
                    <p>Examples:</p>
                    <p>• <code className="bg-slate-700 px-1 rounded">0 0 * * *</code> - Daily at midnight</p>
                    <p>• <code className="bg-slate-700 px-1 rounded">0 */6 * * *</code> - Every 6 hours</p>
                    <p>• <code className="bg-slate-700 px-1 rounded">0 0 * * 1</code> - Weekly on Monday</p>
                  </div>
                </div>
              )}

              {scheduleType === 'Interval' && (
                <div>
                  <label className="block text-sm font-medium text-slate-300 mb-2">
                    Interval (seconds)
                  </label>
                  <input
                    type="number"
                    value={intervalSeconds}
                    onChange={(e) => setIntervalSeconds(parseInt(e.target.value))}
                    min="1"
                    className="input"
                  />
                  <p className="mt-1 text-xs text-slate-500">
                    Job will repeat every {intervalSeconds} seconds
                  </p>
                </div>
              )}

              {scheduleType === 'Once' && (
                <div>
                  <label className="block text-sm font-medium text-slate-300 mb-2">
                    Run At (optional)
                  </label>
                  <input
                    type="datetime-local"
                    value={runAt}
                    onChange={(e) => setRunAt(e.target.value)}
                    className="input"
                  />
                  <p className="mt-1 text-xs text-slate-500">
                    Leave empty to run immediately
                  </p>
                </div>
              )}
            </div>
          )}

          {/* Step 4: Options */}
          {step === 4 && (
            <div className="space-y-4">
              <div className="flex items-center justify-between p-3 bg-slate-700/50 rounded-lg">
                <div>
                  <div className="font-medium text-slate-50">Disallow Concurrent Execution</div>
                  <div className="text-sm text-slate-400">
                    Prevent this job from running multiple instances simultaneously
                  </div>
                </div>
                <input
                  type="checkbox"
                  checked={disallowConcurrent}
                  onChange={(e) => setDisallowConcurrent(e.target.checked)}
                  className="w-5 h-5 rounded border-slate-600 bg-slate-700 text-blue-500 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-300 mb-2">
                  Misfire Policy
                </label>
                <select
                  value={misfirePolicy}
                  onChange={(e) => setMisfirePolicy(e.target.value)}
                  className="input"
                >
                  {MISFIRE_POLICIES.map((policy) => (
                    <option key={policy.value} value={policy.value}>
                      {policy.label}
                    </option>
                  ))}
                </select>
                <p className="mt-1 text-xs text-slate-500">
                  How to handle missed executions
                </p>
              </div>

              <div className="mt-6 p-4 bg-slate-700/30 rounded-lg">
                <h4 className="font-medium text-slate-50 mb-2">Summary</h4>
                <div className="text-sm text-slate-400 space-y-1">
                  <p>Job Key: <span className="text-slate-50 font-mono">{jobKey}</span></p>
                  <p>Type: <span className="text-slate-50">{selectedJobType}</span></p>
                  <p>Schedule: <span className="text-slate-50">{scheduleType}</span></p>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex justify-between p-4 border-t border-slate-700">
          <button
            onClick={step === 1 ? onClose : handleBack}
            className="btn-secondary"
          >
            {step === 1 ? 'Cancel' : 'Back'}
          </button>
          
          {step < 4 ? (
            <button onClick={handleNext} className="btn-primary">
              Next
            </button>
          ) : (
            <button 
              onClick={handleSubmit} 
              disabled={createJob.isPending}
              className="btn-primary disabled:opacity-50"
            >
              {createJob.isPending ? 'Creating...' : 'Create Job'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default CreateJobModal;
