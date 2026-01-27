const LoadingSkeleton = ({ type = 'card', count = 1 }) => {
  const skeletons = {
    card: () => (
      <div className="bg-white rounded-lg border border-gray-200 p-6 animate-pulse">
        <div className="h-4 bg-gray-200 rounded w-1/4 mb-4"></div>
        <div className="h-8 bg-gray-200 rounded w-1/2 mb-4"></div>
        <div className="h-3 bg-gray-200 rounded w-3/4"></div>
      </div>
    ),

    table: () => (
      <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div className="animate-pulse">
          {/* Header */}
          <div className="bg-gray-50 px-6 py-3 border-b border-gray-200">
            <div className="flex space-x-4">
              {[1, 2, 3, 4].map((i) => (
                <div key={i} className="h-3 bg-gray-200 rounded flex-1"></div>
              ))}
            </div>
          </div>
          {/* Rows */}
          {[1, 2, 3, 4, 5].map((row) => (
            <div key={row} className="px-6 py-4 border-b border-gray-200">
              <div className="flex space-x-4">
                {[1, 2, 3, 4].map((i) => (
                  <div key={i} className="h-4 bg-gray-200 rounded flex-1"></div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>
    ),

    stat: () => (
      <div className="bg-white rounded-lg border border-gray-200 p-6 animate-pulse">
        <div className="flex items-center justify-between mb-4">
          <div className="h-4 bg-gray-200 rounded w-1/3"></div>
          <div className="w-10 h-10 bg-gray-200 rounded-lg"></div>
        </div>
        <div className="h-8 bg-gray-200 rounded w-1/2 mb-2"></div>
        <div className="h-3 bg-gray-200 rounded w-2/3"></div>
      </div>
    ),
  };

  const SkeletonComponent = skeletons[type];

  return (
    <>
      {Array.from({ length: count }).map((_, index) => (
        <div key={index}>
          <SkeletonComponent />
        </div>
      ))}
    </>
  );
};

export default LoadingSkeleton;
