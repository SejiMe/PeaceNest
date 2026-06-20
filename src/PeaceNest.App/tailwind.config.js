/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./{app,components,features,hooks,lib}/**/*.{js,jsx,ts,tsx}'],
  presets: [require('nativewind/preset')],
  theme: {
    extend: {
      colors: {
        peacenest: {
          background: '#FFF9F5',
          surface: '#FFFFFF',
          'soft-rose': '#F6C7C8',
          rose: '#D97C83',
          blush: '#FBE7E5',
          gold: '#D6A84F',
          'gold-light': '#FFF1C7',
          sage: '#A8BFA3',
          clay: '#B8755A',
          charcoal: '#2F2A28',
          muted: '#8A7D78',
          border: '#EFE2DC',
          danger: '#D96B6B',
        },
      },
    },
  },
  plugins: [],
};
