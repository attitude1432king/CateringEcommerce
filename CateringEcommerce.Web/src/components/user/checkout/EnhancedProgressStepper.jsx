import React from 'react';

const EnhancedProgressStepper = ({ steps, currentStep, className = '' }) => {
  return (
    <div className={`bg-white rounded-xl shadow-sm border border-gray-200 p-6 ${className}`}>
      {/* Mobile: Vertical Progress */}
      <div className="md:hidden">
        <div className="space-y-4">
          {steps.map((step, index) => {
            const stepNumber = step.number || index + 1;
            const isActive = currentStep === stepNumber;
            const isCompleted = currentStep > stepNumber;
            const isFuture = currentStep < stepNumber;

            return (
              <div key={stepNumber} className="flex items-start">
                {/* Step Indicator */}
                <div className="flex flex-col items-center mr-4">
                  <div
                    className={`w-10 h-10 rounded-full flex items-center justify-center font-semibold transition-all duration-300 ${
                      isCompleted
                        ? 'bg-green-500 text-white shadow-md'
                        : isActive
                        ? 'bg-red-500 text-white shadow-lg ring-4 ring-red-100'
                        : 'bg-gray-200 text-neutral-500'
                    }`}
                  >
                    {isCompleted ? (
                      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M5 13l4 4L19 7" />
                      </svg>
                    ) : (
                      <span className="text-lg">{step.icon || stepNumber}</span>
                    )}
                  </div>
                  {index < steps.length - 1 && (
                    <div
                      className={`w-0.5 h-12 mt-2 transition-colors ${
                        isCompleted ? 'bg-green-500' : 'bg-gray-200'
                      }`}
                    />
                  )}
                </div>

                {/* Step Content */}
                <div className="flex-1 pt-2">
                  <div
                    className={`font-semibold transition-colors ${
                      isActive ? 'text-red-600' : isCompleted ? 'text-green-600' : 'text-gray-400'
                    }`}
                  >
                    {step.name}
                  </div>
                  {step.description && (
                    <div className="text-sm text-neutral-500 mt-1">{step.description}</div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Desktop: Horizontal Progress */}
      <div className="hidden md:block">
        <div className="relative flex items-center justify-between">
          {steps.map((step, index) => {
            const stepNumber = step.number || index + 1;
            const isActive = currentStep === stepNumber;
            const isCompleted = currentStep > stepNumber;
            const isFuture = currentStep < stepNumber;

            return (
              <React.Fragment key={stepNumber}>
                {/* Step */}
                <div className="flex flex-col items-center flex-1 relative z-10">
                  {/* Step Circle */}
                  <div
                    className={`w-14 h-14 rounded-full flex items-center justify-center font-bold text-xl transition-all duration-300 ${
                      isCompleted
                        ? 'bg-green-500 text-white shadow-lg transform scale-110'
                        : isActive
                        ? 'bg-gradient-to-br from-red-500 to-red-600 text-white shadow-xl ring-4 ring-red-100 transform scale-110'
                        : 'bg-gray-200 text-neutral-500'
                    }`}
                  >
                    {isCompleted ? (
                      <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M5 13l4 4L19 7" />
                      </svg>
                    ) : (
                      <span>{step.icon || stepNumber}</span>
                    )}
                  </div>

                  {/* Step Label */}
                  <div className="mt-3 text-center">
                    <div
                      className={`font-semibold text-sm transition-colors ${
                        isActive
                          ? 'text-red-600'
                          : isCompleted
                          ? 'text-green-600'
                          : 'text-gray-400'
                      }`}
                    >
                      {step.name}
                    </div>
                    {step.description && (
                      <div className="text-xs text-neutral-500 mt-1 max-w-[120px]">
                        {step.description}
                      </div>
                    )}
                  </div>

                  {/* Status Badge */}
                  {isActive && (
                    <div className="mt-2 px-3 py-1 bg-red-100 text-red-700 text-xs font-semibold rounded-full">
                      Current
                    </div>
                  )}
                  {isCompleted && (
                    <div className="mt-2 px-3 py-1 bg-green-100 text-green-700 text-xs font-semibold rounded-full">
                      Completed
                    </div>
                  )}
                </div>

                {/* Connector Line */}
                {index < steps.length - 1 && (
                  <div className="flex-1 flex items-center" style={{ marginTop: '-60px' }}>
                    <div className="w-full relative">
                      {/* Background Line */}
                      <div className="h-1 bg-gray-200 rounded-full"></div>
                      {/* Progress Line */}
                      <div
                        className={`absolute top-0 left-0 h-1 rounded-full transition-all duration-500 ${
                          isCompleted ? 'w-full bg-green-500' : 'w-0 bg-red-500'
                        }`}
                      ></div>
                    </div>
                  </div>
                )}
              </React.Fragment>
            );
          })}
        </div>

        {/* Progress Percentage */}
        <div className="mt-6 pt-4 border-t border-gray-200">
          <div className="flex items-center justify-between text-sm text-neutral-600">
            <span>Progress</span>
            <span className="font-semibold text-red-600">
              {Math.round(((currentStep - 1) / (steps.length - 1)) * 100)}% Complete
            </span>
          </div>
          <div className="mt-2 h-2 bg-gray-200 rounded-full overflow-hidden">
            <div
              className="h-full bg-gradient-to-r from-red-500 to-orange-500 rounded-full transition-all duration-500"
              style={{ width: `${((currentStep - 1) / (steps.length - 1)) * 100}%` }}
            ></div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default EnhancedProgressStepper;
