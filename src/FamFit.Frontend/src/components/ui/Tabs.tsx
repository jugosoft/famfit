import { TabsHTMLAttributes, forwardRef, useState } from 'react';
import { cn } from '@/lib/utils';

interface TabItem {
  value: string;
  label: string;
}

interface TabsProps extends TabsHTMLAttributes<HTMLDivElement> {
  items: TabItem[];
  value?: string;
  onValueChange?: (value: string) => void;
}

export function Tabs({ items, value, onValueChange, className }: TabsProps) {
  return (
    <div className={cn('w-full', className)}>
      <div className="flex border-b border-border">
        {items.map((item) => (
          <button
            key={item.value}
            className={cn(
              'px-4 py-2 text-sm font-medium border-b-2 transition-colors',
              value === item.value
                ? 'border-primary text-primary'
                : 'border-transparent text-muted-foreground hover:text-foreground'
            )}
            onClick={() => onValueChange?.(item.value)}
          >
            {item.label}
          </button>
        ))}
      </div>
    </div>
  );
}
