/**
 * SignatureCapture Component
 * Canvas-based signature capture with save/clear
 */

import { useRef, useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { Pen, Trash2, Check } from 'lucide-react';

const SignatureCapture = ({
  label,
  onSave,
  width = 400,
  height = 200,
  strokeColor = '#000000',
  strokeWidth = 2,
  backgroundColor = '#ffffff',
  className = '',
}) => {
  const canvasRef = useRef(null);
  const [isDrawing, setIsDrawing] = useState(false);
  const [hasSignature, setHasSignature] = useState(false);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    ctx.fillStyle = backgroundColor;
    ctx.fillRect(0, 0, width, height);
  }, [width, height, backgroundColor]);

  const startDrawing = (e) => {
    const canvas = canvasRef.current;
    const rect = canvas.getBoundingClientRect();
    const ctx = canvas.getContext('2d');

    ctx.beginPath();
    ctx.moveTo(
      e.clientX - rect.left || e.touches[0].clientX - rect.left,
      e.clientY - rect.top || e.touches[0].clientY - rect.top
    );
    setIsDrawing(true);
    setHasSignature(true);
  };

  const draw = (e) => {
    if (!isDrawing) return;

    const canvas = canvasRef.current;
    const rect = canvas.getBoundingClientRect();
    const ctx = canvas.getContext('2d');

    ctx.lineTo(
      e.clientX - rect.left || e.touches[0].clientX - rect.left,
      e.clientY - rect.top || e.touches[0].clientY - rect.top
    );
    ctx.strokeStyle = strokeColor;
    ctx.lineWidth = strokeWidth;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.stroke();
  };

  const stopDrawing = () => {
    setIsDrawing(false);
  };

  const clearSignature = () => {
    const canvas = canvasRef.current;
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = backgroundColor;
    ctx.fillRect(0, 0, width, height);
    setHasSignature(false);
  };

  const saveSignature = () => {
    const canvas = canvasRef.current;
    const dataUrl = canvas.toDataURL('image/png');
    onSave?.(dataUrl);
  };

  return (
    <div className={`w-full ${className}`}>
      {/* Label */}
      {label && (
        <label className="block text-sm font-medium text-gray-700 mb-2">
          {label}
        </label>
      )}

      {/* Canvas */}
      <div className="border-2 border-gray-300 rounded-lg overflow-hidden bg-white">
        <canvas
          ref={canvasRef}
          width={width}
          height={height}
          onMouseDown={startDrawing}
          onMouseMove={draw}
          onMouseUp={stopDrawing}
          onMouseLeave={stopDrawing}
          onTouchStart={startDrawing}
          onTouchMove={draw}
          onTouchEnd={stopDrawing}
          className="touch-none cursor-crosshair"
          style={{ width: '100%', height: 'auto' }}
        />
      </div>

      {/* Actions */}
      <div className="flex gap-2 mt-3">
        <button
          type="button"
          onClick={clearSignature}
          disabled={!hasSignature}
          className="flex-1 flex items-center justify-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <Trash2 className="w-4 h-4" />
          Clear
        </button>

        <button
          type="button"
          onClick={saveSignature}
          disabled={!hasSignature}
          className="flex-1 flex items-center justify-center gap-2 px-4 py-2 border border-transparent rounded-lg text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <Check className="w-4 h-4" />
          Save Signature
        </button>
      </div>

      {/* Helper Text */}
      <p className="mt-2 text-xs text-gray-500 text-center">
        <Pen className="w-3 h-3 inline mr-1" />
        Sign within the box above
      </p>
    </div>
  );
};

SignatureCapture.propTypes = {
  label: PropTypes.string,
  onSave: PropTypes.func,
  width: PropTypes.number,
  height: PropTypes.number,
  strokeColor: PropTypes.string,
  strokeWidth: PropTypes.number,
  backgroundColor: PropTypes.string,
  className: PropTypes.string,
};

export default SignatureCapture;
