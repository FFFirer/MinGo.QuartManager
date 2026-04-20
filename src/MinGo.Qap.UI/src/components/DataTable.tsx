import React from 'react';

interface Column<T> {
  header: string;
  accessor: keyof T | ((row: T) => React.ReactNode);
  width?: string | number;
  align?: 'left' | 'center' | 'right';
  format?: (value: any) => React.ReactNode;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  loading?: boolean;
  emptyMessage?: string;
  onRowClick?: (row: T) => void;
  showBorder?: boolean;
  showHeader?: boolean;
  className?: string;
}

const DataTable: React.FC<DataTableProps<any>> = ({
  columns,
  data,
  loading = false,
  emptyMessage = 'No data available',
  onRowClick,
  showBorder = true,
  showHeader = true,
  className = '',
}) => {
  if (loading) {
    return (
      <div className="text-center py-8 text-slate-400">
        Loading...
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className="text-center py-8 text-slate-400">
        {emptyMessage}
      </div>
    );
  }

  return (
    <div className={`${showBorder ? 'border border-slate-700' : ''} ${className}`}>
      {showHeader && (
        <div className="bg-slate-700">
          <div className="flex flex-wrap">
            {columns.map((column, index) => {
              const width = column.width 
                ? typeof column.width === 'number' 
                  ? `w-${column.width}` 
                  : column.width 
                : 'w-auto';
              
              const align = column.align === 'center' 
                ? 'text-center' 
                : column.align === 'right' 
                  ? 'text-right' 
                  : 'text-left';
                  
              return (
                <div key={index} className={`${width} ${align} px-3 py-2 text-xs font-medium text-slate-400 uppercase tracking-wider`}>
                  {column.header}
                </div>
              );
            })}
          </div>
        </div>
      )}
      
      <div className="divide-y divide-slate-700">
        {data.map((row, rowIndex) => (
          <div 
            key={rowIndex} 
            className={`hover:bg-slate-800/50 cursor-pointer ${onRowClick ? 'cursor-pointer' : ''}`}
            onClick={() => onRowClick && onRowClick(row)}
          >
            <div className="flex flex-wrap">
              {columns.map((column, colIndex) => {
                const width = column.width 
                  ? typeof column.width === 'number' 
                    ? `w-${column.width}` 
                    : column.width 
                  : 'w-auto';
                
                const align = column.align === 'center' 
                  ? 'text-center' 
                  : column.align === 'right' 
                    ? 'text-right' 
                    : 'text-left';
                
                // Get the value from the row using the accessor
                let value: any;
                if (typeof column.accessor === 'function') {
                  value = column.accessor(row);
                } else {
                  value = row[column.accessor];
                }
                
                // Apply formatting if provided
                let formattedValue: any = value;
                if (column.format) {
                  formattedValue = column.format(value);
                }
                
                return (
                  <div 
                    key={colIndex} 
                    className={`${width} ${align} px-3 py-2 text-sm text-slate-300 whitespace-nowrap`}
                  >
                    {formattedValue}
                  </div>
                );
              })}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default DataTable;