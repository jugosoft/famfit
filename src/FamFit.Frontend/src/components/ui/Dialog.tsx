import { HTMLAttributes } from 'react';
import { X } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface DialogProps extends HTMLAttributes<HTMLDivElement> {
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
  title?: string;
  description?: string;
  children?: React.ReactNode;
}

export function Dialog({
  className,
  open = true,
  onOpenChange,
  title,
  description,
  children,
  ...props
}: DialogProps) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/60"
        onClick={() => onOpenChange?.(false)}
      />

      {/* Content */}
      <div
        className={cn(
          'relative z-50 w-full max-w-lg rounded-lg border bg-card p-6 shadow-lg',
          className
        )}
        {...props}
      >
        <div className="flex items-center justify-between mb-4">
          <div>
            {title && <h2 className="text-lg font-semibold">{title}</h2>}
            {description && <p className="text-sm text-muted-foreground">{description}</p>}
          </div>
          <button
            className="rounded-sm opacity-70 hover:opacity-100 transition-opacity"
            onClick={() => onOpenChange?.(false)}
          >
            <X className="h-4 w-4" />
          </button>
        </div>
        {children}
      </div>
    </div>
  );
}
