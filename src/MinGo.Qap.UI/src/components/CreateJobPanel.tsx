/* eslint-disable react-hooks/set-state-in-effect */
import React, { useEffect, useState } from 'react';
import { X } from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import SlidePanel from './SlidePanel';
import { jobApi, manifestApi } from '../api';
import toast from 'react-hot-toast';
import type { CreateJobRequest, ScheduleDto, QuartzOptionsDto, JobTypeInfoDto, ParameterInfoDto, ScheduleType } from '../types';

interface CreateJobPanelProps {
  schedulerName: string;
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

const CreateJobPanel: React.FC<CreateJobPanelProps> = ({ schedulerName, isOpen, onClose }) => {
  const queryClient = useQueryClient();

  const { data: manifest } = useQuery({
    queryKey: ['manifest', schedulerName],
    queryFn: async () => {
      const response = await manifestApi.get(schedulerName);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data;
    },
    enabled: !!schedulerName,
  });

  // Existing jobs for template copy mode
  const { data: existingJobs } = useQuery({
    queryKey: ['existing-jobs', schedulerName],
    queryFn: async () => {
      const resp = await jobApi.getAll(schedulerName, 1, 1000);
      if (!resp.success) throw new Error(resp.errorMessage);
      return resp.data || [];
    },
    enabled: !!schedulerName,
  });

  const createJob = useMutation({
    mutationFn: (request: CreateJobRequest) => jobApi.create(schedulerName, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['jobs', schedulerName] });
    },
  });

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

  // Template selection (basic support)
  const [templateSource, setTemplateSource] = useState<'blank' | 'manifest' | 'copy'>('blank');
  const [selectedExistingTemplate, setSelectedExistingTemplate] = useState<string | undefined>(undefined);

  const selectedJob = manifest?.jobs?.find(j => j.key === selectedJobType);

  // Reset when opened
  useEffect(() => {
    if (isOpen) {
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
      setTemplateSource('blank');
      setSelectedExistingTemplate(undefined);
    }
  }, [isOpen]);

  const handleParamChange = (name: string, value: any) => {
    setParams(prev => ({ ...prev, [name]: value }));
  };

  const validateStep = () => {
    setError('');
    if (step === 1) {
      // Template handling: if user chose a template, prefill accordingly
      if (templateSource === 'manifest' && manifest?.jobs?.length) {
        // prefill if not selected
        if (!selectedJobType) setSelectedJobType(manifest.jobs[0].key);
      }
      if (!selectedJobType) {
        setError('Please select a job type');
        return false;
      }
      if (!jobKey.trim()) {
        setError('Please enter a job key');
        return false;
      }
      // If template copy from existing selected, no additional validation here
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
    if (validateStep()) setStep(prev => Math.min(prev + 1, 4));
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
      toast.success('Job created successfully!');
      onClose();
    } catch (err: any) {
      toast.error(err.message || 'Failed to create job');
      setError(err.message || 'Failed to create job');
    }
  };

  // Helper to reset template when switched
  const setSelectedJobTypeSafe = (val: string) => {
    setSelectedJobType(val);
  };

  if (!isOpen) return null;

  // Basic UI: reuse the same structure as modal, with a top template selector
  return (
    <SlidePanel isOpen={isOpen} onClose={onClose} title="Create Job" width="w-full max-w-2xl" footer={
      <div className="flex justify-between w-full">
        <button onClick={handleBack} className="btn-secondary">Back</button>
        {step < 4 ? (
          <button onClick={handleNext} className="btn-primary">Next</button>
        ) : (
          <button onClick={handleSubmit} disabled={createJob.isPending} className="btn-primary">
            {createJob.isPending ? 'Creating...' : 'Create Job'}
          </button>
        )}
      </div>
    }>
      {/* Step 0: Template selector */}
      <div className="mb-4">
        <div className="text-sm font-medium text-slate-300 mb-1">Template</div>
        <div className="flex items-center gap-2 flex-wrap">
          <button
            className={`px-3 py-1 rounded border ${templateSource === 'blank' ? 'border-blue-500 text-white bg-blue-500/10' : 'border-slate-700 text-slate-300'}`}
            onClick={() => setTemplateSource('blank')}
          >
            Blank
          </button>
          <button
            className={`px-3 py-1 rounded border ${templateSource === 'manifest' ? 'border-blue-500 text-white bg-blue-500/10' : 'border-slate-700 text-slate-300'}`}
            onClick={() => setTemplateSource('manifest')}
          >
            Templates (manifest)
          </button>
          <div className={`flex items-center ${templateSource === 'copy' ? '' : 'opacity-70'}`}>
            <span className="text-sm text-slate-300 mr-2">Copy from existing</span>
            <select
              className="input"
              value={templateSource === 'copy' ? (selectedExistingTemplate ?? '') : ''}
              onChange={(e) => {
                const val = e.target.value;
                if (val) {
                  setTemplateSource('copy');
                  setSelectedExistingTemplate(val);
                  setSelectedJobTypeSafe(val);
                }
              }}
              style={{ width: 180 }}
            >
              <option value="">Select existing...</option>
              {existingJobs?.map((j: any) => (
                <option key={j.jobKey} value={j.jobKey}>{j.jobKey}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Step 1: Select Job Type (including manifest templates or copy-from-existing) */}
      {templateSource === 'manifest' && (
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">Job Type</label>
            <div className="grid grid-cols-1 gap-2">
              {manifest?.jobs?.map((job: JobTypeInfoDto) => (
                <div
                  key={job.key}
                  onClick={() => setSelectedJobTypeSafe(job.key)}
                  className={`p-4 rounded-lg border cursor-pointer transition-colors ${
                    selectedJobType === job.key ? 'border-blue-500 bg-blue-500/10' : 'border-slate-700 hover:border-slate-600'
                  }`}
                >
                  <div className="font-medium text-slate-50">{job.key}</div>
                  <div className="text-sm text-slate-400">{job.description}</div>
                  <div className="text-xs text-slate-500 mt-1">{job.parameters.length} parameters</div>
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
            <p className="mt-1 text-xs text-slate-500">Unique identifier for this job instance</p>
          </div>
        </div>
      )}

      {templateSource === 'blank' && (
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">Job Type</label>
            <div className="grid grid-cols-1 gap-2">
              {manifest?.jobs?.map((job: JobTypeInfoDto) => (
                <div
                  key={job.key}
                  onClick={() => setSelectedJobTypeSafe(job.key)}
                  className={`p-4 rounded-lg border cursor-pointer transition-colors ${
                    selectedJobType === job.key ? 'border-blue-500 bg-blue-500/10' : 'border-slate-700 hover:border-slate-600'
                  }`}
                >
                  <div className="font-medium text-slate-50">{job.key}</div>
                  <div className="text-sm text-slate-400">{job.description}</div>
                  <div className="text-xs text-slate-500 mt-1">{job.parameters.length} parameters</div>
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
            <p className="mt-1 text-xs text-slate-500">Unique identifier for this job instance</p>
          </div>
        </div>
      )}

      {templateSource === 'copy' && (
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">Copy from existing</label>
            <select
              className="input"
              value={selectedExistingTemplate || ''}
              onChange={(e) => {
                const val = e.target.value;
                setSelectedExistingTemplate(val || undefined);
                if (val) setSelectedJobTypeSafe(val);
              }}
            >
              <option value="">Select existing...</option>
              {existingJobs?.map((j: any) => (
                <option key={j.jobKey} value={j.jobKey}>{j.jobKey}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">Job Key</label>
            <input
              type="text"
              value={jobKey}
              onChange={(e) => setJobKey(e.target.value)}
              placeholder="e.g., copied-job"
              className="input"
            />
          </div>
        </div>
      )}

      {/* Step 2: Configure Parameters (same as modal) */}
      {step >= 2 && selectedJob && (
        <div className="space-y-4 mt-4">
          <h3 className="text-lg font-medium text-slate-50 mb-4">Configure {selectedJob.key}</h3>
          {selectedJob.parameters.length === 0 ? (
            <p className="text-slate-400">No parameters required</p>
          ) : (
            selectedJob.parameters.map((param: ParameterInfoDto) => (
              <div key={param.name}>
                <label className="block text-sm font-medium text-slate-300 mb-2">{param.label || param.name}{param.required && <span className="text-red-500 ml-1">*</span>}</label>
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
                <p className="mt-1 text-xs text-slate-500">Type: {param.type} {param.default !== undefined && `(default: ${param.default})`}</p>
              </div>
            ))
          )}
        </div>
      )}

      {/* Step 3: Schedule */}
      {step === 3 && (
        <div className="space-y-4 mt-4">
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">Schedule Type</label>
            <div className="grid grid-cols-3 gap-2">
              {SCHEDULE_TYPES.map((type) => (
                <div key={type.value} onClick={() => setScheduleType(type.value as ScheduleType)} className={`p-3 rounded-lg border cursor-pointer text-center transition-colors ${scheduleType === type.value ? 'border-blue-500 bg-blue-500/10' : 'border-slate-700 hover:border-slate-600'}`}>
                  <div className="font-medium text-slate-50 text-sm">{type.label}</div>
                  <div className="text-xs text-slate-500">{type.description}</div>
                </div>
              ))}
            </div>
          </div>
          {scheduleType === 'Cron' && (
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">Cron Expression</label>
              <input type="text" value={cronExpression} onChange={(e) => setCronExpression(e.target.value)} placeholder="0 0 * * *" className="input font-mono" />
              <div className="mt-2 text-xs text-slate-500 space-y-1">
                <p>Examples:</p>
                <p>• <code className="bg-slate-700 px-1 rounded">0 0 * * *</code> - Daily at midnight</p>
                <p>• <code className="bg-slate-700 px-1 rounded">0 */6 * * *</code> - Every 6 hours</p>
              </div>
            </div>
          )}
          {scheduleType === 'Interval' && (
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">Interval (seconds)</label>
              <input type="number" value={intervalSeconds} onChange={(e) => setIntervalSeconds(parseInt(e.target.value))} min="1" className="input" />
              <p className="mt-1 text-xs text-slate-500">Job will repeat every {intervalSeconds} seconds</p>
            </div>
          )}
          {scheduleType === 'Once' && (
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">Run At (optional)</label>
              <input type="datetime-local" value={runAt} onChange={(e) => setRunAt(e.target.value)} className="input" />
              <p className="mt-1 text-xs text-slate-500">Leave empty to run immediately</p>
            </div>
          )}
        </div>
      )}

      {/* Step 4: Options */}
      {step === 4 && (
        <div className="space-y-4 mt-4">
          <div className="flex items-center justify-between p-3 bg-slate-700/50 rounded-lg">
            <div>
              <div className="font-medium text-slate-50">Disallow Concurrent Execution</div>
              <div className="text-sm text-slate-400">Prevent this job from running multiple instances simultaneously</div>
            </div>
            <input type="checkbox" checked={disallowConcurrent} onChange={(e) => setDisallowConcurrent(e.target.checked)} className="w-5 h-5 rounded border-slate-600 bg-slate-700 text-blue-500 focus:ring-blue-500" />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-2">Misfire Policy</label>
            <select value={misfirePolicy} onChange={(e) => setMisfirePolicy(e.target.value)} className="input">
              {MISFIRE_POLICIES.map((p) => (
                <option key={p.value} value={p.value}>{p.label}</option>
              ))}
            </select>
            <p className="mt-1 text-xs text-slate-500">How to handle missed executions</p>
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
    </SlidePanel>
  );
};

export default CreateJobPanel;
