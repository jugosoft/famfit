import { cn } from '@/lib/utils';

interface StepperProps {
  steps: { label: string; description?: string }[];
  currentStep: number;
  onComplete?: (step: number) => void;
  className?: string;
}

export function Stepper({ steps, currentStep, className }: StepperProps) {
  return (
    <div className={cn('flex items-center gap-2', className)}>
      {steps.map((step, index) => (
        <div key={index} className="flex items-center gap-2">
          <div
            className={cn(
              'flex h-8 w-8 items-center justify-center rounded-full text-xs font-medium transition-colors',
              index < currentStep
                ? 'bg-green-500 text-white'
                : index === currentStep
                ? 'bg-primary text-primary-foreground'
                : 'bg-muted text-muted-foreground'
            )}
          >
            {index < currentStep ? '✓' : index + 1}
          </div>
          <div className="hidden sm:block">
            <p className="text-xs font-medium">{step.label}</p>
            {step.description && (
              <p className="text-[10px] text-muted-foreground">{step.description}</p>
            )}
          </div>
          {index < steps.length - 1 && (
            <div className={cn('h-px w-6', index < currentStep ? 'bg-green-500' : 'bg-muted')} />
          )}
        </div>
      ))}
    </div>
  );
}
