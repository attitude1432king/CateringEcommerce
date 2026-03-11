/**
 * Device Fingerprinting Utility
 * Generates a unique device fingerprint based on browser and system characteristics
 * Used for trusted device tracking and 2FA
 */

/**
 * Generate a unique device fingerprint
 * @returns {Promise<string>} A unique fingerprint string
 */
export const generateDeviceFingerprint = async () => {
  try {
    const components = [];

    // 1. Screen Resolution
    components.push(`screen:${window.screen.width}x${window.screen.height}`);
    components.push(`avail:${window.screen.availWidth}x${window.screen.availHeight}`);
    components.push(`color:${window.screen.colorDepth}`);

    // 2. Timezone
    components.push(`tz:${Intl.DateTimeFormat().resolvedOptions().timeZone}`);
    components.push(`offset:${new Date().getTimezoneOffset()}`);

    // 3. Language
    components.push(`lang:${navigator.language}`);
    components.push(`langs:${navigator.languages?.join(',') || ''}`);

    // 4. Platform & User Agent
    components.push(`platform:${navigator.platform}`);
    components.push(`ua:${navigator.userAgent}`);

    // 5. Hardware Concurrency (CPU cores)
    components.push(`cores:${navigator.hardwareConcurrency || 'unknown'}`);

    // 6. Device Memory (if available)
    if (navigator.deviceMemory) {
      components.push(`memory:${navigator.deviceMemory}`);
    }

    // 7. Touch Support
    components.push(`touch:${navigator.maxTouchPoints || 0}`);

    // 8. Pixel Ratio
    components.push(`pixelRatio:${window.devicePixelRatio}`);

    // 9. Canvas Fingerprint (lightweight version)
    const canvasFingerprint = getCanvasFingerprint();
    components.push(`canvas:${canvasFingerprint}`);

    // 10. WebGL Fingerprint
    const webglFingerprint = getWebGLFingerprint();
    if (webglFingerprint) {
      components.push(`webgl:${webglFingerprint}`);
    }

    // 11. Plugins (deprecated in modern browsers, but still useful)
    const plugins = Array.from(navigator.plugins || [])
      .map(p => p.name)
      .sort()
      .join(',');
    if (plugins) {
      components.push(`plugins:${plugins}`);
    }

    // 12. Audio Context Fingerprint
    const audioFingerprint = await getAudioFingerprint();
    if (audioFingerprint) {
      components.push(`audio:${audioFingerprint}`);
    }

    // Combine all components and hash
    const fingerprintString = components.join('|');
    const fingerprint = await hashString(fingerprintString);

    return fingerprint;
  } catch (error) {
    console.error('Error generating device fingerprint:', error);
    // Fallback to a simpler fingerprint
    return await hashString(
      `${navigator.userAgent}|${window.screen.width}x${window.screen.height}|${navigator.language}|${new Date().getTimezoneOffset()}`
    );
  }
};

/**
 * Get Canvas fingerprint
 */
const getCanvasFingerprint = () => {
  try {
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    if (!ctx) return 'no-canvas';

    canvas.width = 200;
    canvas.height = 50;

    // Draw text
    ctx.textBaseline = 'top';
    ctx.font = '14px Arial';
    ctx.fillStyle = '#f60';
    ctx.fillRect(100, 10, 100, 30);
    ctx.fillStyle = '#069';
    ctx.fillText('Device Fingerprint 🔒', 10, 15);

    // Get canvas data
    const dataURL = canvas.toDataURL();
    return dataURL.substring(dataURL.length - 100); // Last 100 chars for uniqueness
  } catch (error) {
    return 'canvas-error';
  }
};

/**
 * Get WebGL fingerprint
 */
const getWebGLFingerprint = () => {
  try {
    const canvas = document.createElement('canvas');
    const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
    if (!gl) return null;

    const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
    if (!debugInfo) return null;

    const vendor = gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL);
    const renderer = gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL);

    return `${vendor}|${renderer}`.substring(0, 100);
  } catch (error) {
    return null;
  }
};

/**
 * Get Audio Context fingerprint
 */
const getAudioFingerprint = async () => {
  try {
    const AudioContext = window.AudioContext || window.webkitAudioContext;
    if (!AudioContext) return null;

    const context = new AudioContext();
    const oscillator = context.createOscillator();
    const analyser = context.createAnalyser();
    const gainNode = context.createGain();
    const scriptProcessor = context.createScriptProcessor(4096, 1, 1);

    gainNode.gain.value = 0; // Mute
    oscillator.connect(analyser);
    analyser.connect(scriptProcessor);
    scriptProcessor.connect(gainNode);
    gainNode.connect(context.destination);

    oscillator.start(0);

    return new Promise((resolve) => {
      scriptProcessor.onaudioprocess = function (event) {
        const output = event.outputBuffer.getChannelData(0);
        let sum = 0;
        for (let i = 0; i < output.length; i++) {
          sum += Math.abs(output[i]);
        }
        oscillator.stop();
        scriptProcessor.disconnect();
        context.close();
        resolve(sum.toString().substring(0, 20));
      };

      // Timeout after 100ms
      setTimeout(() => {
        try {
          oscillator.stop();
          scriptProcessor.disconnect();
          context.close();
        } catch (e) {}
        resolve(null);
      }, 100);
    });
  } catch (error) {
    return null;
  }
};

/**
 * Hash a string using SHA-256
 */
const hashString = async (str) => {
  try {
    // Use SubtleCrypto for hashing (modern browsers)
    const encoder = new TextEncoder();
    const data = encoder.encode(str);
    const hashBuffer = await crypto.subtle.digest('SHA-256', data);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
    return hashHex;
  } catch (error) {
    // Fallback to simple hash for older browsers
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32bit integer
    }
    return Math.abs(hash).toString(16);
  }
};

/**
 * Get device information (browser, OS) for display purposes
 */
export const getDeviceInfo = () => {
  const ua = navigator.userAgent;
  const platform = navigator.platform;

  // Detect browser
  let browser = 'Unknown';
  if (ua.indexOf('Firefox') > -1) browser = 'Firefox';
  else if (ua.indexOf('SamsungBrowser') > -1) browser = 'Samsung Internet';
  else if (ua.indexOf('Opera') > -1 || ua.indexOf('OPR') > -1) browser = 'Opera';
  else if (ua.indexOf('Trident') > -1) browser = 'Internet Explorer';
  else if (ua.indexOf('Edge') > -1) browser = 'Edge';
  else if (ua.indexOf('Edg') > -1) browser = 'Edge Chromium';
  else if (ua.indexOf('Chrome') > -1) browser = 'Chrome';
  else if (ua.indexOf('Safari') > -1) browser = 'Safari';

  // Detect OS
  let os = 'Unknown';
  if (platform.indexOf('Win') > -1) os = 'Windows';
  else if (platform.indexOf('Mac') > -1) os = 'macOS';
  else if (platform.indexOf('Linux') > -1) os = 'Linux';
  else if (ua.indexOf('Android') > -1) os = 'Android';
  else if (ua.indexOf('iOS') > -1 || ua.indexOf('iPhone') > -1 || ua.indexOf('iPad') > -1) os = 'iOS';

  // Device name
  const deviceName = `${browser} on ${os}`;

  return {
    browser,
    os,
    deviceName,
    userAgent: ua,
    platform: platform
  };
};

/**
 * Store device fingerprint in localStorage for consistency
 */
export const getStoredFingerprint = () => {
  return localStorage.getItem('device_fingerprint');
};

/**
 * Store device fingerprint in localStorage
 */
export const storeFingerprint = (fingerprint) => {
  localStorage.setItem('device_fingerprint', fingerprint);
  return fingerprint;
};

/**
 * Get or generate device fingerprint (with caching)
 */
export const getOrGenerateFingerprint = async () => {
  let fingerprint = getStoredFingerprint();

  if (!fingerprint) {
    fingerprint = await generateDeviceFingerprint();
    storeFingerprint(fingerprint);
  }

  return fingerprint;
};

/**
 * Clear stored fingerprint (useful for testing or logout)
 */
export const clearFingerprint = () => {
  localStorage.removeItem('device_fingerprint');
};

// Export default
export default {
  generateDeviceFingerprint,
  getDeviceInfo,
  getStoredFingerprint,
  storeFingerprint,
  getOrGenerateFingerprint,
  clearFingerprint
};
