/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
    ],
  theme: {
    extend: {
      fontFamily: {
        'sans': ['Inter', 'Poppins', 'system-ui', 'sans-serif'],
      },
      colors: {
        'catering': {
          'primary': '#FF6B35',     // Warm orange
          'primary-dark': '#E55A24', // Darker orange
          'secondary': '#FF8C42',   // Light orange
          'accent': '#FFB627',      // Gold accent
          'light': '#FFF8F3',       // Off-white
        }
      },
      spacing: {
        'section': '6rem',
      },
      borderRadius: {
        'xl': '20px',
        '2xl': '24px',
      },
      boxShadow: {
        'card': '0 4px 20px rgba(0, 0, 0, 0.08)',
        'card-hover': '0 12px 28px rgba(0, 0, 0, 0.12)',
        'input': '0 2px 8px rgba(0, 0, 0, 0.06)',
      },
      backgroundImage: {
        'gradient-catering': 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)',
        'gradient-catering-soft': 'linear-gradient(135deg, #FF8C42 0%, #FFB627 100%)',
      }
    },
  },
  plugins: [],
}

