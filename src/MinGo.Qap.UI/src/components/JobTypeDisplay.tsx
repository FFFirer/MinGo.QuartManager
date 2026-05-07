import React, { useState } from 'react';
import { Copy, Check } from 'lucide-react';

interface JobTypeDisplayProps {
  jobType: string;
  /** Max length before truncation with ellipsis in the middle (default: 60) */
  maxLength?: number;
}

/**
 * Displays a CLR type full name with:
 * - Namespace prefix in a muted color, class name prominent
 * - Hover tooltip with the full name
 * - Copy button to copy the full name to clipboard
 */
const JobTypeDisplay: React.FC<JobTypeDisplayProps> = ({ jobType, maxLength = 60 }) => {
  const [copied, setCopied] = useState(false);

  if (!jobType) {
    return <span className="text-slate-500 italic">unknown</span>;
  }

  // Split namespace and class name
  const lastDot = jobType.lastIndexOf('.');
  const namespace = lastDot > 0 ? jobType.slice(0, lastDot) : '';
  const className = lastDot > 0 ? jobType.slice(lastDot + 1) : jobType;

  // Truncate the full name for display if it exceeds maxLength
  const displayName = jobType.length > maxLength
    ? `${jobType.slice(0, Math.floor((maxLength - 3) / 2))}...${jobType.slice(-Math.ceil((maxLength - 3) / 2))}`
    : jobType;

  const handleCopy = async (e: React.MouseEvent) => {
    e.stopPropagation();
    try {
      await navigator.clipboard.writeText(jobType);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      // Fallback for environments where clipboard API is not available
      const textArea = document.createElement('textarea');
      textArea.value = jobType;
      document.body.appendChild(textArea);
      textArea.select();
      document.execCommand('copy');
      document.body.removeChild(textArea);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    }
  };

  return (
    <span className="inline-flex items-center gap-1.5 group relative">
      {/* Main display: namespace muted + class name prominent */}
      <span className="text-sm text-slate-50 truncate max-w-[200px]" title={jobType}>
        {namespace && (
          <span className="text-slate-500">{namespace}.</span>
        )}
        <span className="text-slate-50 font-medium">{className}</span>
      </span>

      {/* Copy button */}
      <button
        onClick={handleCopy}
        className="p-1 rounded text-slate-500 hover:text-slate-300 hover:bg-slate-700/50 opacity-0 group-hover:opacity-100 transition-all shrink-0"
        title="Copy full type name"
      >
        {copied ? <Check size={12} className="text-green-400" /> : <Copy size={12} />}
      </button>

      {/* Tooltip on hover */}
      <div className="absolute bottom-full left-0 mb-2 px-2 py-1 bg-slate-700 text-slate-200 text-xs rounded shadow-lg whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none z-50 max-w-md overflow-hidden text-ellipsis">
        {displayName}
      </div>
    </span>
  );
};

export default JobTypeDisplay;
