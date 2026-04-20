import React from 'react';

interface LayoutWrapperProps {
  children: React.ReactNode;
  className?: string;
}

const LayoutWrapper: React.FC<LayoutWrapperProps> = ({ children, className = '' }) => {
  return (
    <div className={`min-h-screen flex flex-col ${className}`}>
      <div className="flex-1 overflow-y-auto p-6">
        {children}
      </div>
    </div>
  );
};

export default LayoutWrapper;