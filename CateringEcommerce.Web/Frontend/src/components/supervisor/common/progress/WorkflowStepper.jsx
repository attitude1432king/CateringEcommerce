/**
 * WorkflowStepper Component
 * Displays workflow progress with steps
 */

import PropTypes from 'prop-types';
import { Check } from 'lucide-react';

const WorkflowStepper = ({
  steps,
  currentStep,
  className = '',
}) => {
  return (
    <div className={`w-full ${className}`}>
      <div className="flex items-center justify-between">
        {steps.map((step, index) => {
          const stepNumber = index + 1;
          const isCompleted = stepNumber < currentStep;
          const isCurrent = stepNumber === currentStep;
          const isPending = stepNumber > currentStep;

          return (
            <div key={index} className="flex-1 flex items-center">
              {/* Step Circle */}
              <div className="flex flex-col items-center">
                <div
                  className={`
                    w-10 h-10 rounded-full flex items-center justify-center text-sm font-semibold
                    ${
                      isCompleted
                        ? 'bg-green-500 text-white'
                        : isCurrent
                        ? 'bg-blue-500 text-white'
                        : 'bg-gray-200 text-gray-500'
                    }
                  `}
                >
                  {isCompleted ? <Check className="w-5 h-5" /> : stepNumber}
                </div>

                {/* Step Label */}
                <div
                  className={`
                    mt-2 text-xs text-center max-w-[100px]
                    ${isCurrent ? 'font-semibold text-blue-600' : 'text-gray-600'}
                  `}
                >
                  {typeof step === 'string' ? step : step.label}
                </div>
              </div>

              {/* Connector Line */}
              {index < steps.length - 1 && (
                <div
                  className={`
                    flex-1 h-0.5 mx-2
                    ${isCompleted ? 'bg-green-500' : 'bg-gray-200'}
                  `}
                />
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
};

WorkflowStepper.propTypes = {
  steps: PropTypes.arrayOf(
    PropTypes.oneOfType([
      PropTypes.string,
      PropTypes.shape({
        label: PropTypes.string.isRequired,
        stage: PropTypes.string,
      }),
    ])
  ).isRequired,
  currentStep: PropTypes.number.isRequired,
  className: PropTypes.string,
};

export default WorkflowStepper;
