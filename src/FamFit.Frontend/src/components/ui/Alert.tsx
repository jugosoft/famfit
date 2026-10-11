import { cn } from '@/lib/utils';

interface AlertProps {
  variant?: 'default' | 'destructive' | 'success' | 'info';
  title?: string;
  description?: string;
  icon?: React.ReactNode;
  action?: React.ReactNode;
  className?: string;
  onClose?: () => void;
}

export function Alert({
  variant = 'default',
  title,
  description,
  icon,
  action,
  className,
  onClose
}: AlertProps) {
  return (
    <div
      className={cn(
        'relative rounded-lg border p-4',
        {
          default: 'border-border bg-card',
          destructive: 'border-destructive/50 text-destructive bg-destructive/5',
          success: 'border-green-500/50 text-green-700 bg-green-50 dark:bg-green-950/20',
          info: 'border-blue-500/50 text-blue-700 bg-blue-50 dark:bg-blue-950/20'
        }[variant],
        className
      )}
    >
      <div className="flex gap-3">
        {icon && <div className="shrink-0 mt-0.5">{icon}</div>}
        <div className="flex-1">
          {title && <h4 className="text-sm font-semibold">{title}</h4>}
          {description && (
            <p className="mt-1 text-sm opacity-90">{description}</p>
          )}
        </div>
        {onClose && (
          <button
            onClick={onClose}
            className="shrink-0 opacity-60 hover:opacity-100 transition-opacity"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        )}
      </div>
      {action && <div className="mt-3">{action}</div>}
    </div>
  );
}
